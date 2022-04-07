﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using FlowChartCommon;
using Prism.Ioc;
using Prism.Unity;

namespace NewFlowChartTool
{


    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();

        protected override Window CreateShell()
        {
#if DEBUG
            AllocConsole();
#endif
            FlowChartCommon.Logger.FCLogger.Info("application startup");

            return Container.Resolve<Views.MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //throw new NotImplementedException();
        }

        //private void Application_Startup(object sender, StartupEventArgs e)
        //{
            

        //}
    }
}
