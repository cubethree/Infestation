using System;
using System.Configuration;

namespace Infestation.VirtualParadise.Configuration
{
    public static class GameSettings
    {
        public static string BlockModel
        {
            get { return ConfigurationManager.AppSettings["blockModel"]; }
        }
        public static int BlockOwner
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["blockOwner"]); }
        }

        public static int DistributionInterval
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["distributionInterval"]); }
        }

        public static int PersistenseInterval
        {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["persistenceInterval"]); }
        }

        public static string WebOverlayServerBinding
        {
            get { return ConfigurationManager.AppSettings["webOverlayServerBinding"]; }
        }

        public static string WebOverlayServerUrl
        {
            get { return ConfigurationManager.AppSettings["webOverlayServerUrl"]; }
        }

        public static string WebOverlayUrl
        {
            get { return ConfigurationManager.AppSettings["webOverlayUrl"]; }
        }
        public static bool TraditionalGameplay
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["traditionalGameplay"]); }
        }
    }
}
