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
using FlowChartCommon;
using NFCT.Common;
using NFCT.Graph.ViewModels;
using NLog.Fluent;

namespace NFCT.Graph.Views
{
    enum GraphCanvasState
    {
        IDLE = 0,
        DRAG = 1,
        BOX_SELECT = 2
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
        }

        #endregion

        private void CanvasBackground_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = WPFHelper.GetDataContext<GraphPaneViewModel>(this);
            if(vm == null) return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                vm.ClearSelectedItems("all");
            }
            
        }

        #region CANVAS MOUSE HANDLER

        private GraphCanvasState CanvasState;
        private Point _mouseScreenPos; // 目前使用相对于scrollviewer的坐标
        private bool _isDragingCanvas => CanvasState == GraphCanvasState.DRAG;
        private bool _isBoxSelecting => CanvasState == GraphCanvasState.BOX_SELECT;

        private void GraphCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = WPFHelper.GetDataContext<GraphPaneViewModel>(this);
            if (vm == null) return;
            Logger.DBG($"canvas mouse down {Mouse.GetPosition((UIElement)sender)}");

            if (!_isBoxSelecting && e.LeftButton == MouseButtonState.Pressed)
            {
                CanvasState = GraphCanvasState.BOX_SELECT;
                vm.ClearSelectedItems("all");
                _mouseScreenPos = Mouse.GetPosition((UIElement)sender);
                SelectBox.Visibility = Visibility;
                SetSelectBoxRect(_mouseScreenPos, _mouseScreenPos);
            }

            // drag and move
            if (e.RightButton == MouseButtonState.Pressed)
            {
                _mouseScreenPos = Mouse.GetPosition(CanvasScroll);
                CanvasState = GraphCanvasState.DRAG;
            }
        }

        private void GraphCanvas_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isBoxSelecting)
            {
                SelectBox.Visibility = Visibility.Collapsed;
            }
            if (e.ChangedButton == MouseButton.Right)
            {
                CanvasState = GraphCanvasState.IDLE;
            }
        }

        private void GraphCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _isBoxSelecting)
            {
                SetSelectBoxRect(_mouseScreenPos, e.GetPosition((UIElement)sender));
            }
            else if (e.RightButton == MouseButtonState.Pressed && _isDragingCanvas)
            {
                var newPosition = e.GetPosition(CanvasScroll);
                var deltaPosition = newPosition - _mouseScreenPos;
                _mouseScreenPos = newPosition;

                CanvasScroll.ScrollToHorizontalOffset(CanvasScroll.HorizontalOffset - deltaPosition.X);
                CanvasScroll.ScrollToVerticalOffset(CanvasScroll.VerticalOffset - deltaPosition.Y);
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

        #endregion


    }
}
