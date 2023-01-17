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
using FlowChart.Parser.NodeParser;
using NFCT.Diff.ViewModels;

namespace NFCT.Diff.Views
{
    /// <summary>
    /// Interaction logic for VersionControlPanel.xaml
    /// </summary>
    public partial class VersionControlPanel : UserControl
    {
        public VersionControlPanel()
        {
            InitializeComponent();
        }

        private void OnDoubleClick_VersionItem(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as VersionControlPanelViewModel;
            if (vm == null)
                return;
            var item = (sender as FrameworkElement).DataContext as VersionItemViewModel;
            if (item == null)
                return;
            vm.ShowVersion(item.Version);

        }

        private void TreeViewItem_OnExpanded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as VersionControlPanelViewModel;
            if (vm == null)
                return;
            var item = (e.OriginalSource as FrameworkElement).DataContext as FileItemViewModel;
            if (item == null)
                return;
            vm.GetChangedGraphInFile(item);

        }

        private void GraphItem_OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as VersionControlPanelViewModel;
            if (vm == null)
                return;
            var item = (e.OriginalSource as FrameworkElement).DataContext as GraphItemViewModel;
            if (item == null)
                return;
            vm.ShowGraph(item);

        }


        private void ChangedItem_OnHandler(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as VersionControlPanelViewModel;
            if (vm == null || vm.DiffGraphVm == null)
                return;
            var item = (e.OriginalSource as FrameworkElement).DataContext;
            if (item is DiffNodeViewModel nodeVm)
            {
                vm.DiffGraphVm.MoveNodeToCenter(nodeVm);
            }
            else if (item is DiffConnectorViewModel connVm)
            {
                vm.DiffGraphVm.MoveLineToCenter(connVm);
            }
        }
    }
}
