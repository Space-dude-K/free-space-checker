using FreeSpaceChecker.Interfaces;
using System;
using System.Net.NetworkInformation;

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
                    pingReply = ping.Send(serverIpOrDnsName, 10000);
                    result = pingReply.Status == IPStatus.Success;
                }
                catch (System.Net.NetworkInformation.PingException ex)
                {
                    throw;
                    logger.Log(serverIpOrDnsName + " -> " + ex.Message);
                }
                catch (Exception ex)
                {
                    throw;
                    logger.Log(serverIpOrDnsName + " -> " + ex.Message);
                }
            }

            return result;
        }
    }
}