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
using NFCT.Common;
using NFCT.Graph.ViewModels;

namespace NFCT.Graph.Views
{
    /// <summary>
    /// Interaction logic for GraphThumbnail.xaml
    /// </summary>
    public partial class GraphThumbnail : UserControl
    {
        public GraphThumbnail()
        {
            InitializeComponent();
            InitScaleComboBox();
        }

        private void InitScaleComboBox()
        {
            ScaleComboBox.Items.Add("200%");
            ScaleComboBox.Items.Add("150%");
            ScaleComboBox.Items.Add("100%");
            ScaleComboBox.Items.Add("70%");
            ScaleComboBox.Items.Add("50%");
            ScaleComboBox.Items.Add("20%");
        }

        private Point _thumbnailPoint;
        private bool _thumbnailMoving = false;
        private Point _thumbnailStartOffset;

        private void Thumbnail_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as GraphPaneViewModel;
            if (vm == null)
                return;
            if (sender is Border)
            {
                _thumbnailPoint = e.GetPosition(sender as Border);
                _thumbnailMoving = true;
                _thumbnailStartOffset.X = vm.ScrollX;
                _thumbnailStartOffset.Y = vm.ScrollY;
            }
        }

        private static int count = 0;
        private void Thumbnail_MouseMove(object sender, MouseEventArgs e)
        {
            var vm = DataContext as GraphPaneViewModel;
            if (vm == null)
                return;
            if (_thumbnailMoving && sender is Border && e.LeftButton == MouseButtonState.Pressed)
            {
                var p = e.GetPosition(sender as Border);
                var delta = p - _thumbnailPoint;
                var xRatio = vm.Width / 180.0;
                var yRatio = vm.Height / 150.0;
                var ratio = xRatio > yRatio ? xRatio : yRatio;
                var scrollX = _thumbnailStartOffset.X + ratio * delta.X;
                vm.ScrollX = Math.Clamp(scrollX, 0, vm.Width - vm.ScrollViewerWidth);
                var scrollY = _thumbnailStartOffset.Y + ratio * delta.Y;
                vm.ScrollY = Math.Clamp(scrollY, 0, vm.Height - vm.ScrollViewerHeight);
            }
        }

        private void Thumbnail_MouseLeave(object sender, MouseEventArgs e)
        {
            _thumbnailMoving = false;
        }

        private void Thumbnail_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _thumbnailMoving = false;
        }
    }
}
