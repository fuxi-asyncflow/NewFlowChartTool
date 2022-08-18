using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace NewFlowChartTool.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TitleBar.MainWindow = this;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm == null) return;

            if (!vm.CloseWindow())
                e.Cancel = true;
            else
                base.OnClosing(e);
        }
    }
}
