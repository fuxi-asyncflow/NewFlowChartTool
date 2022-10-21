using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FlowChart.Misc;
using FlowChartCommon;
using NewFlowChartTool.ViewModels;
using NewFlowChartTool.Views;
using NFCT.Common.Services;
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
            EventManager.RegisterClassHandler(
                typeof(UIElement),
                Keyboard.PreviewGotKeyboardFocusEvent,
                (KeyboardFocusChangedEventHandler)OnPreviewGotKeyboardFocus);

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

            containerRegistry.RegisterSingleton<IDebugService, DebugDialogViewModel>();
            containerRegistry.RegisterDialog<DebugDialog, DebugDialogViewModel>(DebugDialogViewModel.NAME);

            //containerRegistry.RegisterSingleton<IDebugService, DebugDialogViewModel>();
            containerRegistry.RegisterDialog<TypeDialog, TypeDialogViewModel>(TypeDialogViewModel.NAME);

            containerRegistry.RegisterSingleton<IOutputMessage, OutputPanelViewModel>();

            //throw new NotImplementedException();
        }

        private void OnPreviewGotKeyboardFocus(object sender,
            KeyboardFocusChangedEventArgs e)
        {
            //Console.WriteLine($"keyboard focus changed : {sender} {e}");
        }

        //private void Application_Startup(object sender, StartupEventArgs e)
        //{


        //}
    }
}
