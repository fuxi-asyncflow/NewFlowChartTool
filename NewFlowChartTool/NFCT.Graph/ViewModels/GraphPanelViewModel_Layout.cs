using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FlowChart.Layout;
using FlowChartCommon;

namespace NFCT.Graph.ViewModels
{
    public partial class GraphPaneViewModel
    {
        public bool NeedLayout { get; set; }
        public bool IsFirstLayout { get; set; }
        private double _width;
        public double Width { get => _width; set => SetProperty(ref _width, value); }

        public double _height;
        public double Height { get => _height; set => SetProperty(ref _height, value); }

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

        public bool Relayout()
        {
            Logger.DBG($"Relayout for graph {Name}");
            var graph = new GraphLayoutAdapter(this);
            if (NodeDict.Count <= 0.0 || NodeDict.First().Value.Width <= 0.0)
                return false;
            MsaglLayout layout = new MsaglLayout();
            try
            {
                layout.Layout(graph);
                if (Height < 0.001 || Width < 0.001)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.ERR($"[error] layout failed {e.Message}");
                return false;
            }

            return true;
        }

        public void ReorderId()
        {
            var nodeStack = new Stack<BaseNodeViewModel>();
            var nodeSet = new HashSet<BaseNodeViewModel>();
            nodeStack.Push(Nodes[0]);
            int id = 0;

            while (nodeStack.Count > 0)
            {
                var node = nodeStack.Pop();
                if (nodeSet.Contains(node))
                    continue;
                nodeSet.Add(node);
                node.Id = id++;
                // left node push last, then pop last
                //node.ChildLines.Sort((a, b) => a.X.CompareTo(b.X));
                var childLines = node.ChildLines;
                childLines.Reverse();
                childLines.ForEach(connVm => nodeStack.Push(connVm.EndNode));
            }
        }

    }
}
