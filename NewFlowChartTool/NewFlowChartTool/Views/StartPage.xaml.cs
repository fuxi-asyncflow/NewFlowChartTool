using System;
using System.Collections.Generic;
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
using NewFlowChartTool.ViewModels;
using NFCT.Common;
using Prism.Ioc;

namespace NewFlowChartTool.Views
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : UserControl
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private void ProjectName_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var vm = WPFHelper.GetDataContext<ProjectHistory>(sender);
            if (vm == null)
                return;
            MainWindowViewModel.Inst?.OpenProject(vm.ProjectPath);
        }

        private async void GraphPath_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var mainWindowVm = MainWindowViewModel.Inst;
            if (mainWindowVm == null)
                return;
            var item = sender as ListBoxItem;
            var lb = WPFHelper.GetVisualParent<ItemsPresenter>(sender);
            if (lb == null) return;
            var vm = WPFHelper.GetDataContext<ProjectHistory>(lb);
            if (item == null || vm == null) return;
            if (mainWindowVm.CurrentProject == null)
            {
                mainWindowVm.OpenProject(vm.ProjectPath);
            }
            if (mainWindowVm.CurrentProject == null) return;
            if (mainWindowVm.CurrentProject.Path == vm.ProjectPath)
            {
                var graphInfo = item.DataContext as ProjectHistory.GraphInfo;
                if (graphInfo != null)
                    mainWindowVm.OpenGraph(graphInfo.FullPath);
            }
        }
    }
}
