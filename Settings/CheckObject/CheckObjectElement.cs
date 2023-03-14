using System;
using System.Configuration;

namespace FreeSpaceChecker.Settings.CheckObject
{
    class CheckObjectElement : ConfigurationElement
    {
        #region Configuration Properties
        [ConfigurationProperty("ip", IsRequired = true)]
        public string ObjectIp
        {
            get { return (string)this["ip"]; }
            set { this["ip"] = value; }
        }
        [ConfigurationProperty("disks", IsRequired = false)]
        public string ObjectDisks
        {
            get { return CheckObject((string)this["disks"]); }
            set { this["disks"] = value; }
        }
        private string CheckObject(string rawStr)
        {
            if(rawStr.Contains("="))
            {
                if(rawStr.Remove(0, 2).StartsWith(@"\"))
                {
                    return rawStr;
                }
                else
                {
                    Console.Write("return2" + rawStr);
                    return rawStr.Insert(2, @"\");
                }
            }
            else
            {
                return rawStr;
            }
        }
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion
        #region Constructors
        public CheckObjectElement()
        {
        }
        #endregion
    }
}
