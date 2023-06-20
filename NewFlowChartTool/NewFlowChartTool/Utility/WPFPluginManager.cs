using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using FlowChart.Common;
using FlowChart.Common.Report;
using FlowChart.Plugin;
using NewFlowChartTool.ViewModels;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace NewFlowChartTool.Utility
{
    public class CustomMenuItemViewModel : BindableBase
    {
        public CustomMenuItemViewModel(string text, DelegateCommand cmd, ImageSource? imageSource)
        {
            Text = text;
            Command = cmd;
            ImageSource = imageSource;
        }
        public string Text { get; set; }
        private ImageSource? _imageSource;
        public ImageSource? ImageSource
        {
            get => _imageSource;
            set => _imageSource = value;
        }
        public DelegateCommand Command { get; set; }
    }
    public class WPFPluginManager: IPluginManager
    {
        static WPFPluginManager()
        {
            Inst = new WPFPluginManager();
            
        }

        public WPFPluginManager()
        {
            CustomViews = new Dictionary<string, Type>();
            CustomViewModels = new Dictionary<string, Type>();
            Menus = new ObservableCollection<CustomMenuItemViewModel>();
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

                        if (!pluginInstance.Register(PluginManager.Inst))
                        {
                            Logger.WARN($"register plugin {type.FullName} to Core failed!");
                        }
                        else
                        {
                            Logger.LOG($"register plugin {type.FullName} to Core success!");
                        }

                        if (!pluginInstance.Register(this))
                        {
                            Logger.WARN($"register plugin {type.FullName} to WPF failed!");
                        }
                        else
                        {
                            Logger.LOG($"register plugin {type.FullName} to WPF success!");
                        }
                    }
                }
            }

            //throw new Exception("crash report test");
        }

        public string GetGitVersion()
        {
            var asm = typeof(WPFPluginManager).Assembly;
            var attrs = asm.GetCustomAttributes<AssemblyMetadataAttribute>();
            var gitHash = attrs.FirstOrDefault(a => a.Key == "GitHash")?.Value;
            if (gitHash == null)
                return "unkown-git-version";
            return gitHash;
        }

        public string GetBuildTime()
        {
            var asm = typeof(WPFPluginManager).Assembly;
            var attrs = asm.GetCustomAttributes<AssemblyMetadataAttribute>();
            var v = attrs.FirstOrDefault(a => a.Key == "BuildTime")?.Value;
            if (v == null)
                return "unkown-build-time";
            return v;
        }

        #region USER CONTROL

        private Dictionary<string, Type> CustomViews;
        private Dictionary<string, Type> CustomViewModels;
        public bool RegisterUserControl<TView, TViewModel>(string name)
            where TView : UserControl
            where TViewModel : BindableBase
        {
            CustomViews.Add(name, typeof(TView));
            CustomViewModels.Add(name, typeof(TViewModel));
            return true;
        }

        public UserControl? GetCustomView(string name)
        {
            if (!CustomViews.ContainsKey(name))
            {
                Logger.ERR($"cannot find usercontrol named {name}, please check plugin");
                return null;
            }

            var view = System.Activator.CreateInstance(CustomViews[name]) as UserControl;
            if (view == null)
            {
                Logger.ERR($"usercontrol `{name}` is not a UserControl, please check plugin register function");
                return null;
            }

            view.DataContext = System.Activator.CreateInstance(CustomViewModels[name]) as BindableBase;
            return view;
        }

        public bool ShowDialog(string name)
        {
            var dlgService = ContainerLocator.Current.Resolve<IDialogService>();
            var parameters = new DialogParameters();
            parameters.Add("name", name);
            dlgService.Show(CustomDialogViewModel.NAME, parameters, result => { });
            return true;
        }
        #endregion

        #region Menu

        public ObservableCollection<CustomMenuItemViewModel> Menus { get; set; }

        public bool RegisterMenu(string name, DelegateCommand cmd, ImageSource? icon = null)
        {
            Menus.Add(new CustomMenuItemViewModel(name, cmd, icon));
            return true;
        }

        #endregion



        public static WPFPluginManager Inst { get; set; }

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
