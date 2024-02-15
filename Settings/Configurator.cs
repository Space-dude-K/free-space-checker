﻿using check_up_money.Cypher;
using FreeSpaceChecker.Settings.CheckObject;
using FreeSpaceChecker.Settings.Email;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace FreeSpaceChecker.Settings
{
    class Configurator
    {
        enum SectionTypes
        {
            Comp,
            Mail
        }
        public Configurator()
        {
        }
        #region User Helpers Comps, Emails
        private Configuration LoadConfig()
        {
            #if DEBUG
            string applicationName =
                Environment.GetCommandLineArgs()[0];
            #else
                string applicationName =
                    Environment.GetCommandLineArgs()[0];
                    //Environment.GetCommandLineArgs()[0]+ ".exe";
            #endif

            string exePath = System.IO.Path.Combine(Environment.CurrentDirectory, applicationName);

            Console.WriteLine("Opening conf -> " + exePath);

            // Get the configuration file. The file name has
            // this format appname.exe.config.
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(exePath);

            return config;
        }
        public (string admLogin, string loginSalt, string admPass, string passSalt) LoadAdminSettings()
        {
            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            return (myConfig.AdminLogin, myConfig.LoginSalt, myConfig.AdminPass, myConfig.PassSalt);
        }
        public void SaveAdminSettings((string admLogin, string loginSalt, string admPass, string passSalt) req)
        {
            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            myConfig.AdminLogin = req.admLogin;
            myConfig.LoginSalt = req.loginSalt;
            myConfig.AdminPass = req.admPass;
            myConfig.PassSalt = req.passSalt;

            myConfig.CurrentConfiguration.Save();
        }
        public List<Comp> LoadCompSettings()
        {
            List<Comp> comps = new List<Comp>();

            SettingsConfiguration myConfig = (SettingsConfiguration)ConfigurationManager.GetSection("settings");

            foreach(CheckObjectElement compSetting in myConfig.CheckObjects)
            {
                Comp comp = new Comp();
                comp.Ip = compSetting.ObjectIp;

                if (compSetting.ObjectDisks != string.Empty)
                {
                    comp.Disks = compSetting.ObjectDisks;
                }
                else
                {
                    throw new Exception("Null setting exception.");
                }
                    
                comps.Add(comp);
            }

            return comps;
        }
        public void SaveSmtpReqSettings(string mailLogin, string mailLoginSalt, string mailPassword, string mailPasswordSalt)
        {
            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            myConfig.Emails.MailLogin = mailLogin;
            myConfig.Emails.MailLoginSalt = mailLoginSalt;
            myConfig.Emails.MailPassword = mailPassword;
            myConfig.Emails.MailPasswordSalt = mailPasswordSalt;

            myConfig.CurrentConfiguration.Save();
        }
        public (bool sendEmail, string smtpServer, string mailFrom, string mailLogin, string mailLoginSalt, string mailPassword, string mailPasswordSalt) LoadSmtpSettings()
        {
            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            bool sendEmail = false;
            bool.TryParse(myConfig.Emails.SendEmail, out sendEmail);

            return (sendEmail, myConfig.Emails.SmtpServer, myConfig.Emails.MailFrom,
                myConfig.Emails.MailLogin, myConfig.Emails.MailLoginSalt, myConfig.Emails.MailPassword, myConfig.Emails.MailPasswordSalt);
        }
        public List<Mail> LoadMailSettings()
        {
            List<Mail> emails = new List<Mail>();

            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            foreach (EmailElement mailSetting in myConfig.Emails)
            {
                Mail mail = new Mail();
                mail.Email = mailSetting.Email;
                mail.Subject = mailSetting.Subject;

                emails.Add(mail);
            }

            return emails;
        }
        public string GetLoggerPath()
        {
            Configuration config = LoadConfig();
            SettingsConfiguration myConfig = config.GetSection("settings") as SettingsConfiguration;

            string cfgPath;

            if (!string.IsNullOrEmpty(myConfig.CheckObjects.Loggerpath) && Directory.Exists(myConfig.CheckObjects.Loggerpath))
                cfgPath = myConfig.CheckObjects.Loggerpath;
            else
            {
                cfgPath = Environment.CurrentDirectory;
            }

            return cfgPath;
        }
        #endregion
    }
}