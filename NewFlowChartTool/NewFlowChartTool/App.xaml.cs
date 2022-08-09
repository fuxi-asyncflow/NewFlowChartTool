using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using FlowChartCommon;
using NFCT.Common.Views;
using NFCT.Common.ViewModels;
using NFCT.Graph.ViewModels;
using Prism.Ioc;
using Prism.Unity;
using Prism.Events;
using NFCT.Graph.Views;

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
            Logger.FCLogger.Info("application startup");

            //var test = Application.Current.TryFindResource(SystemColors.WindowBrushKey);
            //Console.WriteLine(test);

            return Container.Resolve<Views.MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<NodeAutoCompleteViewModel>();
            containerRegistry.RegisterSingleton<NodeAutoComplete>();
            
            containerRegistry.RegisterSingleton<SimpleEditBoxViewModel>();
            containerRegistry.RegisterSingleton<SimpleEditBox>();


            //throw new NotImplementedException();
        }

        //private void Application_Startup(object sender, StartupEventArgs e)
        //{
            

        //}
    }
}
