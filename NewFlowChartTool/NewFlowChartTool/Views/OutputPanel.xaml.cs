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
using NFCT.Common.Events;

namespace NewFlowChartTool.Views
{
    /// <summary>
    /// Interaction logic for OutputPanel.xaml
    /// </summary>
    public partial class OutputPanel : UserControl
    {
        public OutputPanel()
        {
            InitializeComponent();
            EventHelper.Sub<ScrollOutputToEndEvent>(ScrollOutputToEnd);
        }

        private void OutputItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var vm = WPFHelper.GetDataContext<OutputItemViewModel>(sender);
            if (vm == null)
                return;
            if (vm.Node == null)
                return;
            MainWindowViewModel.Inst?.OnOpenGraph(vm.Node.OwnerGraph, vm.Node);
        }

        void ScrollOutputToEnd()
        {
            //TODO Debounce
            if (VisualTreeHelper.GetChildrenCount(OutputListBox) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(OutputListBox, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }
    }
}
