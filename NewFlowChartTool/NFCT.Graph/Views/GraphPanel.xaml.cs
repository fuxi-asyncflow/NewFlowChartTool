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
using System.Windows.Threading;
using FlowChart.Common;
using NFCT.Common;
using NFCT.Graph.ViewModels;
using NLog.Fluent;

namespace NFCT.Graph.Views
{
    public enum GraphCanvasState
    {
        IDLE = 0,   // default states
        DRAG = 1,   // drag canvas by modify scroll
        BOX_SELECT = 2, // select nodes by box
        MOVE = 3,   // move nodes or other items
        CONNECT = 4 // connect two nodes
    }
    /// <summary>
    /// Interaction logic for GraphPanel.xaml
    /// </summary>
    public partial class GraphPanel : UserControl
    {
        public GraphPanel()
        {
            InitializeComponent();
            CanvasState = GraphCanvasState.IDLE;
        }

        #region LAYOUT
        private int LayoutCount = 0;
        private void Graph_LayoutUpdated(object sender, EventArgs e)
        {
            if (LayoutCount == 0)
                LayoutCount = 3;
            GraphPaneViewModel? vm = DataContext as GraphPaneViewModel;
            if (vm == null) return;

            if (vm.NeedLayout)
            {
                // if layout failed, retry up to 3 times
                if (vm.Relayout())
                {
                    vm.NeedLayout = false;
                    vm.ReorderId();
                    if (vm.IsFirstLayout) // move start node to center after first layout
                    {
                        vm.IsFirstLayout = false;
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            var startNode = vm.Nodes.FirstOrDefault();
                            if (startNode != null)
                            {
                                vm.MoveNodeToCenter(startNode);
                                Logger.DBG($"scroll x is set to {vm.ScrollX}");
                            }
                            
                        }), DispatcherPriority.Render);
                    }
                }
                    
