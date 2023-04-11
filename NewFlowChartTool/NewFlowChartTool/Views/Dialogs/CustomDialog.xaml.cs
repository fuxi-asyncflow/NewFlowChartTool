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
using NewFlowChartTool.Utility;
using NewFlowChartTool.ViewModels;

namespace NewFlowChartTool.Views
{
    /// <summary>
    /// Interaction logic for CustomDialog.xaml
    /// </summary>
    public partial class CustomDialog : UserControl
    {
        public CustomDialog()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as CustomDialogViewModel;
            if (vm == null)
                return;
            vm.ControlChangeEvent += OnControlChangeEvent;
        }

        private void OnControlChangeEvent(string name)
        {
            Main.Content = WPFPluginManager.Inst.GetCustomView(name);
        }
    }
}
