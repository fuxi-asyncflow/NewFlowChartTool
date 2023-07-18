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
    /// Interaction logic for GroupBox.xaml
    /// </summary>
    public partial class GroupBox : UserControl
    {
        public GroupBox()
        {
            InitializeComponent();
        }

        private void GroupBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var groupVm = WPFHelper.GetDataContext<GroupBoxViewModel>(sender);
            bool isCtrlDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            bool isShiftDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
            if (groupVm == null) return;
            groupVm.Owner.ClearSelectedItems();
            groupVm.Owner.SetCurrentGroup(groupVm);
            if (e.LeftButton == MouseButtonState.Pressed)
            {               
                e.Handled = true;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                
            }
        }
    }
}
