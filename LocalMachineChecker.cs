using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FreeSpaceChecker
{
    class LocalMachineChecker
    {
        static string IPHost;

        public static bool IsMachineLocal(string hostName)
        {
            IPHostEntry local;

            if (isValidIp(hostName))
            {
                // ip
                local = Dns.GetHostEntry(hostName);
            }
            else
            {
                // dns
                string hostNameDns = Dns.GetHostName();
                local = Dns.GetHostEntry(hostName);
            }

            foreach (IPAddress ipaddress in local.AddressList)
            {
                IPHost = ipaddress.ToString();
            }

            if (isLocal(IPHost))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private static bool isValidIp(string host)
        {
            IPAddress ipAddress = null;

            return IPAddress.TryParse(host, out ipAddress);
        }
        private static bool isLocal(string host)
        {
            try
            {
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    // is local address
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }
    }
}