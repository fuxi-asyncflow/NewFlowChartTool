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
using FlowChart.Common;
using NFCT.Common;
using NFCT.Graph.ViewModels;

namespace NFCT.Graph.Views
{
    /// <summary>
    /// Interaction logic for GraphNode.xaml
    /// </summary>
    public partial class GraphNode : UserControl
    {
        public GraphNode()
        {
            InitializeComponent();
        }

        private GraphPanel? _graphPanel;

        private GraphPanel? Graph
        {
            get { return _graphPanel ??= WPFHelper.GetVisualParent<GraphPanel>(this); }
        }

        private void NodeGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var nodeVm = WPFHelper.GetDataContext<BaseNodeViewModel>(sender);
            bool isCtrlDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            bool isShiftDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
            if (nodeVm == null) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (isShiftDown)
                {
                    var firstNode = nodeVm.Owner.CurrentNode;
                    if (firstNode != null)
                    {
                        var nodes = nodeVm.Owner.FindPathBetweenNodes(firstNode, nodeVm);
                        if(nodes == null)
                            nodes = nodeVm.Owner.FindPathBetweenNodes(nodeVm, firstNode);
                        if (nodes != null)
                            nodeVm.Owner.SelectNodes(nodes, !isCtrlDown);
                    }
                }
                else
                {
                    nodeVm.Owner.SetCurrentNode(nodeVm, !isCtrlDown);
                    nodeVm.Owner.EndConnect();
                    Logger.DBG($"node mouse down {Graph}");
                    Graph?.BeginMove();
                }
                
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                nodeVm.Owner.SetCurrentNode(nodeVm, !isCtrlDown);
                //bool nodeIsSelected = UnitCanvasViewModel.Current.SelectedNodes.Contains(nodeVm);
                //UnitCanvasViewModel.Current.SetCurrentNode(nodeVm, !nodeIsSelected, false);
                //e.Handled = true;
            }
        }

        private void Node_OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
            //throw new NotImplementedException();
        }
    }
}
