using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Layout.Layered;

using GraphNode = Microsoft.Msagl.Core.Layout.Node;

namespace FlowChart.Layout
{
    public class MsaglLayout : ILayout
    {
        GeometryGraph CreateGraph(IGraph graph)
        {
            var gg = new GeometryGraph();

            var nodeDict = new Dictionary<INode, GraphNode>();

            foreach (var n in graph.Nodes)
            {
                var node = new GraphNode(CurveFactory.CreateRectangle(n.Width, n.Height, new Point()), n);
                nodeDict.Add(n, node);
                gg.Nodes.Add(node);
            }

            foreach (var conn in graph.Edges)
            {
                gg.Edges.Add(new Edge(nodeDict[conn.Start], nodeDict[conn.End]));
            }
            return gg;
        }
        public void Layout(IGraph _graph)
        {
            var graph = CreateGraph(_graph);

            // 准备 settings
            var settings = new SugiyamaLayoutSettings
            {
                // Transformation 用于控制布局方向 0 - TB, PI - BT, PI/2 - LR, -PI/2 - RL
                //Transformation = PlaneTransformation.Rotation(Math.PI / 2),
                EdgeRoutingSettings = { EdgeRoutingMode = EdgeRoutingMode.SugiyamaSplines }
            };

            // 计算布局
            var layout = new LayeredLayout(graph, settings);
            layout.Run();

            // ox 和 oy 用于画布左上角的归0
            double ox = graph.BoundingBox.Left;
            double oy = graph.BoundingBox.Top;

            Func<double, float> transY = _y => (float)(oy - _y);

            // 设置node 坐标
            foreach (var n in _graph.Nodes)
            {
                var node = graph.FindNodeByUserData(n);                
                n.X = (float)(node.BoundingBox.Left - ox);
                n.Y = transY(node.BoundingBox.Top);                
            }

            // 设置线条坐标
            foreach (var edge in graph.Edges)
            {
                var edgeNodes = edge.UnderlyingPolyline.ToList();
                var points = new List<Point>();
                points.Add(edgeNodes.First());
                points.Add(edgeNodes.Last());
                //var connector = c.FindConnector(edge.Source.UserData.ToString(), edge.Target.UserData.ToString());
                //connector.Points = points.ConvertAll(p => new DrawPoint(p.X - ox, transY(p.Y)));
            }
            Console.WriteLine($"{graph.BoundingBox.Left}, {graph.BoundingBox.Top}, {graph.BoundingBox.Right}, {graph.BoundingBox.Bottom}");
            
            _graph.Width = (float)graph.BoundingBox.Width;
            _graph.Height = (float)graph.BoundingBox.Height;
        }

       
    }
}
