using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropHuntMod.Utils
{
    internal static class Log
    {
        static ManualLogSource logger;
        public static void SetLogger(ManualLogSource log)
        {
            logger = log;
        }

        public static void LogInfo(object data)
        {
            logger.LogInfo(data);
        }
        public static void LogWarning(object data)
        {
            logger.LogWarning(data);
        }
        public static void LogError(object data)
        {
            logger.LogError(data);
        }
        public static void LogFatal(object data)
        {
            logger.LogFatal(data);
        }
        public static void LogDebug(object data)
        {
            logger.LogDebug(data);
        }
        public static void LogMessage(object data)
        {
            logger.LogMessage(data);
        }
    }
}
