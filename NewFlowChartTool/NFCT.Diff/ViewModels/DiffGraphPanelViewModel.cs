using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using FlowChart.Common;
using FlowChart.Core;
using FlowChart.Diff;
using FlowChart.Layout;
using Prism.Mvvm;

namespace NFCT.Diff.ViewModels
{
    public class DiffNodeViewModel : BindableBase, INode
    {
        static DiffNodeViewModel()
        {
            BackgroundColors = new List<Brush>()
            {
                new SolidColorBrush(Colors.Gray),
                new SolidColorBrush(Colors.LightCoral),
                new SolidColorBrush(Colors.Green),
                new SolidColorBrush(Colors.Gold),
            };
        }
        public DiffNodeViewModel(Node node)
        {
            _node = node;
            Content = node.DisplayString;
            State = DiffState.NoChange;

        }

        private Node _node;
        public DiffState State { get; set; }
        public string Content { get; set; }
        public double Width => ActualWidth;
        public double Height => ActualHeight;


        private double _left;
        public double Left { get => _left; set => SetProperty(ref _left, value, nameof(Left)); }

        private double _top;
        public double Top { get => _top; set => SetProperty(ref _top, value, nameof(Top)); }

        public double X
        {
            set { Left = value; }
            get => _left + 0.5 * ActualWidth;
        }
        public double Y
        {
            set { Top = value; }
            get => _top + 0.5 * ActualHeight;
        }

        private double _actualHeight;
        public double ActualHeight { get => _actualHeight; set => SetProperty(ref _actualHeight, value); }
        private double _actualWidth;
        public double ActualWidth { get => _actualWidth; set => SetProperty(ref _actualWidth, value); }

        public double Opacity => State == DiffState.NoChange ? 0.3 : 1.0;

        public Brush BgColor => GetColor(State);

        public static Brush GetColor(DiffState state)
        {
            switch (state)
            {
                case DiffState.NoChange:
                    return BackgroundColors[0];
                case DiffState.Remove:
                    return BackgroundColors[1];
                case DiffState.Add:
                    return BackgroundColors[2];
                case DiffState.Modify:
                    return BackgroundColors[3];
                default:
                    return BackgroundColors[0];
            }
        }

        public static List<Brush> BackgroundColors;
    }

    public class DiffConnectorViewModel : BindableBase, IEdge
    {
        static DiffConnectorViewModel()
        {
            LineColors = new List<Brush>()
            {
                new SolidColorBrush(Colors.Blue),
                new SolidColorBrush(Colors.Red),
                new SolidColorBrush(Colors.Green),
                new SolidColorBrush(Colors.Gray),
            };
            DashLines = new List<DoubleCollection>()
            {
                new DoubleCollection(){2,2},
                new DoubleCollection(){1,0}
            };
        }

        public DiffConnectorViewModel(DiffNodeViewModel start, DiffNodeViewModel end)
        {
            Start = start;
            End = end;
        }
        public double Opacity => State == DiffState.NoChange ? 0.3 : 1.0;
        public DiffState State { get; set; }
        public Connector.ConnectorType ConnectorType { get; set; }
        public INode Start { get; set; }
        public INode End { get; set; }

        public double Top { get; set; }
        public double Left { get; set; }

        private bool _isSelect;
        public bool IsSelect
        {
            get => _isSelect;
            set
            {
                SetProperty(ref _isSelect, value, nameof(IsSelect));
                if (_isSelect)
                    Console.WriteLine("line is selcected");
            }
        }

        private bool _isFocused;
        public bool IsFocused { get => _isFocused; set => SetProperty(ref _isFocused, value, nameof(IsFocused)); }

        #region DRAW
        public Brush Color => GetColor(ConnectorType);
        public PathGeometry? Path { get; set; }
        public List<Curve> Curves
        {
            set => CreatePath(value);
        }

