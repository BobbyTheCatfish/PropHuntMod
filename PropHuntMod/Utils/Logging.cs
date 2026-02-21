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

#if DEBUG
            FilteredLogs.API.ApplyFilter("Prop Hunt");
#endif
        }

        public static void LogInfo(params object[] data)
        {
            foreach (object obj in data)
                logger.LogInfo(obj);
        }
        public static void LogWarning(params object[] data)
        {
            foreach (object obj in data)
                logger.LogWarning(obj);
        }
        public static void LogError(params object[] data)
        {
            foreach (object obj in data)
                logger.LogError(obj);
        }
        public static void LogFatal(params object[] data)
        {
            foreach (object obj in data)
                logger.LogFatal(obj);
        }
        public static void LogDebug(params object[] data)
        {
            foreach (object obj in data)
                logger.LogDebug(obj);
        }
        public static void LogMessage(params object[] data)
        {
            foreach (object obj in data)
                logger.LogMessage(obj);
        }
    }
}
