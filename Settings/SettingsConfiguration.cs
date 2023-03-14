using FreeSpaceChecker.Settings.CheckObject;
using FreeSpaceChecker.Settings.Email;
using System.Configuration;

namespace FreeSpaceChecker.Settings
{
    class SettingsConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("objectsToCheck", IsDefaultCollection = false)]
        public CheckObjectElementCollection CheckObjects
        {
            get { return ((CheckObjectElementCollection)(base["objectsToCheck"])); }
        }

        [ConfigurationProperty("emails", IsDefaultCollection = false)]
        public EmailElementCollection Emails
        {
            get { return ((EmailElementCollection)(base["emails"])); }
        }
        [ConfigurationProperty("adminLogin", IsDefaultCollection = false)]
        public string AdminLogin
        {
            get { return (string)this["adminLogin"]; }
            set { this["adminLogin"] = value; }
        }
        [ConfigurationProperty("loginSalt", IsDefaultCollection = false)]
        public string LoginSalt
        {
            get { return (string)this["loginSalt"]; }
            set { this["loginSalt"] = value; }
        }
        [ConfigurationProperty("adminPass", IsDefaultCollection = false)]
        public string AdminPass
        {
            get { return (string)this["adminPass"]; }
            set { this["adminPass"] = value; }
        }
        [ConfigurationProperty("passSalt", IsDefaultCollection = false)]
        public string PassSalt
        {
            get { return (string)this["passSalt"]; }
            set { this["passSalt"] = value; }
        }
    }
}