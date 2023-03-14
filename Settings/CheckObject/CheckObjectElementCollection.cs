using System;
using System.Configuration;

namespace FreeSpaceChecker.Settings.CheckObject
{
    [ConfigurationCollection(typeof(CheckObjectElement), AddItemName = "comp", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    class CheckObjectElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CheckObjectElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("IpData");

            return ((CheckObjectElement)element).ObjectIp;
            //return element;
        }
        [ConfigurationProperty("loggerPath", IsDefaultCollection = false)]
        public string Loggerpath
        {
            get { return (string)this["loggerPath"]; }
            set { this["loggerPath"] = value; }
        }
    }
}
