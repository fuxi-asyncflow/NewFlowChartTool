using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NewFlowChartTool.Utility;
using NewFlowChartTool.ViewModels;
using NFCT.Common;
using NFCT.Common.Events;
using NFCT.Common.Localization;

namespace NewFlowChartTool.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TitleBar.MainWindow = this;
            Loaded += OnInitialized;

            EventHelper.Sub<LangSwitchEvent, Lang>(OnLangSwitch);
        }

        void OnInitialized(object? sender, EventArgs args)
        {
            WPFPluginManager.Inst.LoadPlugins();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm == null) return;

            if (!vm.CloseWindow())
                e.Cancel = true;
            else
                base.OnClosing(e);
        }

        void OnLangSwitch(Lang lang)
        {
            LayoutProject.Title = Application.Current.FindResource(ResourceKeys.Pane_ProjectKey) as string;
            LayoutType.Title = Application.Current.FindResource(ResourceKeys.Pane_TypeKey) as string;
            LayoutOutput.Title = Application.Current.FindResource(ResourceKeys.Pane_OutputKey) as string;
        }
    }
}
