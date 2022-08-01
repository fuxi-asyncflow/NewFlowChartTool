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
using NFCT.Graph.ViewModels;
using NFCT.Common;

namespace NFCT.Graph.Views
{
    /// <summary>
    /// Interaction logic for TextNode.xaml
    /// </summary>
    public partial class TextNode : UserControl
    {
        public TextNode()
        {
            InitializeComponent();
        }

        private void NodeGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var nodeVm = WPFHelper.GetDataContext<BaseNodeViewModel>(sender);
            if (nodeVm == null) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                nodeVm.Owner.SelectNode(nodeVm);
                e.Handled = true;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                //bool nodeIsSelected = UnitCanvasViewModel.Current.SelectedNodes.Contains(nodeVm);
                //UnitCanvasViewModel.Current.SetCurrentNode(nodeVm, !nodeIsSelected, false);
                //e.Handled = true;
            }
        }

        private void NodeGrid_OnMouseMove(object sender, MouseEventArgs e)
        {
            // 防止在节点上右键拖动
            if (e.RightButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
            }
        }
    }
}

    
