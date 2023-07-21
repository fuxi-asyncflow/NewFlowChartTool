using FlowChart.Layout.MyLayout;

namespace FlowChart.Layout
{
    public struct Position
    {
        public Position(double _x, double _y) { x = _x; y = _y; }
        public double x;
        public double y;

        public override string ToString()
        {
            return $"[{x}, {y}]";
        }

        public void Minus(double a, double b)
        {
            x -= a;
            y -= b;
        }
    }

    public class Curve
    {
        public Curve()
        {
            Points = new List<Position>();
        }
        public enum CurveType
        {
            Line = 0,
            SPLINE = 1,
            Ellipse = 2
        }
        public CurveType Type { get; set; }
        public List<Position> Points { get; set; }
        public List<double>? Parameters { get; set; }
    }

    public interface INode
    {
        public double Width { get; }
        public double Height { get; }
        public double X { set; }
        public double Y { set; }
        public bool IsSelect { get; set; }
    }

    public interface IEdge
    {
        public INode Start { get; }
        public INode End { get; }
        public List<Curve> Curves { set; }
    }

    public interface IGroup
    {
        public List<INode> InsideNodes { get; }
    }

    public interface IGraph
    {
        public IEnumerable<INode> Nodes { get; }
        public IEnumerable<IEdge> Edges { get; }
        public IEnumerable<IGroup> Groups { get; }
        double Width { set; }
        double Height { set; }
    }

    public interface ILayout
    {
        public void Layout(IGraph graph);

        public void RedrawConnectPin(IGraph graph);
    }

    public interface IDrawCurve
    {
        public Curve DrawCurve(LayoutNode StartNode, LayoutNode EndNode);

    }
}