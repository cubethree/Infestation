using System;
using System.Configuration;

namespace Infestation.VirtualParadise.Configuration
{
    public static class LoginSettings
    {
        public static string ServerAddress
        {
            get { return ConfigurationManager.AppSettings["serverAddress"]; }
        }

        public static ushort ServerPort
        {
            get { return Convert.ToUInt16(ConfigurationManager.AppSettings["serverPort"]); }
        }

        public static string Username
        {
            get { return ConfigurationManager.AppSettings["username"]; }
        }

        public static string Password
        {
            get { return ConfigurationManager.AppSettings["password"]; }
        }

        public static string World
        {
            get { return ConfigurationManager.AppSettings["world"]; }
        }
    }
}
