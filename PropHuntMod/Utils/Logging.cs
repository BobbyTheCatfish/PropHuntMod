using SSMP.Logging;
using System;

namespace PropHuntMod.Utils
{
    internal static class Log
    {
        static ILogger logger;
        public static void SetLogger(ILogger log)
        {
            if (logger == null)
            {
                logger = log;

#if DEBUG
                FilteredLogs.API.ApplyFilter(ShouldLog);
#endif
            }
        }

        private static bool ShouldLog(BepInEx.Logging.LogEventArgs log)
        {
            //Debug.Log(log.Data);
            //return true;
            if (log.Source.SourceName == "SSMP" && log.Data is string data)
            {
                if (data.StartsWith("[PropHuntMod")) return true;
            }

            return false;
        }

        public static void LogInfo(params object[] data)
        {
            foreach (object obj in data)
                logger.Info(obj.ToString());
        }
        public static void LogWarning(params object[] data)
        {
            foreach (object obj in data)
                logger.Warn(obj.ToString());
        }
        public static void LogError(params object[] data)
        {
            foreach (object obj in data)
                logger.Error(obj.ToString());
        }
        public static void LogFatal(params object[] data)
        {
            foreach (object obj in data)
                logger.Error(obj.ToString());
        }
        public static void LogDebug(params object[] data)
        {
            foreach (object obj in data)
                logger.Debug(obj.ToString());
        }
        public static void LogMessage(params object[] data)
        {
            foreach (object obj in data)
                logger.Message(obj.ToString());
        }
    }
}
