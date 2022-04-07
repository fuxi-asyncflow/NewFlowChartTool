using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using FlowChartCommon;

namespace NewFlowChartTool
{


    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            
#if DEBUG
            AllocConsole();
#endif
            FlowChartCommon.Logger.FCLogger.Info("application startup");
        }
    }
}
