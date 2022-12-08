using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FlowChart.Core;
using FlowChart.Layout;
using FlowChart.Layout.MyLayout;
using FlowChartCommon;

namespace NFCT.Graph.ViewModels
{
    public partial class GraphPaneViewModel
    {
        void GraphPaneViewModel_Layout_Init()
        {
            _layout = LayoutManager.Instance.LayoutDict["layout_group"].Invoke();
            Scale = 1.0f;
        }
        public Node? InitCenterNode { get; set; }
        public bool NeedLayout { get; set; }
        public bool IsFirstLayout { get; set; }
        private double _width;
        public double Width
        {
            get => _width;
            set
            {
                SetProperty(ref _width, value); 
                RaisePropertyChanged(nameof(ScaledWidth));
            }
        }

        public double _height;
        public double Height
        {
            get => _height;
            set
            {
                SetProperty(ref _height, value);
                RaisePropertyChanged(nameof(ScaledHeight));
            }
        }

        public double ScaledWidth => Width * Scale;
        public double ScaledHeight => Height * Scale;
        private ILayout _layout;
        public static double ScaleMax => 2.0;
        public static double ScaleMin => 0.05;
        private double _scale;
        public double Scale
        {
            get => _scale;
            set
            {
                SetProperty(ref _scale, value);
                RaisePropertyChanged(nameof(ScaledWidth));
                RaisePropertyChanged(nameof(ScaledHeight));
            }
        }

        public bool AutoLayout
        {
            get => _graph.AutoLayout;
        }

        public void ChangeAutoLayout()
        {
            if (AutoLayout)
            {
                _graph.AutoLayout = false;
                RaisePropertyChanged(nameof(AutoLayout));
            }
            else
            {
                var result = MessageBox.Show("Switch to AutoLayout, all nodes position will change", "Be careful",
                    MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                    return;
                _graph.AutoLayout = true;
                NeedLayout = true;
                RaisePropertyChanged(nameof(AutoLayout));
            }
        }

        public void ChangeLayout(ILayout layout)
        {
            _layout = layout;
            Relayout();
        }

        public bool Relayout()
        {
            ReorderId();
            Logger.DBG($"Relayout for graph {Name}");
            var graph = new GraphLayoutAdapter(this);
            if (NodeDict.Count <= 0.0 || NodeDict.First().Value.Width <= 0.0)
                return false;
            //var layout = new MsaglLayout();
            //var layout = new MyLayout();
            //var layout = new MyLayout2();
            try
            {
                _layout.Layout(graph);
                if (Height < 0.001 || Width < 0.001)
                {
                    return false;
                }

                if (InitCenterNode != null)
                {
                    MoveNodeToCenter(InitCenterNode);
                    InitCenterNode = null;
                }
#if DEBUG
                foreach (var nodeVm in Nodes)
                {
                    nodeVm.ToolTip = $"t:{nodeVm.Top:F2} l:{nodeVm.Left:F2} w:{nodeVm.ActualWidth:F2} h:{nodeVm.ActualHeight:F2}";
                }
#endif
            }
            catch (Exception e)
            {
                Logger.ERR($"[error] layout failed {e.Message}");
#if DEBUG
                throw;
#endif
                return false;
            }

            foreach (var groupVm in Groups)
            {
                groupVm.Resize();
            }

            return true;
        }

        public void ReorderId()
        {
            var nodeStack = new Stack<BaseNodeViewModel>();
            var nodeSet = new HashSet<BaseNodeViewModel>();
            nodeStack.Push(Nodes[0]);
            int id = 0;
            Nodes.Clear();

            while (nodeStack.Count > 0)
            {
                var node = nodeStack.Pop();
                
                if (nodeSet.Contains(node))
                    continue;
                nodeSet.Add(node);
                node.Id = id++;
                Nodes.Add(node);
                // left node push last, then pop last
                //node.ChildLines.Sort((a, b) => a.X.CompareTo(b.X));
                var childLines = node.ChildLines;
                childLines.Reverse();
                childLines.ForEach(connVm => nodeStack.Push(connVm.EndNode));
                //childLines.Reverse();
            }

            Connectors.Clear();
            foreach (var nodeVm in Nodes)
            {
                Connectors.AddRange(nodeVm.ChildLines);
            }
        }

        public void MoveNodeToCenter(Node? node)
        {
            if (node == null)
                return;
            var nodeVm = GetNodeVm(node);
            ScrollX = Math.Clamp(nodeVm.Left * Scale - ScrollViewerWidth / 2, 0, Width*Scale);
            ScrollY = Math.Clamp(nodeVm.Top * Scale - ScrollViewerHeight / 2, 0, Height*Scale);

        }
    }
}
