using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Core.DataStructures;

namespace FlowChart.Layout.MyLayout
{
    public class LayoutNode : INode
    {
        public LayoutNode(INode node)
        {
            _node = node;
            InEdges = new List<LayoutEdge>();
            OutEdges = new List<LayoutEdge>();
            Width = _node.Width;
            Height = _node.Height;
        }

        public LayoutNode()
        {
            _node = this;
            InEdges = new List<LayoutEdge>();
            OutEdges = new List<LayoutEdge>();
        }

        private INode _node;
        public INode Node => _node;
        public double Width { get; set; }
        public double Height { get; set; }

        private double _x;
        public double X
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    _node.X = value;
                }
            }
        }

        private double _y;
        public double Y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    _node.Y = value;
                }
            }
        }

        public LayoutNode? TreeParent;
        public int Rank;
        public List<LayoutEdge> InEdges;
        public List<LayoutEdge> OutEdges;
        public bool IsLeaf => OutEdges.Count == 0;
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

        public bool IsAncestor(LayoutNode descendantNode)
        {
            var node = descendantNode;
            while (node != null)
            {
                if (node == this)
                    return true;
                node = node.TreeParent;
            }
            return false;
        }
    }

    public class LayoutEdge : IEdge
    {
        public LayoutEdge(IEdge edge, LayoutNode startNode, LayoutNode endNode)
        {
            _edge = edge;
            StartNode = startNode;
            EndNode = endNode;

            StartNode.OutEdges.Add(this);
            EndNode.InEdges.Add(this);
        }

        public LayoutEdge(LayoutNode startNode, LayoutNode endNode)
        {
            _edge = this;
            StartNode = startNode;
            EndNode = endNode;
            StartNode.OutEdges.Add(this);
            EndNode.InEdges.Add(this);
        }

        private IEdge _edge;
        public IEdge Edge => _edge;
        public LayoutNode StartNode;
        public LayoutNode EndNode;
        public bool IsTreeEdge;

        public INode Start => StartNode;
        public INode End => EndNode;
        public List<Curve> Curves
        {
            set { _edge.Curves = value; }
        }

        public void Disconnect()
        {
            StartNode.OutEdges.Remove(this);
            EndNode.InEdges.Remove(this);
        }

        public void Line()
        {
            var curve = new Curve() {Type = Curve.CurveType.Line};
            
            curve.Points = new List<Position>();
            curve.Points.Add(new Position(StartNode.X + StartNode.Width * 0.5, StartNode.Y + StartNode.Height));
            curve.Points.Add(new Position(EndNode.X + EndNode.Width * 0.5, EndNode.Y));
            _edge.Curves = new List<Curve>() {curve};
        }

        public void CubicBezier()
        {
            const double L = 80.0;
            var startX = StartNode.X + StartNode.Width * 0.5;
            var endX = EndNode.X + EndNode.Width * 0.5;

            var curve = new Curve() { Type = Curve.CurveType.SPLINE };

            if (Math.Abs(startX - endX) < 20.0 && (EndNode.Rank - StartNode.Rank) != 1)
            {
                var delatHeight = EndNode.Y - StartNode.Y - StartNode.Height;
                var d = delatHeight * 0.25;
                if (d < L)
                    d = L;
                curve.Points = new List<Position>();
                curve.Points.Add(new Position(startX, StartNode.Y + StartNode.Height));
                curve.Points.Add(new Position(startX + d, StartNode.Y + StartNode.Height + L));
                curve.Points.Add(new Position(endX + d, EndNode.Y - L));
                curve.Points.Add(new Position(endX, EndNode.Y));
                _edge.Curves = new List<Curve>() { curve };
                return;
            }
            

            curve.Points = new List<Position>();
            curve.Points.Add(new Position(startX, StartNode.Y + StartNode.Height));
            curve.Points.Add(new Position(startX, StartNode.Y + StartNode.Height + L));
            curve.Points.Add(new Position(endX, EndNode.Y - L));
            curve.Points.Add(new Position(endX, EndNode.Y));
            _edge.Curves = new List<Curve>() { curve };
        }
    }

    public class LayoutGraphSetting
    {
        public double NodeSpace = 40.0;
        public double RankSpace = 40.0;
        public double MinWidth = 1920.0;
        public double MinHeight = 1080.0;
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

            //Init(graph);
        }

        protected IGraph _graph;
        public LayoutGraphSetting Setting;

        public List<LayoutNode> Roots;
        public Dictionary<INode, LayoutNode> NodeDict;
        public Dictionary<IEdge, LayoutEdge> EdgeDict;
        public List<LayoutEdge> NonTreeEdges;

        public Stack<LayoutNode> BackOrderNodes;
        public int MaxRank;

        public void Init()
        {
            var graph = _graph;
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

                node.OutEdges.ForEach(edge =>
                {
                    var child = edge.EndNode;
                    if (child.TreeParent == null)
                    {
                        child.TreeParent = node;
                        queue.Enqueue(child);
                    }
                });
            }

            foreach (var edge in EdgeDict.Values)
            {
                edge.IsTreeEdge = edge.EndNode.TreeParent == edge.StartNode;
                if (!edge.IsTreeEdge)
                {
                    edge.StartNode.OutEdges.Remove(edge);
                    NonTreeEdges.Add(edge);
                }
            }
        }

        protected void Acyclic_BFS_Max()
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

            Action<LayoutNode, int>? updateSubtreeRank = null;
            updateSubtreeRank = (node, r) =>
            {
                node.Rank = r;
                node.OutEdges.ForEach(edge =>
                {
                    if (edge.EndNode.TreeParent == node)
                    {
                        updateSubtreeRank?.Invoke(edge.EndNode, r + 1);
                    }
                });
            };

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                node.OutEdges.ForEach(edge =>
                {
                    var rank = node.Rank + 1;
                    var child = edge.EndNode;
                    if (child.TreeParent == null)
                    {
                        child.TreeParent = node;
                        child.Rank = rank;
                        queue.Enqueue(child);
                    }
                    else
                    {
                        var originRank = child.Rank;
                        if (rank > originRank && !child.IsAncestor(node))
                        {
                            child.TreeParent = node;
                            updateSubtreeRank(child, rank);
                        }
                    }
                });
            }

            MaxRank = -1;
            foreach (var node in NodeDict.Values)
            {
                if (node.Rank > MaxRank)
                    MaxRank = node.Rank;
            }

            foreach (var edge in EdgeDict.Values)
            {
                edge.IsTreeEdge = edge.EndNode.TreeParent == edge.StartNode;
                if (!edge.IsTreeEdge)
                {
                    edge.StartNode.OutEdges.Remove(edge);
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
                var childCount = node.OutEdges.Count;
                if (childCount == 0)
                {
                    node.RectWidth = node.Width;
                    node.RectHeight = node.Height;
                    node.NodeLeft = 0.0;
                    node.NodeTop = 0.0;
                }
                else if (childCount == 1)
                {
                    var child = node.OutEdges[0].EndNode;
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
                    foreach (var edge in node.OutEdges)
                    {
                        var child = edge.EndNode;
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

                    foreach (var edge in node.OutEdges)
                    {
                        var child = edge.EndNode;
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
            node.OutEdges.ForEach(edge => _calcPositionForNode(edge.EndNode));
        }

        public void CalcEdges()
        {
            foreach (var edge in EdgeDict.Values)
            {
                if (edge.StartNode.Rank < edge.EndNode.Rank)
                    edge.Line();
            }
        }

        public void CalcCubicBezierEdge()
        {
            const double L = 40.0;
            //TODO self-connected edge
            foreach (var edge in EdgeDict.Values)
            {
                edge.CubicBezier();
            }
        }
    }

    public class MyLayout : ILayout
    {
        public void Layout(IGraph graph)
        {
            var g = new LayoutGraph(graph);
            g.Init();
            g.Layout();
        }
    }
}
