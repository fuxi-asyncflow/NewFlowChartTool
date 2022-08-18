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

namespace NewFlowChartTool.Views
{
    /// <summary>
    /// Interaction logic for MainWindowTitleBar.xaml
    /// </summary>
    public partial class MainWindowTitleBar : UserControl
    {
        public Window MainWindow { get; set; }
        public MainWindowTitleBar()
        {
            InitializeComponent();
            
        }

        private void ButtonMaximize_OnClick(object sender, RoutedEventArgs e)
        {
            if (MainWindow.WindowState == WindowState.Normal)
            {
                MainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                MainWindow.WindowState = WindowState.Maximized;
                MainWindow.WindowStyle = WindowStyle.None;
            }
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Close();
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("drag move mouse down");
            MainWindow.DragMove();
        }
    }
}
