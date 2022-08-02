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
using System.Windows.Threading;
using FlowChartCommon;
using NFCT.Common;
using NFCT.Graph.ViewModels;
using NLog.Fluent;

namespace NFCT.Graph.Views
{
    /// <summary>
    /// Interaction logic for GraphPanel.xaml
    /// </summary>
    public partial class GraphPanel : UserControl
    {
        public GraphPanel()
        {
            InitializeComponent();
        }

        private int LayoutCount = 0;
        private void Graph_LayoutUpdated(object sender, EventArgs e)
        {
            if (LayoutCount == 0)
                LayoutCount = 3;
            GraphPaneViewModel? vm = DataContext as GraphPaneViewModel;
            if (vm == null) return;

            if (vm.NeedLayout)
            {
                // if layout failed, retry up to 3 times
                if (vm.Relayout())
                {
                    vm.NeedLayout = false;
                    if (vm.IsFirstLayout) // move start node to center after first layout
                    {
                        vm.IsFirstLayout = false;
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            var startNode = vm.Nodes.FirstOrDefault();
                            if (startNode != null)
                            {
                                vm.MoveNodeToCenter(startNode);
                                Logger.DBG($"scroll x is set to {vm.ScrollX}");
                            }
                            
                        }), DispatcherPriority.Render);
                    }
                }
                    
                else
                {
                    LayoutCount--;
                    if(LayoutCount == 0)
                        vm.NeedLayout = false;
                }
            }
        }

        private void CanvasBackground_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = WPFHelper.GetDataContext<GraphPaneViewModel>(this);
            if(vm == null) return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                vm.ClearSelectedItems("all");
            }
            
        }
    }
}