        private static PathFigure _genArrow(Position s, Position e, double w = 4.0, double h = 8.0)
        {
            var p2 = e;
            var p1 = s;
            double dx = p2.x - p1.x;
            double dy = p2.y - p1.y;

            double l = Math.Sqrt(dx * dx + dy * dy);
            double offsetX = -dy * w / l;
            double offsetY = dx * w / l;


            var seg = new LineSegment();

            Point[] arrowPoints = new Point[3];
            arrowPoints[0].X = p2.x + offsetX - h / l * dx;
            arrowPoints[0].Y = p2.y + offsetY - h / l * dy;
            //arrowPoints[3].X = p2.X + offsetX;
            //arrowPoints[3].Y = p2.Y + offsetY;

            arrowPoints[2].X = p2.x - offsetX - h / l * dx;
            arrowPoints[2].Y = p2.y - offsetY - h / l * dy;

            arrowPoints[1].X = p2.x;
            arrowPoints[1].Y = p2.y;


            PathFigure figure = new PathFigure(arrowPoints[0], new[] { new LineSegment(arrowPoints[1], true), new LineSegment(arrowPoints[2], true) }, true);
            return figure;
        }

        public void CreatePath(List<Curve> curves)
        {
            PathGeometry geometry = new PathGeometry();
            if (curves.Count == 0)
            {
                Logger.ERR("[canvas] connector path has no curve");
                return;
            }

            // get left and top of all curve points
            Left = double.MaxValue;
            Top = double.MaxValue;
            foreach (var curve in curves)
            {
                foreach (var p in curve.Points)
                {
                    if (p.x < Left) Left = p.x;
                    if (p.y < Top) Top = p.y;
                }
            }

            foreach (var curve in curves)
            {
                for (var index = 0; index < curve.Points.Count; index++)
                {
                    var p = curve.Points[index];
                    curve.Points[index] = new Position(p.x - Left, p.y - Top);
                }
            }


            // Console.WriteLine("=================");
            foreach (var curve in curves)
            {
                if (curve.Type == Curve.CurveType.Line)
                {
                    var segments = new List<PathSegment>();
                    var s = curve.Points[0];
                    var e = curve.Points[1];

                    segments.Add(new LineSegment(new Point(e.x, e.y), true));
                    PathFigure figure = new PathFigure(new Point(s.x, s.y), segments, false);

                    geometry.Figures.Add(figure);
                }
                else if (curve.Type == Curve.CurveType.SPLINE)
                {
                    var segments = new List<PathSegment>();
                    var s = curve.Points[0];

                    //curve.Points.ForEach(p => Console.WriteLine($"{p}"));
                    var seg = new PolyBezierSegment
                    {
                        Points = new PointCollection(curve.Points.Skip(1).ToList().ConvertAll(p => new Point(p.x, p.y)))
                    };
                    segments.Add(seg);
                    PathFigure figure = new PathFigure(new Point(s.x, s.y), segments, false);
                    geometry.Figures.Add(figure);
                }
                else if (curve.Type == Curve.CurveType.Ellipse)
                {
                    if (curve.Parameters == null)
                        continue;
                    List<double> p = curve.Parameters;
                    var angle = p[2];
                    var segments = new List<PathSegment>();
                    var s = curve.Points[0];

                    var seg = new ArcSegment()
                    {
                        Point = new Point(curve.Points[1].x, curve.Points[1].y),
                        Size = new Size(Math.Abs(p[0]), Math.Abs(p[1])),
                        IsLargeArc = Math.Abs(angle) >= Math.PI,
                        SweepDirection = angle < 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise
                    };
                    segments.Add(seg);
                    PathFigure figure = new PathFigure(new Point(s.x, s.y), segments, false);
                    geometry.Figures.Add(figure);
                }

            }

            var endCurve = curves.Last();
            if (endCurve.Type == Curve.CurveType.Line)
            {
                geometry.Figures.Add(_genArrow(endCurve.Points[0], endCurve.Points[1]));
            }
            else if (endCurve.Type == Curve.CurveType.SPLINE)
            {
                int pointCount = endCurve.Points.Count;
                geometry.Figures.Add(_genArrow(endCurve.Points[pointCount - 2], endCurve.Points[pointCount - 1]));
            }

            Path = geometry;
            RaisePropertyChanged(nameof(Path));
            RaisePropertyChanged(nameof(Left));
            RaisePropertyChanged(nameof(Top));
        }

        public void StraightLineConnect(Point start, Point end)
        {
            var curves = new List<Curve>();
            curves.Add(new Curve() { Type = Curve.CurveType.Line, Points = new List<Position>() { new(start.X, start.Y), new(end.X, end.Y) } });
            CreatePath(curves);
        }

        #endregion

