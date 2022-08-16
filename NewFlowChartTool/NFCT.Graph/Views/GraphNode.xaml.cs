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
using FlowChartCommon;
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
            if (nodeVm == null) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                nodeVm.Owner.SetCurrentNode(nodeVm, !Keyboard.Modifiers.HasFlag(ModifierKeys.Control));
                nodeVm.Owner.EndConnect();
                Logger.DBG($"node mouse down {Graph}");
                Graph?.BeginMove();
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                //bool nodeIsSelected = UnitCanvasViewModel.Current.SelectedNodes.Contains(nodeVm);
                //UnitCanvasViewModel.Current.SetCurrentNode(nodeVm, !nodeIsSelected, false);
                //e.Handled = true;
            }
        }
    }
}