                else
                {
                    LayoutCount--;
                    if(LayoutCount == 0)
                        vm.NeedLayout = false;
                }
            }
            if (vm.NeeedReDrawConnectPin)
            {
                vm.ReDrawConnectPin();
                vm.NeeedReDrawConnectPin = false;
            }
        }

        #endregion

        private void CanvasBackground_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = WPFHelper.GetDataContext<GraphPaneViewModel>(this);
            if(vm == null) return;

            if (vm.IsConnecting)
            {
                vm.EndConnect(true);
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                vm.ClearSelectedItems();
                vm.ClearCurrentItem();
                if (!_isBoxSelecting)
                {
                    CanvasState = GraphCanvasState.BOX_SELECT;
                    _mouseScreenPos = Mouse.GetPosition((UIElement)sender);
                    SelectBox.Visibility = Visibility;
                    SetSelectBoxRect(_mouseScreenPos, _mouseScreenPos);
                }
                e.Handled = true;
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                _mouseScreenPos = Mouse.GetPosition(CanvasScroll);
                CanvasState = GraphCanvasState.DRAG;
                e.Handled = true;
            }

        }

        #region CANVAS MOUSE HANDLER

        public GraphCanvasState CanvasState;
        private Point _mouseScreenPos; // 目前使用相对于scrollviewer的坐标
        private bool _isDragingCanvas => CanvasState == GraphCanvasState.DRAG;
        private bool _isBoxSelecting => CanvasState == GraphCanvasState.BOX_SELECT;
        private bool _isMoveingNodes => CanvasState == GraphCanvasState.MOVE;

        private void GraphCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = WPFHelper.GetDataContext<GraphPaneViewModel>(this);
            if (vm == null) return;
            Logger.DBG($"canvas mouse down {Mouse.GetPosition((UIElement)sender)}");
            _mouseScreenPos = Mouse.GetPosition((UIElement)sender);
            e.Handled = true;
        }

        private void GraphCanvas_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isBoxSelecting)
            {
                SelectBox.Visibility = Visibility.Collapsed;
            }
            CanvasState = GraphCanvasState.IDLE;
        }

        private void GraphCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            var vm = WPFHelper.GetDataContext<GraphPaneViewModel>(this);
            if (vm == null) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_isBoxSelecting)
                {
                    var rect = SetSelectBoxRect(_mouseScreenPos, e.GetPosition((UIElement)sender));
                    vm.SelectNodeInBox(rect);
                }
                else if (_isMoveingNodes)
                {
                    
                    var pos = e.GetPosition((UIElement)sender);
                    var deltaX = pos.X - _mouseScreenPos.X;
                    var deltaY = pos.Y - _mouseScreenPos.Y;
                    Logger.DBG($"move node {deltaX} {deltaY}");
                    vm.MoveSelectedItems(deltaX, deltaY);
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed && _isDragingCanvas)
            {
                var newPosition = e.GetPosition(CanvasScroll);
                var deltaPosition = newPosition - _mouseScreenPos;
                _mouseScreenPos = newPosition;

                CanvasScroll.ScrollToHorizontalOffset(CanvasScroll.HorizontalOffset - deltaPosition.X);
                CanvasScroll.ScrollToVerticalOffset(CanvasScroll.VerticalOffset - deltaPosition.Y);
            }
            else
            {
                if (vm.IsConnecting)
                {
                    var pos = e.GetPosition((UIElement)sender);
                    ConnectingLine.X2 = pos.X;
                    ConnectingLine.Y2 = pos.Y;
                }
            }
        }

        private Rect SetSelectBoxRect(Point a, Point b)
        {
            var rect = new Rect(a, b);
            Canvas.SetTop(SelectBox, rect.Top);
            Canvas.SetLeft(SelectBox, rect.Left);
            SelectBox.Width = rect.Width;
            SelectBox.Height = rect.Height;
            return rect;
        }

        public void BeginMove()
        {
            var vm = WPFHelper.GetDataContext<GraphPaneViewModel>(this);
            if (vm == null) return;

            if (vm.AutoLayout)
                return;

            CanvasState = GraphCanvasState.MOVE;
            vm.SelectedNodes.ForEach(node => node.SaveOriginalPos());
        }

        #endregion


        private void GraphPanel_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as GraphPaneViewModel;
            if (vm == null) return;
            vm.ScreenShotEvent += OnScreenShot;
        }

        private void OnScreenShot()
        {
            var vm = DataContext as GraphPaneViewModel;
            if (vm == null)
                return;
            try
            {
                var canvas = GraphCanvas;
                var width = (int)vm.Width;
                var height = (int)vm.Height;
                RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96d, 96d,
                    System.Windows.Media.PixelFormats.Default);
                canvas.Measure(new Size(width, height));
                canvas.Arrange(new Rect(0, 0, width, height));
                rtb.Render(canvas);
                Clipboard.SetImage(rtb);
                MessageBox.Show("图片已保存到剪贴板");
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show("图片太大，内存不足，无法截图");
            }
        }

        private void GraphCanvas_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var vm = DataContext as GraphPaneViewModel;
            if (vm == null)
                return;

            bool ctrlDown = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
            if (!ctrlDown)
                return;
            double oldScale = vm.Scale;
            double newScale = e.Delta > 0 ? Math.Min(oldScale + 0.1, GraphPaneViewModel.ScaleMax) : Math.Max(oldScale - 0.1, GraphPaneViewModel.ScaleMin);

            // 不动点在未缩放的图中的坐标为 (X, Y)
            // MouseX + OffsetX = X * Scale
            double X = Mouse.GetPosition((UIElement)sender).X;
            double Y = Mouse.GetPosition((UIElement)sender).Y;

            double offsetX = vm.ScrollX;
            double offsetY = vm.ScrollY;
            double mouseX = X * oldScale - offsetX;
            double mouseY = Y * oldScale - offsetY;
            vm.ScrollX = X * newScale - mouseX;
            vm.ScrollY = Y * newScale - mouseY;
            vm.Scale = newScale;
        }
    }
}