        private static List<Brush> LineColors;
        public static Brush GetColor(Connector.ConnectorType type)
        {
            switch (type)
            {
                case Connector.ConnectorType.ALWAYS:
                    return LineColors[0];
                case Connector.ConnectorType.FAILURE:
                    return LineColors[1];
                case Connector.ConnectorType.SUCCESS:
                    return LineColors[2];
                default:
                    return LineColors[3];
            }
        }

        public DoubleCollection Dash => State == DiffState.Remove ? DashLines[0] : DashLines[1];

        private static List<DoubleCollection> DashLines;
    }

    internal class DiffGraphLayoutAdapter : IGraph
    {
        public DiffGraphLayoutAdapter(DiffGraphPanelViewModel vm)
        {
            _g = vm;
            var nodes = new List<INode>();
            nodes.AddRange(vm.Nodes);
            _nodes = nodes;
            var edges = new List<IEdge>();
            edges.AddRange(vm.Connectors);
            _edges = edges;
            _groups = new List<IGroup>();
        }

        private DiffGraphPanelViewModel _g;

        public IEnumerable<INode> Nodes => _nodes;
        public IEnumerable<IEdge> Edges => _edges;
        public IEnumerable<IGroup> Groups => _groups;
        public double Width { set => _g.Width = value; }
        public double Height { set => _g.Height = value; }

        public IEnumerable<INode> _nodes;
        public IEnumerable<IEdge> _edges;
        public IEnumerable<IGroup> _groups;
    }

    public class DiffGraphPanelViewModel: BindableBase
    {
        public DiffGraphPanelViewModel(DiffGraph graph)
        {
            Graph = graph;
            Scale = 1.0;
            Nodes = new ObservableCollection<DiffNodeViewModel>();
            ChangedNodes = new List<DiffNodeViewModel>();
            Connectors = new ObservableCollection<DiffConnectorViewModel>();
            _layout = LayoutManager.Instance.LayoutDict["layout_group"].Invoke();
            Init(graph);
            NeedLayout = true;
        }

        private void Init(DiffGraph graph)
        {
            if (graph.State == DiffState.NoChange || graph.State == DiffState.Rename)
                return;
            if (graph.State == DiffState.Add)
            {

            }
            else if (graph.State == DiffState.Remove)
            {

            }
            else if (graph.State == DiffState.Modify)
            {
                if (graph.OldGraph == null)
                    return;
                var nodeDict = new Dictionary<Guid, DiffNodeViewModel>();
                graph.OldGraph.Nodes.ForEach(node =>
                {
                    var dnVm = new DiffNodeViewModel(node);
                    nodeDict.Add(node.Uid, dnVm);
                });
                graph.Nodes.ForEach(node =>
                {
                    if (node.State == DiffState.Modify)
                    {
                        var dnVm = nodeDict[node.OldNode.Uid];
                        dnVm.State = node.State;
                        ChangedNodes.Add(dnVm);
                    }
                    else if (node.State == DiffState.Add)
                    {
                        var dnVm = new DiffNodeViewModel(node.NewNode);
                        dnVm.State = DiffState.Add;
                        nodeDict.Add(node.NewNode.Uid, dnVm);
                        ChangedNodes.Add(dnVm);
                    }
                    else if (node.State == DiffState.Remove)
                    {
                        var dnVm = nodeDict[node.OldNode.Uid];
                        dnVm.State = node.State;
                        ChangedNodes.Add(dnVm);
                    }
                });
                foreach (var diffNodeViewModel in nodeDict.Values)
                {
                    Nodes.Add(diffNodeViewModel);
                }

                Graph.UnchangedConnectors.ForEach(conn =>
                {
                    Connectors.Add(new DiffConnectorViewModel(nodeDict[conn.OldConn.Start.Uid], nodeDict[conn.OldConn.End.Uid]){State = DiffState.NoChange});
                });

                Graph.Connectors.ForEach(conn =>
                {
                    if (conn.State == DiffState.Modify)
                    {
                        Connectors.Add(new DiffConnectorViewModel(nodeDict[conn.OldConn.Start.Uid], nodeDict[conn.OldConn.End.Uid]) { State = DiffState.Modify, ConnectorType = conn.NewConn.ConnType});
                    }
                    else if (conn.State == DiffState.Add)
                    {
                        Connectors.Add(new DiffConnectorViewModel(nodeDict[conn.NewConn.Start.Uid], nodeDict[conn.NewConn.End.Uid]) { State = DiffState.Add, ConnectorType = conn.NewConn.ConnType });
                    }
                    else if (conn.State == DiffState.Remove)
                    {
                        Connectors.Add(new DiffConnectorViewModel(nodeDict[conn.OldConn.Start.Uid], nodeDict[conn.OldConn.End.Uid]) { State = DiffState.Remove, ConnectorType = conn.OldConn.ConnType });
                    }
                });
            }
        }

