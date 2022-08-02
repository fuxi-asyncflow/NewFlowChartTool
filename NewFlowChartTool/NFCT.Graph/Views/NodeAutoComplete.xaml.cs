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

namespace NFCT.Graph.Views
{
    /// <summary>
    /// Interaction logic for NodeAutoComplete.xaml
    /// </summary>
    public partial class NodeAutoComplete : UserControl
    {
        public NodeAutoComplete()
        {
            InitializeComponent();
        }

        private void NodeContentEditBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var vm = WPFHelper.GetDataContext<NodeAutoCompleteViewModel>(this);
            if (vm == null)
                return;
            vm.Node.ExitEditingMode(vm);
        }

        public void SetFocus()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                NodeContentEditBox.Focus(); // Don't care about false values.
                Keyboard.Focus(NodeContentEditBox);
            }), DispatcherPriority.Render);
        }
    }
}
