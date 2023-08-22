using FreeSpaceChecker.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace FreeSpaceChecker
{
    class TotalSpaceChecker
    {
        private enum DiskSizeFormatType
        {
            Bytes = 0,
            KiloBytes = 1,
            MegaBytes = 2,
            GigaBytes = 3,
            TeraBytes = 4
        }
        /// <summary>
        /// Format raw bytes to Kb, Mb, Gb, Tb
        /// </summary>
        /// <param name="dlBytes">Raw bytes</param>
        /// <returns>Formatted string</returns>
        private string FormatSizeBytes(object dlBytesStr)
        {
            ulong dlBytes = ulong.Parse(dlBytesStr.ToString());

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
        public void CheckTotalSpace1(string ip, string region)
        {
            var machine = ip;

            var options = new ConnectionOptions { Username = "", Password = "&U" };

            var scope = new ManagementScope(@"\\" + machine + @"\root\cimv2", options);

            var queryString = "select Name, Size, FreeSpace from Win32_LogicalDisk where DriveType=3";
            var query = new ObjectQuery(queryString);

            var worker = new ManagementObjectSearcher(scope, query);

            var results = worker.Get();

            ulong totalFreeSpace = 0;
            ulong totalLogicalSpace = 0;

            using (StreamWriter writetext =
                new StreamWriter(@"C:\Users\mgl15.GKMOGILEV\Desktop\Проекты\Soft GU\FreeSpaceChecker\FreeSpaceChecker\Settings\totalSpaceGu.txt", true))
            {
                writetext.WriteLine("[" + region + "]".PadRight(16 - region.Length) + ip);
            }

            foreach (ManagementObject item in results)
            {
                //Console.WriteLine("{3} - {0} {2} {1}", item["Name"], item["FreeSpace"], item["Size"], region);
                WriteToFile(item["Name"].ToString(), FormatSizeBytes(item["FreeSpace"]), FormatSizeBytes(item["Size"]));

                totalFreeSpace += ulong.Parse(item["FreeSpace"].ToString());
                totalLogicalSpace += ulong.Parse(item["Size"].ToString());
            }

            using (StreamWriter writetext =
                new StreamWriter(@"C:\Users\mgl15.GKMOGILEV\Desktop\Проекты\Soft GU\FreeSpaceChecker\FreeSpaceChecker\Settings\totalSpaceGu.txt", true))
            {
                writetext.WriteLine("Total: " + Environment.NewLine + FormatSizeBytes((object)totalFreeSpace) + @" \ " + FormatSizeBytes((object)totalLogicalSpace));
            }
        }
        private void WriteToFile(string name,string freeSpace, string size)
        {
            using (StreamWriter writetext = 
                new StreamWriter(@"C:\Users\mgl15.GKMOGILEV\Desktop\Проекты\Soft GU\FreeSpaceChecker\FreeSpaceChecker\Settings\totalSpaceGu.txt", true))
            {
                writetext.WriteLine(name + "  " + freeSpace + @" / " + size);
            }
        }
        public void CheckTotalSpace(string ip, string region)
        {
            try
            {
                Console.WriteLine("=======================================");
                Console.WriteLine("Data for " + ip + " " + region);

                string ComputerName = ip;
                ManagementScope Scope;
                Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", ComputerName), null);

                Scope.Connect();
                ObjectQuery Query = new ObjectQuery("SELECT * FROM Win32_PnPSignedDriver");
                ManagementObjectSearcher Searcher = new ManagementObjectSearcher(Scope, Query);

                foreach (ManagementObject WmiObject in Searcher.Get())
                {
                    Console.WriteLine("{0,-35} {1,-40}", "ClassGuid", WmiObject["ClassGuid"]);// String
                    Console.WriteLine("{0,-35} {1,-40}", "DeviceClass", WmiObject["DeviceClass"]);// String
                    Console.WriteLine("{0,-35} {1,-40}", "DeviceID", WmiObject["DeviceID"]);// String
                    Console.WriteLine("{0,-35} {1,-40}", "DeviceName", WmiObject["DeviceName"]);// String
                    Console.WriteLine("{0,-35} {1,-40}", "Manufacturer", WmiObject["Manufacturer"]);// String
                    Console.WriteLine("{0,-35} {1,-40}", "Name", WmiObject["Name"]);// String
                    Console.WriteLine("{0,-35} {1,-40}", "Status", WmiObject["Status"]);// String

                }

                Console.WriteLine("=======================================");
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Exception {0} Trace {1}", e.Message, e.StackTrace));
            }
        }
        public Tuple<ulong, ulong> CheckSpace(string ip, string diskOrShare, ILogger logger, bool isShare = false)
        {
            if (isShare)
            {
                return GetSpaceForShare(ip, diskOrShare, logger);
            }
            else
            {
                return GetSpaceForDisk(ip, diskOrShare, logger, isShare);
            }
        }
        private Tuple<ulong, ulong> GetSpaceForDisk(string ip, string diskOrShare, ILogger logger, bool isShare)
        {
            ulong calculatedSpace = 0;
            ulong calculatedCapacity = 0;

            ManagementPath path = new ManagementPath()
            {
                NamespacePath = @"root\cimv2",
                Server = ip
            };

            ConnectionOptions con = new ConnectionOptions();
            con.Username = "G600-Administrator";
            con.Password = "ncgbg6&U";

            ManagementScope scope = null;

            // Remote or local destination check
            try
            {
                if (!LocalMachineChecker.IsMachineLocal(ip))
                {
                    scope = new ManagementScope(path, con);
                }
                else
                {
                    scope = new ManagementScope();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            string condition = "DriveLetter = '" + diskOrShare + ":'";
            string[] selectedProperties = new string[] { "FreeSpace", "Capacity" };
            SelectQuery query = new SelectQuery("Win32_Volume", condition, selectedProperties);

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                using (ManagementObjectCollection results = searcher.Get())
                {
                    ManagementObject volume = results.Cast<ManagementObject>().SingleOrDefault();

                    if (volume != null)
                    {
                        ulong freeSpace = (ulong)volume.GetPropertyValue("FreeSpace");
                        ulong capacity = (ulong)volume.GetPropertyValue("Capacity");

                        calculatedSpace = freeSpace;
                        calculatedCapacity = capacity;
                        Console.WriteLine("Space: " + freeSpace + " Capacity: " + capacity);
                    }
                    else
                    {
                        Console.WriteLine("Volume " + ip + " " + diskOrShare + " is null.");
                    }
                }
            }
            catch (System.Management.ManagementException ex)
            {
                Console.WriteLine(ip + " " + diskOrShare + " " + ex.Message);
            }
            catch (Exception ex)
            {
                logger.Log("Errors with server " + ip + " " + diskOrShare + " Message: " + ex.Message);
            }

            return new Tuple<ulong, ulong>(calculatedSpace, calculatedCapacity);
        }
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);
        public Tuple<ulong, ulong> GetSpaceForShare(string ip, string share, ILogger logger)
        {
            ulong FreeBytesAvailable;
            ulong TotalNumberOfBytes;
            ulong TotalNumberOfFreeBytes;

            bool success = GetDiskFreeSpaceEx(@"\\" + ip + share.Remove(0, 2),
                                              out FreeBytesAvailable,
                                              out TotalNumberOfBytes,
                                              out TotalNumberOfFreeBytes);
            if (!success)
            {
                logger.Log("Network error! IP: " + ip + " Share: " + share);
            }
            return new Tuple<ulong, ulong>(FreeBytesAvailable, TotalNumberOfBytes);
        }
    }
}
