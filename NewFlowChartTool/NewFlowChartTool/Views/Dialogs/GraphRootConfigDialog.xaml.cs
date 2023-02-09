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

namespace NewFlowChartTool.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for GraphRootConfigDialog.xaml
    /// </summary>
    public partial class GraphRootConfigDialog : UserControl
    {
        public GraphRootConfigDialog()
        {
            InitializeComponent();
            Loaded += delegate
            {
                var window = Window.GetWindow(this);
                if (window == null)
                    return;
                var mainWindow = Application.Current.MainWindow;
                var mousePos = Mouse.GetPosition(mainWindow);
                window.Left = mousePos.X + mainWindow.Left;
                window.Top = mousePos.Y + mainWindow.Top;
                window.Height = 240;
                window.Width = 480;
            };
        }
    }
}
