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
        }

        private void OnDebugChartGridRowDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            var vm = WPFHelper.GetDataContext<DebugDialogViewModel>(this);
            if (vm == null) return;
            if (vm.SelectedGraphInfo == null) return;
            
            //vm.OpenUnitInDebugMode(vm.SelectedChart.ChartName);
            vm.StartDebugGraph(vm.SelectedGraphInfo.GraphInfo);
        }

        private void TitleBlank_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var move = sender as UIElement;
            var win = Window.GetWindow(move);
            win.DragMove();
        }
    }
}
