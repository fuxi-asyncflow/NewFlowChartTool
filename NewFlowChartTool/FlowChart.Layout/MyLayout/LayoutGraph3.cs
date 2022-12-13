using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Common;
using Microsoft.Msagl.Core.DataStructures;

namespace FlowChart.Layout.MyLayout
{
    public class LayoutGroup : IGraph
    {
        public LayoutGroup(IGroup group, LayoutGraph graph)
        {
            GroupNodes = new List<LayoutNode>();
            GroupEdges = new List<LayoutEdge>();
            CrossEdges = new List<LayoutEdge>();
            RootNodes = new Dictionary<LayoutNode, LayoutNode>();
            NodeDict = new Dictionary<INode, LayoutNode>();
            EdgeDict = new Dictionary<IEdge, LayoutEdge>();
            VirtualNodes = new List<LayoutNode>();
            VirtualEdges = new List<LayoutEdge>();
            _nodeSet = new Set<LayoutNode>();
            _group = group;
            _graph = graph;

            Init();
            if (GroupNodes.Count == 0)
                return;
            GetRoots();
            Layout();
            CreateVirtualNodes();
        }

        public void PostProcess()
        {
            if (GroupNodes.Count == 0)
                return;
            CalcNodesXY();
            RestoreNodesAndEdges();
        }

        private LayoutGraph _graph;
        public LayoutGraph Graph => _graph;
        private IGroup _group;
        public List<LayoutNode> GroupNodes { get; set; }
        public List<LayoutEdge> GroupEdges { get; set; }
        public List<LayoutEdge> CrossEdges { get; set; }
        public Dictionary<LayoutNode, LayoutNode> RootNodes { get; set; } // <Root, Parent>
        public Set<LayoutNode> _nodeSet;
        public Dictionary<INode, LayoutNode> NodeDict { get; set; }
        public Dictionary<IEdge, LayoutEdge> EdgeDict { get; set; }
        public List<LayoutNode> VirtualNodes { get; set; }
        public List<LayoutEdge> VirtualEdges { get; set; }

