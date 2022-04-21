namespace FlowChart.Layout
{
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