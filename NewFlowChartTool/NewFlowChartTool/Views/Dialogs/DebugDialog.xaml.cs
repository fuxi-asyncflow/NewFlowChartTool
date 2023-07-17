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
using FlowChart.Debug;
using NewFlowChartTool.ViewModels;
using NFCT.Common;
using NFCT.Common.Events;

namespace NewFlowChartTool.Views
{
    /// <summary>
    /// Interaction logic for DebugDialog.xaml
    /// </summary>
    public partial class DebugDialog : UserControl
    {
        public DebugDialog()
        {
            InitializeComponent();
            Loaded += delegate
            {
                var window = Window.GetWindow(this);
                if (window == null)
                    return;
                var mainWindow = Application.Current.MainWindow;
                var mousePos = Mouse.GetPosition(mainWindow);
                window.Left = mousePos.X + mainWindow.Left;
                window.Top = mousePos.Y + mainWindow.Top;
                window.Height = 450;
                window.Width = 800;
            };
        }

        private void OnDebugChartGridRowDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            var vm = WPFHelper.GetDataContext<DebugDialogViewModel>(this);
            if (vm == null) return;
            if (vm.SelectedGraphInfo == null) return;
            
            //vm.OpenUnitInDebugMode(vm.SelectedChart.ChartName);
            if (vm.IsReplay)
            {
                vm.StartReplay(vm.SelectedGraphInfo.GraphInfo);
            }
            else
            {
                vm.StartDebugGraph(vm.SelectedGraphInfo.GraphInfo);
            }
        }

        private void TitleBlank_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var move = sender as UIElement;
            var win = Window.GetWindow(move);
            win.DragMove();
        }

        private void OnServerConfigChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = WPFHelper.GetDataContext<DebugDialogViewModel>(this);
            if (vm == null) return;
            var item = ServerComboBox.SelectedItem as ServerConfigViewModel;
            if (item == null) return;
            vm.StartPort = item.StartPort;
            vm.EndPort = item.EndPort;
            vm.Host = item.Host;
        }
    }
}
