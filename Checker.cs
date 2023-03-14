using check_up_money.Cypher;
using FreeSpaceChecker.Interfaces;
using FreeSpaceChecker.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSpaceChecker
{
    class Checker
    {
        private string loggerPath;

        private ILogger logger;
        ICypher encryptor;
        private IMailSender sender;
        private ISpaceChecker checker;
        private List<Comp> comps;
        private (string smtpServer, string mailFrom) smtpSettings;
        private List<Mail> mails;
        RequisiteInformation requisiteInformation;
        private enum DiskSizeFormatType
        {
            Bytes = 0,
            KiloBytes = 1,
            MegaBytes = 2,
            GigaBytes = 3,
            TeraBytes = 4
        }
        public Checker()
        {
            Configurator conf = new Configurator();

            loggerPath = conf.GetLoggerPath() + "FreeSpaceChecker" + "_" + DateTime.Now.Year.ToString() + ".txt";

            var req = conf.LoadAdminSettings();
            encryptor = new Encryptor();
            CheckRequisites(req, conf);
            logger = new Logger(loggerPath);
            sender = new MailSender(logger);
            checker = new FreeSpaceChecker();
            comps = conf.LoadCompSettings();
            smtpSettings = conf.LoadSmtpSettings();
            mails = conf.LoadMailSettings();
            //Console.ReadLine();
            PerfomCheck();
        }
        private void CheckRequisites((string admLogin, string loginSalt, string admPass, string passSalt) req, Configurator conf)
        {
            if(string.IsNullOrEmpty(req.admLogin) || string.IsNullOrEmpty(req.admPass))
            {
                Console.WriteLine("Не задан пароль или логин. Введите логин и пароль через пробел: ");

                while (true)
                {
                    var loginAdnPass = (Console.ReadLine().Split());

                    (string admLogin, string admPass) newReq = (loginAdnPass[0], loginAdnPass[1]);
                    Console.WriteLine("Raw: " + newReq.admLogin + " " + newReq.admPass);

                    var enc = encryptor.Encrypt(encryptor.ToSecureString(loginAdnPass[0]), encryptor.ToSecureString(loginAdnPass[1]));
                    requisiteInformation = enc;
                    newReq.admLogin = encryptor.ToInsecureString(enc.User);
                    newReq.admPass = encryptor.ToInsecureString(enc.Password);
                    Console.WriteLine("Enc: " + newReq.admLogin + " " + newReq.admPass);

                    conf.SaveAdminSettings((encryptor.ToInsecureString(enc.User), enc.USalt, encryptor.ToInsecureString(enc.Password), enc.PSalt));

                    if (Console.ReadKey().Key == ConsoleKey.Enter)
                        break;
                }
            }
            else
            {
                Console.WriteLine("Login and password - OK");

                requisiteInformation = new RequisiteInformation(encryptor.ToSecureString(req.admLogin), req.loginSalt, encryptor.ToSecureString(req.admPass), req.passSalt);

                //var dec = encryptor.Decrypt(encryptor.ToSecureString(req.admLogin), req.loginSalt, encryptor.ToSecureString(req.admPass), req.passSalt);
                //Console.WriteLine("Dec req: " + encryptor.ToInsecureString(dec.User) + " Pass: " + encryptor.ToInsecureString(dec.Password));
            }
        }
        /// <summary>
        /// Запуск проверки
        /// </summary>
        public void PerfomCheck()
        {
            int lowSpaceAndAvailabilityCounter = 0;

            ServerAvailabilityChecker sac = new ServerAvailabilityChecker();

            logger.Log("Perfoming daily checking.");

            //var dec = encryptor.Decrypt(requisiteInformation.User, requisiteInformation.USalt, requisiteInformation.Password, requisiteInformation.PSalt);
            //Console.WriteLine("Dec req: " + encryptor.ToInsecureString(dec.User) + " Pass: " + encryptor.ToInsecureString(dec.Password));
            //Console.ReadLine();


            List<Tuple<bool, string, string, string, string, string>> msgBlob = new List<Tuple<bool, string, string, string, string, string>>();

            foreach(Comp comp in comps)
            {
                bool serverAvailability = sac.CheckServer(comp.Ip, logger);

                // Check server availability
                if (serverAvailability)
                {
                    foreach (Tuple<string, ulong> diskAndTreshold in GetDiskAndTreshold(comp.Disks))
                    {
                        string drive = diskAndTreshold.Item1;
                        Tuple<ulong, ulong> freeAndTotalSpace = checker.CheckSpace(comp.Ip, drive, logger, requisiteInformation, encryptor, drive.Contains("=") ? true : false);
                        ulong treshold = diskAndTreshold.Item2;
                        string formTreshold = FormatSizeBytes(treshold);
                        ulong space = freeAndTotalSpace.Item1;
                        double freeSpace = Math.Round((double)freeAndTotalSpace.Item1 * 100 / (double)freeAndTotalSpace.Item2, 2);
                        string freeSpaceProcentage = freeSpace.ToString() + "%";
                        string size = FormatSizeBytes(space);

                        if (freeSpace == 0.0)
                        {
                            logger.Log("Server: " + comp.Ip.PadRight(15)
                                + " " + drive.PadRight(15) + ": " + size.PadRight(10)
                                + " Free %: " + freeSpaceProcentage.PadLeft(6)
                                + " Treshold: " + formTreshold.PadLeft(8)
                                + " << Server unavailable!");

                            msgBlob.Add(new Tuple<bool, string, string, string, string, string>(serverAvailability, comp.Ip, drive, "NO RESPONSE", "NO RESPONSE", formTreshold));
                        }
                        else if (treshold > space)
                        {
                            logger.Log("Server: " + comp.Ip.PadRight(15)
                                + " " + drive.PadRight(15) + ": " + size.PadRight(10)
                                + " Free %: " + freeSpaceProcentage.PadLeft(6)
                                + " Treshold: " + formTreshold.PadLeft(8)
                                + " << Low space!");

                            msgBlob.Add(new Tuple<bool, string, string, string, string, string>(serverAvailability, comp.Ip, drive, size, freeSpaceProcentage, formTreshold));

                            lowSpaceAndAvailabilityCounter++;
                        }
                        else
                        {
                            logger.Log("Server: " + comp.Ip.PadRight(15)
                                + " " + drive.PadRight(15)
                                + ": " + size.PadRight(10)
                                + " Free %: " + freeSpaceProcentage.PadLeft(6)
                                + " Treshold: " + formTreshold.PadLeft(8));
                        }
                    }
                }
                else
                {
                    // Server offline
                    logger.Log("Server: " + comp.Ip.PadRight(10)
                                + " " + "NONE".PadRight(15) + ": " + "NONE".PadRight(10)
                                + " Free %: " + "NONE".PadLeft(6)
                                + " Treshold: " + "NONE".PadRight(9)
                                + " << OFFLINE!");
                    msgBlob.Add(new Tuple<bool, string, string, string, string, string>(serverAvailability, comp.Ip, "OFFLINE", "OFFLINE", "OFFLINE", "OFFLINE"));

                    lowSpaceAndAvailabilityCounter++;
                }
            }

            logger.Log(string.Empty, true);

            if(lowSpaceAndAvailabilityCounter != 0)
            {
                foreach (Mail mail in mails)
                {
                    sender.SendEmail(MailComposer(msgBlob), mail.Subject, mail.Email, smtpSettings.smtpServer, smtpSettings.mailFrom);
                }
            }
        }
        /// <summary>
        /// Компоновщик. Создает xml-шаблон с таблицей. Create xml-template for table.
        /// </summary>
        /// <param name="msgData">Подготовленные данные. Data: Ip - disk - size - free size in % - treshold</param>
        /// <returns></returns>
        private string MailComposer(List<Tuple<bool, string, string, string, string, string>> msgData)
        {
            StringBuilder result = new StringBuilder();

            result.Append("<head><style>table, td, th { border: 1px solid black; width: 480px; }</style></head>");
            result.Append("<h1>Attention! Low space on:</h1>");
            result.Append("<table><tr><th>Server</th><th>Disk</th><th>Free space</th><th>Free %</th><th>Treshold</th></tr>");

            foreach(Tuple<bool, string, string, string, string, string> data in msgData)
            {
                if (data.Item1)
                {
                    result.Append(
                    "<tr><td style='text-align:center'>" + data.Item2
                    + "</td><td style='text-align:center'>" + data.Item3 + ":"
                    + "</td><td style='text-align:right'>" + data.Item4
                    + "</td><td style='text-align:right'>" + data.Item5
                    + "</td><td style='text-align:center'>" + data.Item6
                    + "</td></tr>");
                }
                else
                {
                    result.Append(
                    "<tr><td bgcolor='#FF0000' style='text-align:center'>" + data.Item2
                    + "</td><td style='text-align:center'>" + data.Item3
                    + "</td><td style='text-align:center'>" + data.Item4
                    + "</td><td style='text-align:center'>" + data.Item5
                    + "</td><td style='text-align:center'>" + data.Item6
                    + "</td></tr>");
                }
            }
            
            result.Append("</table>");
            result.Append("<h2><a href=" + "file:///" + @"\\G600-SRWORK\FreeSpaceChecker\logs\" + System.IO.Path.GetFileName(loggerPath) + ">Log file link</a></h2>");
            result.Append("<h3><a href=" + "file:///" + @"\\G600-SRWORK\FreeSpaceChecker\FreeSpaceChecker.exe.config" + ">Edit settings</a></h3>");

            return result.ToString();
        }
        /// <summary>
        /// Получаем disk и treshold со строки настроек. Recieve disk and treshold from setting string.
        /// </summary>
        /// <param name="confData">Строка с настроек. Settings string</param>
        /// <returns>Набор диск - лимит. Disk - treshold (bytes)</returns>
        private List<Tuple<string, ulong>> GetDiskAndTreshold(string confData)
        {
            List<Tuple<string, ulong>> result = new List<Tuple<string,ulong>>();
            ulong GBtoBytes = 1024L * 1024L * 1024L;

            // If shares
            if(!confData.Contains("="))
            {
                foreach (string dataGroup in confData.Split(','))
                {
                    Match match = new Regex(@"(?<disk>\b[A-Za-z]\b)-(?<treshold>[0-9]+)").Match(dataGroup);

                    result.Add(new Tuple<string, ulong>(match.Groups["disk"].Value, ulong.Parse(match.Groups["treshold"].Value) * GBtoBytes));
                }
            }
            else
            {
                Console.WriteLine("SHARES " + confData);

                foreach(string dataGroup in confData.Split(','))
                {
                    Match match = new Regex(@"(?<diskWithShare>(?<disk>\b[A-Za-z]\b)=(?<share>(\\[A-Za-z]+)+))-(?<treshold>\d+)").Match(dataGroup);

                    result.Add(new Tuple<string, ulong>(match.Groups["diskWithShare"].Value, ulong.Parse(match.Groups["treshold"].Value) * GBtoBytes));
                }
            }

            return result;
        }
        /// <summary>
        /// Format raw bytes to Kb, Mb, Gb, Tb
        /// </summary>
        /// <param name="dlBytes">Raw bytes</param>
        /// <returns>Formatted string</returns>
        private string FormatSizeBytes(ulong dlBytes)
        {
            if (dlBytes < 1048576)
            {
                return FormatSize(dlBytes, DiskSizeFormatType.KiloBytes).ToString("F").PadRight(6) + " Kb";
            }
            else if (dlBytes >= 1048576 && dlBytes < 1073741824)
            {
                return FormatSize(dlBytes, DiskSizeFormatType.MegaBytes).ToString("F").PadRight(6) + " Mb";
            }
            else if (dlBytes >= 1073741824 && dlBytes < 1099511627776)
            {
                return FormatSize(dlBytes, DiskSizeFormatType.GigaBytes).ToString("F").PadRight(6) + " Gb"; 
            }
            else
            {
                return FormatSize(dlBytes, DiskSizeFormatType.TeraBytes).ToString("F").PadRight(6) + " Tb"; 
            }
        }
        /// <summary>
        /// Get size for specific type
        /// </summary>
        /// <param name="freeBytes">Raw bytes</param>
        /// <param name="type">Format type</param>
        /// <returns>Formatted bytes</returns>
        private decimal FormatSize(ulong freeBytes, DiskSizeFormatType type)
        {
            decimal formatedSizeFree;

            formatedSizeFree = (decimal)(freeBytes / Math.Pow(1024, (int)type));

            return formatedSizeFree;
        }
    }
}