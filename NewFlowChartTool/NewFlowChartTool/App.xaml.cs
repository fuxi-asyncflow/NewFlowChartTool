using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FlowChart.Common;
using FlowChart.Lua;
using FlowChart.Misc;
using NewFlowChartTool.Utility;
using NewFlowChartTool.ViewModels;
using NewFlowChartTool.Views;
using NewFlowChartTool.Views.Dialogs;
using NFCT.Common.Services;
using NFCT.Common.Views;
using NFCT.Common.ViewModels;
using NFCT.Graph.ViewModels;
using Prism.Ioc;
using Prism.Unity;
using Prism.Events;
using NFCT.Graph.Views;
using NFCT.Diff.ViewModels;


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
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(MyExceptionHandler);
            Logger.FCLogger.Info("application startup");

            var lua = Lua.Inst;

            //var test = Application.Current.TryFindResource(SystemColors.WindowBrushKey);
            //Console.WriteLine(test);
            if (!Directory.Exists("tmp"))
            {
                Directory.CreateDirectory("tmp");
            }
            return Container.Resolve<Views.MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<NodeAutoCompleteViewModel>();
            containerRegistry.RegisterSingleton<NodeAutoComplete>();
            
            containerRegistry.RegisterSingleton<SimpleEditBoxViewModel>();
            containerRegistry.RegisterSingleton<SimpleEditBox>();

            containerRegistry.RegisterSingleton<DebugDialogViewModel>();

            containerRegistry.RegisterSingleton<IDebugService, DebugDialogViewModel>();
            containerRegistry.RegisterDialog<DebugDialog, DebugDialogViewModel>(DebugDialogViewModel.NAME);

            containerRegistry.RegisterDialog<DocumentationDialog, DocumentationDialogViewModel>(DocumentationDialogViewModel.NAME);

            //containerRegistry.RegisterSingleton<IDebugService, DebugDialogViewModel>();
            containerRegistry.RegisterDialog<TypeDialog, TypeDialogViewModel>(TypeDialogViewModel.NAME);
            containerRegistry.RegisterDialog<InputDialog, InputDialogViewModel>(InputDialogViewModel.NAME);
            containerRegistry.RegisterDialog<GraphRootConfigDialog, GraphRootConfigDialogViewModel>(GraphRootConfigDialogViewModel.NAME);

            containerRegistry.RegisterSingleton<OutputPanelViewModel>();
            containerRegistry.RegisterSingleton<IOutputMessage, OutputPanelViewModel>();

            containerRegistry.RegisterSingleton<MainWindowStatusBarViewModel>();
            containerRegistry.RegisterSingleton<IStatusBarService, MainWindowStatusBarViewModel>();

            containerRegistry.RegisterSingleton<VersionControlPanelViewModel>();

            //throw new NotImplementedException();
        }

        private void OnPreviewGotKeyboardFocus(object sender,
            KeyboardFocusChangedEventArgs e)
        {
            //Console.WriteLine($"keyboard focus changed : {sender} {e}");
        }

        static void MyExceptionHandler(object sender, System.UnhandledExceptionEventArgs args)
        {
            var e = args.ExceptionObject as Exception;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion ?? "unkown version";

            string dumpMsg = $"dump for version {fileVersion} \n";
            dumpMsg += CrashDump.GetExceptionString(e, false);
            File.WriteAllText("crash.txt", dumpMsg);

            if(PluginManager.Inst.UnhandledExceptionHandler != null)
                PluginManager.Inst.UnhandledExceptionHandler.Invoke(sender, args);
        }

        //private void Application_Startup(object sender, StartupEventArgs e)
        //{


        //}
    }
}
