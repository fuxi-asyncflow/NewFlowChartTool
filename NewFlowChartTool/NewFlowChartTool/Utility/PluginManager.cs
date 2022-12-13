using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Common;

namespace NewFlowChartTool.Utility
{
    public class PluginManager: IPluginManager
    {
        static PluginManager()
        {
            Inst = new PluginManager();
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

            throw new Exception("crash report test");
        }
        public static PluginManager Inst { get; set; }
        public System.UnhandledExceptionEventHandler? UnhandledExceptionHandler { get; private set; }

        public void SetUnhandledExceptionHandler(System.UnhandledExceptionEventHandler handler)
        {
            UnhandledExceptionHandler = handler;
        }
    }
}