        public double Width { get; set; }
        public double Height { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public IEnumerable<INode> Nodes { get; set; }
        public IEnumerable<IEdge> Edges { get; set; }
        public IEnumerable<IGroup> Groups { get; set; }

        public bool NodeInsideGroup(LayoutNode node)
        {
            return _nodeSet.Contains(node);
        }

        // remove nodes and edge from graph, and save them in LayoutGroup
        void Init()
        {
            // get Nodes
            _group.InsideNodes.ForEach(node =>
            {
                if (_graph.NodeDict.TryGetValue(node, out var layoutNode))
                {
                    GroupNodes.Add(layoutNode);
                    _nodeSet.Insert(layoutNode);
                    _graph.NodeDict.Remove(node);
                    NodeDict.Add(node, layoutNode);
                }
            });

            // get CrossEdges and GroupEdges
            GroupNodes.ForEach(node =>
            {
                node.InEdges.ForEach(edge =>
                {
                    if (NodeInsideGroup(edge.StartNode))
                        GroupEdges.Add(edge);
                    else
                        CrossEdges.Add(edge);
                    _graph.EdgeDict.Remove(edge.Edge);
                    EdgeDict.Add(edge.Edge, edge);
                });

                node.OutEdges.ForEach(edge =>
                {
                    if (!NodeInsideGroup(edge.EndNode))
                    {
                        CrossEdges.Add(edge);
                        _graph.EdgeDict.Remove(edge.Edge);
                        EdgeDict.Add(edge.Edge, edge);
                    }
                });
            });

            // disconnect cross edges should called after GetRoots, because find root's inEdge need it
        }

        void GetRoots()
        {
            // rootNodes are those nodes with no in groupEdges except self-connected edge
            var set = new Set<LayoutNode>(_nodeSet);
            GroupEdges.ForEach(edge =>
            {
                if (edge.StartNode == edge.EndNode)
                    return;
                set.Remove(edge.EndNode);
            });
            foreach (var node in set)
            {
                RootNodes.Add(node, node.InEdges[0].StartNode);
            }

            // if all group nodes has input edges, then find a node with input edge as root node
            //TODO: check if there is a better rule to find root node
            if (RootNodes.Count == 0)
            {
                foreach (var edge in CrossEdges)
                {
                    if (_nodeSet.Contains(edge.EndNode))
                    {
                        RootNodes.Add(edge.EndNode, edge.StartNode);
                        break;
                    }
                }
            }

            Debug.Assert(RootNodes.Count > 0);
            CrossEdges.ForEach(edge => edge.Disconnect());
        }

        void Layout()
        {
            var layoutGraph = new LayoutGraph2(this)
            {
                NodeDict = NodeDict,
                EdgeDict = EdgeDict,
                Setting = Graph.Setting.Clone(),
                Roots = RootNodes.Keys.ToList()
            };
            layoutGraph.Setting.MinWidth = 0.0;
            layoutGraph.Setting.MinHeight = 0.0;
            layoutGraph.Layout();
            Logger.DBG($"layout group {Width} {Height}");
        }

        void AddVirtualEdge(LayoutEdge edge)
        {
            Graph.EdgeDict.Add(edge, edge);
            VirtualEdges.Add(edge);
        }

        void CreateVirtualNodes()
        {
            if (GroupNodes.Count == 0)
                return;
            double nodeHeight = GroupNodes[0].Height;
            double nodeWidth = Width;
            double spaceHeight = Graph.Setting.RankSpace;
            int nodeCount = (int)Math.Ceiling(Height / (nodeHeight + spaceHeight));

            // add virtual nodes
            for (int i = 0; i < nodeCount; i++)
            {
                var newNode = new LayoutNode() {Width = nodeWidth, Height = nodeHeight};
                VirtualNodes.Add(newNode);
                Graph.NodeDict.Add(newNode, newNode);
            }

            // add edge inside virtual nodes
            for (int i = 0; i < nodeCount - 1; i++)
            {
                AddVirtualEdge(new LayoutEdge(VirtualNodes[i], VirtualNodes[i + 1]));
            }

            // add edge between virtual nodes and outside nodes
            AddVirtualEdge(new LayoutEdge(RootNodes.First().Value, VirtualNodes[0]));

            // find children nodes : nodes don't belong group, but all its parent nodes are in group
            var handledNodes = new Set<LayoutNode>();
            CrossEdges.ForEach(edge =>
            {
                var node = edge.EndNode;
                if (NodeInsideGroup(node))
                    return;
                if (handledNodes.Contains(node))
                    return;
                handledNodes.Insert(node);
                if (node.InEdges.All(inEdge => NodeInsideGroup(inEdge.StartNode)))
                {
                    AddVirtualEdge(new LayoutEdge(VirtualNodes.Last(), node));
                }
            });
        }

        void RestoreNodesAndEdges()
        {
            // remove virtual node and edges
            VirtualEdges.ForEach(edge =>
            {
                edge.Disconnect();
                Graph.EdgeDict.Remove(edge);
            });

            VirtualNodes.ForEach(node =>
            {
                Graph.NodeDict.Remove(node);
            });

            // restore 
            GroupNodes.ForEach(node => Graph.NodeDict.Add(node.Node, node));
            GroupEdges.ForEach(edge => Graph.EdgeDict.Add(edge.Edge, edge));
            CrossEdges.ForEach(edge =>
            {
                Graph.EdgeDict.Add(edge.Edge, edge);
                edge.Connect();
            });
        }

        void CalcNodesXY()
        {
            X = VirtualNodes[0].X;
            Y = VirtualNodes[0].Y;
            Logger.DBG($"group t: {Y}, l:{X}");
            GroupNodes.ForEach(node =>
            {
                node.X += X;
                node.Y += Y;
            });
        }
    }

    class MyLayoutGroup : ILayout
    {
        public void Layout(IGraph graph)
        {
            var layoutGraph = new LayoutGraph3(graph);
            layoutGraph.Init();
            layoutGraph.LayoutWithGroup();
        }
    }

    internal class LayoutGraph3 : LayoutGraph2
    {
        public LayoutGraph3(IGraph graph) : base(graph)
        {
            LayoutGroups = new List<LayoutGroup>();
            
        }

        public List<LayoutGroup> LayoutGroups;

        public void LayoutWithGroup()
        {
            PreCalcGroups();
            Acyclic_BFS_Max();
            InitLayout();
            CalcLeftDistance();
            CalcX();
            CalcY();
            PostCalcGroups();
            CalcGraphWidthHeight();
            CalcCubicBezierEdge();
        }

        public void PreCalcGroups()
        {
            foreach (var group in _graph.Groups)
            {
                LayoutGroups.Add(new LayoutGroup(group, this));
            }
        }

        public void PostCalcGroups()
        {
            LayoutGroups.ForEach(group => group.PostProcess());
            LayoutGroups.Clear();
        }
    }
}
