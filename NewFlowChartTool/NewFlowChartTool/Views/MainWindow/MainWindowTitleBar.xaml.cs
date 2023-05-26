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
using NewFlowChartTool.ViewModels;

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

        private void MaximizeOrRestore()
        {
            if (MainWindow.WindowState == WindowState.Normal)
            {
                // if not set windowsStyle, windows maximized will hidden taskbar and fulfill entire screen
                MainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                MainWindow.WindowState = WindowState.Maximized;
                MainWindow.WindowStyle = WindowStyle.None;

                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;

                MainWindow.BorderThickness = new Thickness(5);
            }
            else
            {
                MainWindow.WindowState = WindowState.Normal;

                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;

                MainWindow.BorderThickness = new Thickness(1);
            }
        }

        private void ButtonMaximize_OnClick(object sender, RoutedEventArgs e)
        {
            MaximizeOrRestore();
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Close();
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
            {
                MaximizeOrRestore();
                return;
            }
            if(e.ChangedButton == MouseButton.Left)
                MainWindow.DragMove();
        }

        private void ButtonRestore_OnClick(object sender, RoutedEventArgs e)
        {
            MaximizeOrRestore();
        }

        private void ButtonMinimize_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.WindowState = WindowState.Minimized;
        }
    }
}
