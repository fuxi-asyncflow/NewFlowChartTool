﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Layout.MyLayout
{
    public class MyLayout2 : ILayout
    {
        public void Layout(IGraph graph)
        {
            var g = new LayoutGraph2(graph);
            g.Layout();
        }
    }
    class DummyNode
    {
        public DummyNode()
        {
            Nodes = new List<LayoutNode>();
        }
        public List<LayoutNode> Nodes { get; set; }

        public double GetLeftPos(LayoutNode node)
        {
            var result = 0.0;
            foreach (var curNode in Nodes)
            {
                if (curNode == node)
                    return result + curNode.LeftDistance;
                result += (curNode.LeftDistance + curNode.Width);
            }
            return -1.0f;
        }

        public LayoutNode? GetRightNode(LayoutNode node)
        {
            var idx = Nodes.IndexOf(node);
            if(idx == -1 || idx == Nodes.Count -1)
                return null;
            return Nodes[idx+1];
        }
    }
    public class LayoutGraph2 : LayoutGraph
    {
        private List<DummyNode> leftEdge;
        public LayoutGraph2(IGraph graph) : base(graph)
        {
            leftEdge = new List<DummyNode>();
        }

        public override void Layout()
        {
            Acyclic_BFS();
            InitLayout();
            CalcLeftDistance();
            CalcX();
            CalcY();
            CalcGraphWidthHeight();

            CalcEdges();
        }

        void InitLayout()
        {
            leftEdge.Clear();
            for (int i = 0; i <= MaxRank; i++)
            {
                leftEdge.Add(new DummyNode());
            }
            
            Roots.ForEach(_initLayout);
        }

        void _initLayout(LayoutNode root)
        {
            // bfs
            leftEdge[root.Rank].Nodes.Add(root);
            root.LeftDistance = Setting.NodeSpace;
            root.OutputEdges.ForEach(edge => _initLayout(edge.End));
        }

        void CalcLeftDistance()
        {
            for (int rank = MaxRank; rank >= 0; rank--)
            {
                var list = leftEdge[rank].Nodes;
                foreach (var node in list)
                {
                    Expand(node);
                }
            }
        }

        void Expand(LayoutNode root)
        {
            if (root.IsLeaf)
                return;
            var rootMid = leftEdge[root.Rank].GetLeftPos(root) + root.Width * 0.5f;
            var firstChild = root.OutputEdges[0].End;
            var childLeft = leftEdge[firstChild.Rank].GetLeftPos(firstChild);
            

            var childRight = childLeft - firstChild.LeftDistance;
            root.OutputEdges.ForEach(edge =>
            {
                var node = edge.End;
                childRight += (node.LeftDistance + node.Width);
            });

            var childMid = (childLeft + childRight) / 2;
            if (rootMid > childMid) // move child
            {
                var delta = rootMid - childMid;
                UpdateDown(root, delta);

            }
            else if (rootMid < childMid)
            {
                var delta = childMid - rootMid;
                root.LeftDistance += delta;
                //UpdateUp(root, delta);
            }
        }

        void UpdateDown(LayoutNode node, double delta)
        {
            if (node.IsLeaf)
            {
                if(node.Rank != MaxRank)
                {
                    var nextNode = leftEdge[node.Rank].GetRightNode(node);
                    if (nextNode != null)
                        UpdateDown(nextNode, delta);
                }
                return;
            }

            var child = node.OutputEdges[0].End;
            child.LeftDistance += delta;
            UpdateDown(child, delta);
        }

        void CalcX()
        {
            foreach (var dummyNode in leftEdge)
            {
                double left = 0;
                foreach (var node in dummyNode.Nodes)
                {
                    var x = left + node.LeftDistance;
                    node.X = x;
                    left = x + node.Width;
                }
            }
        }

        void CalcY()
        {
            Roots.ForEach(node => _calcY(node, 0.0));
        }

        void _calcY(LayoutNode node, double nodeY)
        {
            node.Y = nodeY;
            var y = nodeY + node.Height + Setting.RankSpace;
            node.OutputEdges.ForEach(edge =>
            {
                _calcY(edge.End, y);
            });
        }

        void CalcGraphWidthHeight()
        {
            double width = 0.0;
            foreach (var dummyNode in leftEdge)
            {
                var node = dummyNode.Nodes.Last();
                if (width < node.X + node.Width)
                {
                    width = node.X + node.Width;
                }
            }

            double height = 0.0;
            foreach (var node in leftEdge[MaxRank].Nodes)
            {
                if(height < node.Y + node.Height)
                    height = node.Y + node.Height;
            }

            if (width < Setting.MinWidth)
            {
                var offsetX = (Setting.MinWidth - width) / 2;
                foreach (var node in NodeDict.Values)
                {
                    node.X += offsetX;
                }

                _graph.Width = Setting.MinWidth;
            }
            else
                _graph.Width = width;

            if (height < Setting.MinHeight)
            {
                var offsetY = (Setting.MinHeight - height) / 2;
                foreach (var node in NodeDict.Values)
                {
                    node.Y += offsetY;
                }
                _graph.Height = height;
            }
            else
                _graph.Height = height;
        }
    }
}