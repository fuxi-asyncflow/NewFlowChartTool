using System.Diagnostics;
using NLog;
using NLog.Conditions;
using NLog.Targets;

namespace FlowChart.Common
{
    public class Logger
    {
        static Logger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "log.txt", MaxArchiveFiles = 5, ArchiveNumbering = ArchiveNumberingMode.Sequence, ArchiveAboveSize = 10000000 };
            //var logconsole = new NLog.Targets.ConsoleTarget("logconsole")
            var logconsole = new NLog.Targets.ColoredConsoleTarget("logconsole")
            {
                Layout = "${longdate}|${level:uppercase=true} ${message:withexception=true}"
            };
            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule((ConditionExpression)("level == LogLevel.Debug")
                , ConsoleOutputColor.DarkGray, ConsoleOutputColor.NoChange));
            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule((ConditionExpression)("level == LogLevel.Info")
                , ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange));
            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule((ConditionExpression)("level == LogLevel.Warn")
                , ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange));
            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule((ConditionExpression)("level == LogLevel.Error")
                , ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));
            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule((ConditionExpression)("level == LogLevel.Debug")
                , ConsoleOutputColor.Red, ConsoleOutputColor.White));

            // Rules for mapping loggers to targets            
#if DEBUG
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
#else
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
#endif
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
            OnErrEvent?.Invoke(msg);
        }

        [Conditional("DEBUG")]
        public static void DBG(string msg)
        {
            FCLogger.Debug(msg);
        }

        public static void WARN(string msg)
        {
            FCLogger.Warn(msg);
            OnWarnEvent?.Invoke(msg);
        }

        public static event Action<string>? OnErrEvent;
        public static event Action<string>? OnWarnEvent;
    }

}