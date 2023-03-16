using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Common;
using FlowChart.Common.Report;
using FlowChart.LuaCodeGen;
using FlowChart.Parser;
using FlowChart.Plugin;

namespace NewFlowChartTool.Utility
{
    public class PluginManager: IPluginManager
    {
        static PluginManager()
        {
            Inst = new PluginManager();
            CodeGenFactory.Register("lua", typeof(LuaCodeGenerator));
            CodeGenFactory.Register("python", typeof(PyCodeGenerator));
        }

        public void LoadPlugins()
        {
            var exeFolder = new DirectoryInfo(FileHelper.GetExeFolder());
            var pluginFolder = new DirectoryInfo(System.IO.Path.Combine(exeFolder.FullName, "plugins"));
            if (!pluginFolder.Exists)
            {
                Logger.WARN("[plugin] no plugin folder");
                return;
            }

            var pluginInterface = typeof(IPlugin);

            foreach (var dllFile in pluginFolder.EnumerateFiles("*.dll"))
            {
                var  dll = Assembly.LoadFrom(dllFile.FullName);
                foreach (var type in dll.GetTypes())
                {
                    if (type.GetInterfaces().Contains(pluginInterface))
                    {
                        Logger.LOG($"find plugin {type.FullName}");
                        var pluginInstance = System.Activator.CreateInstance(type) as IPlugin;
                        if (pluginInstance == null)
                        {
                            Logger.WARN($"create plugin {type.FullName} failed!");
                            continue;
                        }

                        if (!pluginInstance.Register(this))
                        {
                            Logger.WARN($"register plugin {type.FullName} failed!");
                        }
                        else
                        {
                            Logger.LOG($"register plugin {type.FullName} success!");
                        }
                    }
                }
            }

            //throw new Exception("crash report test");
        }

        public string GetGitVersion()
        {
            var asm = typeof(PluginManager).Assembly;
            var attrs = asm.GetCustomAttributes<AssemblyMetadataAttribute>();
            var gitHash = attrs.FirstOrDefault(a => a.Key == "GitHash")?.Value;
            if (gitHash == null)
                return "unkown-git-version";
            return gitHash;
        }

        public string GetBuildTime()
        {
            var asm = typeof(PluginManager).Assembly;
            var attrs = asm.GetCustomAttributes<AssemblyMetadataAttribute>();
            var v = attrs.FirstOrDefault(a => a.Key == "BuildTime")?.Value;
            if (v == null)
                return "unkown-build-time";
            return v;
        }

        public static PluginManager Inst { get; set; }

        #region ExceptionHandler
        public System.UnhandledExceptionEventHandler? UnhandledExceptionHandler { get; private set; }

        public void SetUnhandledExceptionHandler(System.UnhandledExceptionEventHandler handler)
        {
            UnhandledExceptionHandler = handler;
        }
        #endregion

        #region Reporter

        public delegate void ReporterDelegate(ReporterEvent ev);

        public event ReporterDelegate? ReporterEvent;

        public void Report(ReporterEvent ev)
        {
            ReporterEvent?.Invoke(ev);
        }

        public void SetReporterHandler(ReporterDelegate handler)
        {
            ReporterEvent += handler;
        }

        #endregion


    }
}
