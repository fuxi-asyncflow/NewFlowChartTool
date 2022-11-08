using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Core.DataStructures;

namespace FlowChart.Layout.MyLayout
{
    public class LayoutNode
    {
        public LayoutNode(INode node)
        {
            _node = node;
            OutputEdges = new List<LayoutEdge>();
            Width = _node.Width;
            Height = _node.Height;
            Console.WriteLine($"node size {Width} {Height}");
        }
        private INode _node;
        public double Width;
        public double Height;

        private double _x;
        public double X
        {
            get => _x;
            set
            {
                _x = value;
                _node.X = value;
            }
        }

        private double _y;
        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                _node.Y = value;
            }
        }

        public LayoutNode? TreeParent;
        public int Rank;
        public List<LayoutEdge> OutputEdges;
        public bool IsLeaf => OutputEdges.Count == 0;
        // Rect is the smallest rect contains node and all descendent nods
        public double RectWidth;
        public double RectHeight;
        // Rect position offset to Rect of node's parent
        public double RectTop;
        public double RectLeft;
        // Node position in its Rect
        public double NodeTop;
        public double NodeLeft;
        public double LeftDistance;

        public void Print()
        {
            Console.WriteLine($"{_node}");
            Console.WriteLine($"{(int)RectWidth} {(int)RectHeight} == {(int)Width} {(int)Height}");
            Console.WriteLine($"{(int)RectTop} {(int)RectLeft} == {(int)NodeTop} {(int)NodeLeft}");
            Console.WriteLine("");
        }
    }

    public class LayoutEdge
    {
        public LayoutEdge(IEdge edge, LayoutNode start, LayoutNode end)
        {
            _edge = edge;
            Start = start;
            End = end;

            Start.OutputEdges.Add(this);
        }
        private IEdge _edge;
        public LayoutNode Start;
        public LayoutNode End;
        public bool IsTreeEdge;

        public void Line()
        {
            var curve = new Curve() {Type = Curve.CurveType.Line};
            
            curve.Points = new List<Position>();
            curve.Points.Add(new Position(Start.X + Start.Width * 0.5, Start.Y + Start.Height));
            curve.Points.Add(new Position(End.X + End.Width * 0.5, End.Y));
            _edge.Curves = new List<Curve>() {curve};

        }
    }

    public class LayoutGraphSetting
    {
        public double NodeSpace = 40.0;
        public double RankSpace = 40.0;
    }

    public class LayoutGraph
    {
        public LayoutGraph(IGraph graph)
        {
            _graph = graph;
            Roots = new List<LayoutNode>();
            NodeDict = new Dictionary<INode, LayoutNode>();
            EdgeDict = new Dictionary<IEdge, LayoutEdge>();
            NonTreeEdges = new List<LayoutEdge>();

            BackOrderNodes = new Stack<LayoutNode>(); // save nodes from down to top

            Setting = new LayoutGraphSetting();

            Init(graph);
        }

        protected IGraph _graph;
        public LayoutGraphSetting Setting;

        public List<LayoutNode> Roots;
        public Dictionary<INode, LayoutNode> NodeDict;
        public Dictionary<IEdge, LayoutEdge> EdgeDict;
        public List<LayoutEdge> NonTreeEdges;

        public Stack<LayoutNode> BackOrderNodes;
        public int MaxRank;

        void Init(IGraph graph)
        {
            foreach (var graphNode in graph.Nodes)
            {
                var node = new LayoutNode(graphNode);
                NodeDict.Add(graphNode, node);
            }

            foreach (var graphEdge in graph.Edges)
            {
                var edge = new LayoutEdge(graphEdge, NodeDict[graphEdge.Start], NodeDict[graphEdge.End]);
                EdgeDict.Add(graphEdge, edge);
            }

            Roots.Add(NodeDict[graph.Nodes.First()]);
        }

        public virtual void Layout()
        {
            Acyclic_BFS();
            CalcSubTreeRect();
            //foreach (var kv in NodeDict)
            //{
            //    kv.Value.Print();
            //}
            CalcPosition();
            CalcEdges();
        }

        protected void Acyclic_BFS()
        {
            MaxRank = -1;
            foreach (var node in NodeDict.Values)
            {
                node.Rank = -1;
            }

            var startNode = Roots[0];
            startNode.Rank = 0;
            startNode.TreeParent = null;
            var queue = new Queue<LayoutNode>();
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                BackOrderNodes.Push(node);
                if(node.Rank > 0)
                    continue;
                if (node.TreeParent != null)
                {
                    node.Rank = node.TreeParent.Rank + 1;
                    if(node.Rank > MaxRank)
                        MaxRank = node.Rank;
                }

                node.OutputEdges.ForEach(edge =>
                {
                    var child = edge.End;
                    if (child.TreeParent == null)
                    {
                        child.TreeParent = node;
                        queue.Enqueue(child);
                    }
                });
            }

            foreach (var edge in EdgeDict.Values)
            {
                edge.IsTreeEdge = edge.End.TreeParent == edge.Start;
                if (!edge.IsTreeEdge)
                {
                    edge.Start.OutputEdges.Remove(edge);
                    NonTreeEdges.Add(edge);
                }
            }
        }

        private void CalcSubTreeRect()
        {
            var rankSpace = Setting.RankSpace;
            var nodeSpace = Setting.NodeSpace;
            while (BackOrderNodes.Count > 0)
            {
                var node = BackOrderNodes.Pop();
                var childCount = node.OutputEdges.Count;
                if (childCount == 0)
                {
                    node.RectWidth = node.Width;
                    node.RectHeight = node.Height;
                    node.NodeLeft = 0.0;
                    node.NodeTop = 0.0;
                }
                else if (childCount == 1)
                {
                    var child = node.OutputEdges[0].End;
                    node.RectWidth = Math.Max(node.Width, child.RectWidth);
                    node.RectHeight = node.Height + rankSpace + child.RectHeight;
                    node.NodeLeft = (node.RectWidth - node.Width) / 2;
                    node.NodeTop = 0.0;
                    child.RectLeft = (node.RectWidth - child.RectWidth) / 2;
                    child.RectTop = node.Height + rankSpace;
                }
                else
                {
                    double w = 0.0;
                    double h = 0.0;
                    foreach (var edge in node.OutputEdges)
                    {
                        var child = edge.End;
                        if(child.RectHeight > h)
                            h = child.RectHeight;
                        w += (child.RectWidth + nodeSpace);
                    }
                    w -= nodeSpace;

                    double left = 0.0;

                    node.NodeTop = 0.0;
                    node.RectHeight = node.Height + rankSpace + h;
                    if (w < node.Width) // node's width > sum of children width 
                    {
                        left = (node.Width - w) / 2;
                        node.NodeLeft = 0.0;
                        node.RectWidth = node.Width;
                    }
                    else
                    {
                        node.NodeLeft = (w - node.Width) / 2;
                        node.RectWidth = w;
                    }

                    foreach (var edge in node.OutputEdges)
                    {
                        var child = edge.End;
                        child.RectLeft = left;
                        child.RectTop = node.Height + rankSpace;
                        left += (child.RectWidth + nodeSpace);
                    }
                }
            }
        }

        private void CalcPosition()
        {
            var startNode = Roots[0];
            _calcPositionForNode(startNode);
            _graph.Width = startNode.RectWidth;
            _graph.Height = startNode.RectHeight;
        }

        private void _calcPositionForNode(LayoutNode node)
        {
            if (node.TreeParent == null)
            {
                node.RectLeft = 0.0;
                node.RectTop = 0.0;
            }
            else
            {
                node.RectLeft += node.TreeParent.RectLeft;
                node.RectTop += node.TreeParent.RectTop;
            }
            node.X = node.RectLeft + node.NodeLeft;
            node.Y = node.RectTop + node.NodeTop;
            node.OutputEdges.ForEach(edge => _calcPositionForNode(edge.End));
        }

        public void CalcEdges()
        {
            foreach (var edge in EdgeDict.Values)
            {
                if (edge.Start.Rank < edge.End.Rank)
                    edge.Line();
            }
        }
    }

    public class MyLayout : ILayout
    {
        public void Layout(IGraph graph)
        {
            var g = new LayoutGraph(graph);
            g.Layout();
        }
    }
}
