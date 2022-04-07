// See https://aka.ms/new-console-template for more information

using System;
using NLog;

namespace FlowChartTest // Note: actual namespace depends on the project name.
{
    class Logger
    {
        static Logger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "file.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole") { 
                Layout = "${longdate}|${level:uppercase=true} ${message:withexception=true}"
            };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
            MyLogger = NLog.LogManager.GetCurrentClassLogger();
        }

        public static NLog.Logger MyLogger ;
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Logger.MyLogger.Info("hello");

            var p = new FlowChart.Core.Project(new ProjectFactory.TestProjectFactory());
            p.Path = @"D:\git\asyncflow_new\test\flowchart";
            p.Load();
        }
    }
}





