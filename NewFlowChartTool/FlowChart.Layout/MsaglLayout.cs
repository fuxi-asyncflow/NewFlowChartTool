using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChartCommon;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Layout.Layered;

using GraphNode = Microsoft.Msagl.Core.Layout.Node;
using GraphCurve = Microsoft.Msagl.Core.Geometry.Curves.Curve;

namespace FlowChart.Layout
{
    public class MsaglLayout : ILayout
    {
        private static double MinWidth = 1920.0;
        private static double MinHeight = 1080.0;
        private static double Padding = 64.0;
        private Dictionary<INode, GraphNode> _nodeDict;

        GeometryGraph CreateGraph(IGraph graph)
        {
            _nodeDict = new Dictionary<INode, Node>();
            var gg = new GeometryGraph();

            foreach (var n in graph.Nodes)
            {
                var node = new GraphNode(CurveFactory.CreateRectangle(n.Width, n.Height, new Point()), n);
                // Console.WriteLine($"layout node size: {n.Width} {n.Height}");
                _nodeDict.Add(n, node);
                gg.Nodes.Add(node);
            }

            foreach (var conn in graph.Edges)
            {
                
                gg.Edges.Add(new Edge(_nodeDict[conn.Start], _nodeDict[conn.End]) { UserData = conn });
            }
            return gg;
        }

        static Position ToPosition(Point p)
        {
            return new Position(p.X, p.Y);
        }

        static FlowChart.Layout.Curve ToCurve(ICurve curve)
        {
            if(curve is LineSegment line)
            {
                var c = new Curve() { Type = Curve.CurveType.Line };
                c.Points.Add(ToPosition(line.Start));
                c.Points.Add(ToPosition(line.End));
                return c;

            }
            else if (curve is CubicBezierSegment bezier)
            {
                var c = new Curve() { Type = Curve.CurveType.SPLINE };
                for (int i = 0; i < 4; i++)
                {
                    c.Points.Add(ToPosition(bezier.B(i)));
                }
                return c;
            }
            else if (curve is Ellipse ellipse)
            {
                var c = new Curve() { Type = Curve.CurveType.Ellipse };
                c.Points.Add(ToPosition(ellipse.Start));
                c.Points.Add(ToPosition(ellipse.End));
                //c.Points.Add(ToPosition(ellipse.Center));
                //c.Points.Add(ToPosition(ellipse.AxisA));
                //c.Points.Add(ToPosition(ellipse.AxisB));
                //c.Points.Add(new Position(ellipse.ParStart, ellipse.ParEnd));
                c.Parameters = new List<double>();
                c.Parameters.Add(ellipse.AxisA.X);
                c.Parameters.Add(ellipse.AxisB.Y);
                // sweep angle, see EllipseSweepAngle function in msagl source code
                var isCounterClockWise = ellipse.OrientedCounterclockwise() ? 1.0 : -1.0;
                var sweepAngle = (ellipse.ParEnd - ellipse.ParStart) * isCounterClockWise;
                c.Parameters.Add(sweepAngle);
                return c;
            }

            Logger.ERR($"[layout] unsupported curve type: {curve.GetType()}");

            return null;

        }
        public void Layout(IGraph _graph)
        {
            
            var graph = CreateGraph(_graph);

            // 准备 settings
            var settings = new SugiyamaLayoutSettings
            {
                // Transformation 用于控制布局方向 0 - TB, PI - BT, PI/2 - LR, -PI/2 - RL
                //Transformation = PlaneTransformation.Rotation(Math.PI / 2),
                EdgeRoutingSettings = { EdgeRoutingMode = EdgeRoutingMode.SugiyamaSplines },
                NodeSeparation = 30
            };

            foreach (var graphNode in _graph.Nodes)
            {
                var childNodes = graphNode.Children;
                if (childNodes.Count > 1)
                {
                    for (int i = 1; i < childNodes.Count; i++)
                        settings.AddLeftRightConstraint(_nodeDict[childNodes[i - 1]], _nodeDict[childNodes[i]]);
                }
            }

            // 计算布局
            var layout = new LayeredLayout(graph, settings);
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            layout.Run();
            stopWatch.Stop();
            Logger.LOG($"layout time: {stopWatch.ElapsedMilliseconds}");

            // ox 和 oy 用于画布左上角的归0
            double ox = graph.BoundingBox.Left;
            double oy = graph.BoundingBox.Top;

            var actualWidth = graph.BoundingBox.Width + Padding * 2.0;
            if (actualWidth < MinWidth)
            {
                ox -= (MinWidth - actualWidth) * 0.5f;
                actualWidth = MinWidth;
            }
            else
            {
                ox -= Padding;
            }

            var actualHeight = graph.BoundingBox.Height + Padding * 2.0;
            if (actualHeight < MinHeight)
                actualHeight = MinHeight;

            oy += Padding;

            Func<double, double> transX = _X => (_X - ox);
            Func<double, double> transY = _y => (oy - _y);

            // 设置node 坐标
            foreach (var n in _graph.Nodes)
            {
                var node = graph.FindNodeByUserData(n);                
                n.X = transX(node.BoundingBox.Left);
                n.Y = transY(node.BoundingBox.Top);
                //Console.WriteLine($"++++++++ {n.Width - node.BoundingBox.Width} {n.Height - node.BoundingBox.Height}");
                //Console.WriteLine($"[NODE] {node.BoundingBox} {n.Width} {n.Height}");
            }

            // 设置线条坐标
            foreach (var edge in graph.Edges)
            {
                var connector = (IEdge)edge.UserData;
                var curves = new List<Curve>();

                if (edge.Curve is LineSegment)
                {
                    curves.Add(ToCurve(edge.Curve));
                }
                else if(edge.Curve is GraphCurve)
                {
                    var c = (GraphCurve)edge.Curve;
                    foreach (var seg in c.Segments)
                    {
                        curves.Add(ToCurve(seg));
                    }
                }

                curves.ForEach(curv => curv.Points = curv.Points.ConvertAll(p => new Position(transX(p.x), transY(p.y))));
                connector.Curves = curves;
               
            }
            // Console.WriteLine($"{graph.BoundingBox.Left}, {graph.BoundingBox.Top}, {graph.BoundingBox.Right}, {graph.BoundingBox.Bottom}");
            
            _graph.Width = actualWidth;
            _graph.Height = actualHeight;
            //Console.WriteLine($"canvas width height {graph.BoundingBox.Width}, {graph.BoundingBox.Height}");
        }

       
    }
}
