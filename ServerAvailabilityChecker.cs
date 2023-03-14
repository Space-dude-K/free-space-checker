using FreeSpaceChecker.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FreeSpaceChecker
{
    class ServerAvailabilityChecker
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverIpOrDnsName"></param>
        /// <returns></returns>
        public bool CheckServer(string serverIpOrDnsName, ILogger logger)
        {
            bool result = false;
            PingReply pingReply;

            using (var ping = new Ping())
            {
                try
                {
                    pingReply = ping.Send(serverIpOrDnsName);
                    result = pingReply.Status == IPStatus.Success;
                }
                catch (Exception ex)
                {
                    logger.Log(serverIpOrDnsName + " -> " + ex.Message);
                }
            }

            return result;
        }
    }
}