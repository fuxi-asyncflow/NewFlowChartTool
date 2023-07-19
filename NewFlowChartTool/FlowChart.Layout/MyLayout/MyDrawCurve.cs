using Microsoft.Msagl.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Layout.MyLayout
{
    public class MyDrawCurve : IDrawCurve
    {
        #region Defaule Value
        public const double Default_Angle = 90;
        public const double StartNodeXoffset = 10;
        public const double Max_X_Offset = 160;
        public const double Min_Y_Offset = 80;
        public const double Max_Y_Offset = 150;
        public const double LinkBackXOffset = 20;
        #endregion

        LayoutGraphSetting Setting;
        public MyDrawCurve()
        {
            Setting = new LayoutGraphSetting();
        }
        public LayoutNode StartNode { set; get; }
        public LayoutNode EndNode { set; get; }

        private double StartX;
        private double EndX;

        public Curve DrawCurve(LayoutNode sNode, LayoutNode eNode)
        {
            StartNode = sNode;
            EndNode = eNode;
            StartX = StartNode.IsSelect == true ? CalcStartNodeX() : StartNode.X + StartNode.Width * 0.5;
            EndX = EndNode.X + EndNode.Width * 0.5;

            var Yoffset = Min_Y_Offset;
            var Xoffset = (StartX + EndX) / 2 - Math.Min(StartX, EndX);

            Position startNodeCenter = new Position((StartNode.X + StartNode.Width / 2), (StartNode.Y + StartNode.Height / 2));
            Position endNodeCenter = new Position((EndNode.X + EndNode.Width / 2), (EndNode.Y + EndNode.Height / 2));

            if (Math.Abs(StartNode.X + StartNode.Width * 0.5 - EndX) < LinkBackXOffset && (EndNode.Rank - StartNode.Rank) != 1)
            {
                return DrawVerticalNode();
            }
            else
            {
                if (Xoffset > Yoffset)
                    Yoffset = Xoffset > 2 * StartNode.Width ? StartNode.Width : Xoffset;

                Xoffset = Xoffset > Max_X_Offset ? Max_X_Offset : Xoffset;
                Yoffset = Yoffset > Max_Y_Offset ? Max_Y_Offset : Yoffset;

                var yCenterOffset = endNodeCenter.y - startNodeCenter.y;
                var xCenterOffset = endNodeCenter.x - startNodeCenter.x;

                var firstControlXoffset = 0.0;
                var secondControlXoffset = 0.0;
                if (yCenterOffset < 0)
                {
                    if(xCenterOffset > 0)
                    {
                        if(xCenterOffset <= Setting.NodeSpace * 2)
                        {
                            var d = xCenterOffset + LinkBackXOffset;
                            firstControlXoffset = -d;
                            secondControlXoffset = -d;
                        }
                        else
                        {
                            firstControlXoffset = Xoffset;
                            secondControlXoffset = -Xoffset;
                        }
                    }
                    else
                    {
                        if (xCenterOffset >= -Setting.NodeSpace * 2)
                        {
                            var d = LinkBackXOffset - xCenterOffset;
                            firstControlXoffset = d;
                            secondControlXoffset = d;
                        }
                        else
                        {
                            firstControlXoffset = -Xoffset;
                            secondControlXoffset = Xoffset;
                        }
                    }
                }
                else
                {
                    return CreateLine(new Position(StartX, StartNode.Y + StartNode.Height),
                                      new Position(EndX, EndNode.Y));
                }
                return CreateCurve(firstControlXoffset, secondControlXoffset, Yoffset);
            }
        }

        Curve DrawVerticalNode()
        {
            const double NullNodeTextNodeDiffVal = 10;
            const double Max_Y_Offset_Vertical = 20;
            //startNode.y - endNode.y = 2 * NodeSpace + node.height  need yoffset reduction
            double NodesHightDiffVal = 80 + StartNode.Height;
            double Yoffset;
            var nodesHeight = EndNode.Y - StartNode.Y - StartNode.Height;
            if (nodesHeight > 0 && nodesHeight - NodesHightDiffVal < NullNodeTextNodeDiffVal)
                Yoffset = Max_Y_Offset_Vertical;
            else
                Yoffset = Setting.NodeSpace * 2;
            var Xoffset = nodesHeight * 0.25 < Setting.NodeSpace * 2 ? Setting.NodeSpace * 2 : nodesHeight * 0.25;
            return CreateCurve(Xoffset, Xoffset, Yoffset);
        }

        /// <summary>
        /// click node,calc startX of line 
        /// </summary>
        /// <returns></returns>
        public double CalcStartNodeX()
        {
            double startX = StartNode.X + StartNode.Width * 0.5;

            List<LayoutEdge> edges = new List<LayoutEdge>();
            //layout use Alloutedges,IsGroup
            StartNode.AllOutEdges.ForEach(n => { if (n.EndNode.IsGroup == false) edges.Add(n); });
            int index = edges.FindIndex(n => n.EndNode == EndNode);
            int outEdgesCount = edges.Count;
            
            double length = (outEdgesCount - 1) * StartNodeXoffset;
            if(length > StartNode.Width)
            {
                startX = StartNode.X + index * (StartNode.Width / (outEdgesCount - 1));
            }
            else
                startX = startX - length / 2 + index * StartNodeXoffset;
           
            return startX;
        }

        public Curve CreateCurve(double firstControlXoffset, double secondControlXoffset, double Yoffset)
        {
            var curve = new Curve() { Type = Curve.CurveType.SPLINE };
            curve.Points.Add(new Position(StartX, StartNode.Y + StartNode.Height));
            curve.Points.Add(new Position(StartX + firstControlXoffset, StartNode.Y + StartNode.Height + Yoffset));
            curve.Points.Add(new Position(EndX + secondControlXoffset, EndNode.Y - Yoffset));
            curve.Points.Add(new Position(EndX, EndNode.Y));
            return curve;
        }

        public Curve CreateLine(Position startPoint, Position endPoint)
        {
            var curve = new Curve() { Type = Curve.CurveType.Line };
            curve.Points.Add(startPoint);
            curve.Points.Add(endPoint);
            return curve;
        }
    }
}
