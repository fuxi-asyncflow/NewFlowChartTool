namespace FlowChart.Layout
{
    public struct Position
    {
        public Position(double _x, double _y) { x = _x; y = _y; }
        public double x;
        public double y;
    }

    public interface INode
    {
        public double Width { get; }
        public double Height { get; }
        public double X { set; }
        public double Y { set; }
    }

    public interface IEdge
    {
        public INode Start { get; }
        public INode End { get; }
        public List<Position> PathPoints { set; }
    }
    public interface IGraph
    {
        public IEnumerable<INode> Nodes { get; }
        public IEnumerable<IEdge> Edges { get; }
        double Width { set; }
        double Height { set; }
    }

    public interface ILayout
    {
        public void Layout(IGraph graph);
    }
}