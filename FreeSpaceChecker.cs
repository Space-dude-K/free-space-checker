using check_up_money.Cypher;
using FreeSpaceChecker.Interfaces;
using System;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace FreeSpaceChecker
{
    class FreeSpaceChecker : ISpaceChecker
    {
        public Tuple<ulong, ulong> CheckSpace(string ip, string diskOrShare, ILogger logger, RequisiteInformation req, ICypher cypher, bool isShare = false)
        {
            if (isShare)
            {
                return GetSpaceForShare(ip, diskOrShare, logger);
            }
            else
            {
                return GetSpaceForDisk(ip, diskOrShare, logger, req, cypher, isShare);
            }
        }
        private Tuple<ulong, ulong> GetSpaceForDisk(string ip, string diskOrShare, ILogger logger, RequisiteInformation req, ICypher cypher, bool isShare)
        {
            ulong calculatedSpace = 0;
            ulong calculatedCapacity = 0;

            ManagementPath path = new ManagementPath()
            {
                NamespacePath = @"root\cimv2",
                Server = ip
            };

            ConnectionOptions con = new ConnectionOptions();

            var dec = cypher.Decrypt(req.User, req.USalt, req.Password, req.PSalt);
            con.Username = cypher.ToInsecureString(dec.User);
            con.Password = cypher.ToInsecureString(dec.Password);

            ManagementScope scope = null;

            // Remote or local destination check
            try
            {
                if(!LocalMachineChecker.IsMachineLocal(ip))
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

            string condition = "DriveLetter = '" + diskOrShare +":'";
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
            catch (System.ArgumentException ex)
            {
                Console.WriteLine($"Проверьте, что в логине указан домен. {ex.Message}");
            }
            catch (System.Management.ManagementException ex)
            {
                Console.WriteLine(ip + " " + diskOrShare + " " + ex.Message);
            }
            catch(Exception ex)
            {
                logger.Log("Errors with server " + ip + " " + diskOrShare + " Message: " + ex.Message);
                throw;
            }
 
            return new Tuple<ulong,ulong>(calculatedSpace, calculatedCapacity);
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
            return new Tuple<ulong,ulong>(FreeBytesAvailable, TotalNumberOfBytes);
        }
    }
}