        public DiffGraph Graph { get; set; }

        public ObservableCollection<DiffNodeViewModel> Nodes { get; set; }
        public ObservableCollection<DiffConnectorViewModel> Connectors { get; set; }
        public List<DiffNodeViewModel> ChangedNodes { get; set; }
        public List<DiffConnectorViewModel> ChangedConnectors { get; set; }

        #region SCROLL

        private double _scrollX;
        private double _scrollY;

        public double ScrollX
        {
            get => _scrollX;
            set => SetProperty(ref _scrollX, value);
        }

        public double ScrollY
        {
            get => _scrollY;
            set => SetProperty(ref _scrollY, value);
        }

        private double _scrollViewerWidth;
        private double _scrollViewerHeight;

        public double ScrollViewerWidth
        {
            get => _scrollViewerWidth;
            set => SetProperty(ref _scrollViewerWidth, value);
        }

        public double ScrollViewerHeight
        {
            get => _scrollViewerHeight;
            set => SetProperty(ref _scrollViewerHeight, value);
        }

        private bool _observeScrollSize;
        public bool ObserveScrollSize { get => _observeScrollSize; set => SetProperty(ref _observeScrollSize, value); }

        #endregion

        private double _width;
        public double Width
        {
            get => _width;
            set
            {
                SetProperty(ref _width, value);
                RaisePropertyChanged(nameof(ScaledWidth));
            }
        }

        public double _height;
        public double Height
        {
            get => _height;
            set
            {
                SetProperty(ref _height, value);
                RaisePropertyChanged(nameof(ScaledHeight));
            }
        }

        public double ScaledWidth => Width * Scale;
        public double ScaledHeight => Height * Scale;
        private ILayout _layout;
        public static double ScaleMax => 2.0;
        public static double ScaleMin => 0.05;
        private double _scale;
        public double Scale
        {
            get => _scale;
            set
            {
                SetProperty(ref _scale, value);
                RaisePropertyChanged(nameof(ScaledWidth));
                RaisePropertyChanged(nameof(ScaledHeight));
            }
        }

        public Node? InitCenterNode { get; set; }
        public bool NeedLayout { get; set; }
        public bool IsFirstLayout { get; set; }

        public void MoveNodeToCenter(DiffNodeViewModel? nodeVm)
        {
            if (nodeVm == null)
                return;
            ScrollX = nodeVm.Left + (nodeVm.Width - ScrollViewerWidth) / 2;
            ScrollY = nodeVm.Top - ScrollViewerHeight / 2;
            Logger.DBG($"MoveNodeToCenter {nodeVm.Left} {ScrollViewerWidth} {nodeVm.Content} {ScrollX} {ScrollY}");
        }

        public bool Relayout()
        {
            var graph = new DiffGraphLayoutAdapter(this);
            if (Nodes.Count <= 0.0 || Nodes.First().Width <= 0.0)
                return false;
            //var layout = new MsaglLayout();
            //var layout = new MyLayout();
            //var layout = new MyLayout2();
            try
            {
                _layout.Layout(graph);
                if (Height < 0.001 || Width < 0.001)
                {
                    return false;
                }

                //if (InitCenterNode != null)
                //{
                //    MoveNodeToCenter(InitCenterNode);
                //    InitCenterNode = null;
                //}
#if DEBUG
                foreach (var nodeVm in Nodes)
                {
                    //nodeVm.ToolTip = $"t:{nodeVm.Top:F2} l:{nodeVm.Left:F2} w:{nodeVm.ActualWidth:F2} h:{nodeVm.ActualHeight:F2}";
                }
#endif
            }
            catch (Exception e)
            {
                Logger.ERR($"[error] layout failed {e.Message}");
#if DEBUG
                throw;
#endif
                return false;
            }

            return true;
        }
    }
}
