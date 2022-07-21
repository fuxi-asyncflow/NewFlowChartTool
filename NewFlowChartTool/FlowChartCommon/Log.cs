using NLog;

namespace FlowChartCommon
{
    public class Logger
    {
        static Logger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "file.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole")
            {
                Layout = "${longdate}|${level:uppercase=true} ${message:withexception=true}"
            };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
            FCLogger = NLog.LogManager.GetCurrentClassLogger();
        }

        public static NLog.Logger FCLogger;

        public static void LOG(string msg)
        {
            FCLogger.Info(msg);
        }

        public static void ERR(string msg)
        {
            FCLogger.Error(msg);
        }

        public static void DBG(string msg)
        {
            FCLogger.Debug(msg);
        }

        public static void WARN(string msg)
        {
            FCLogger.Warn(msg);
        }
    }

}