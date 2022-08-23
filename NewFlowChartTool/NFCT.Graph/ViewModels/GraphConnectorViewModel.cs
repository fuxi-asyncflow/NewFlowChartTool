using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using FlowChart.Layout;
using FlowChart.Core;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FlowChartCommon;
using Prism.Commands;


namespace NFCT.Graph.ViewModels
{
    public class GraphConnectorViewModel : BindableBase, IEdge
    {
        public INode Start => StartNode;
        public INode End => EndNode;
        public Connector.ConnectorType ConnType => _conn.ConnType;

        public BaseNodeViewModel StartNode { get; set; }
        public BaseNodeViewModel EndNode { get; set; }

        public double X => (StartNode.X + EndNode.X) * 0.5f;
        public double Y => (StartNode.Y + EndNode.Y) * 0.5f;

        public double Left { get; set; }
        public double Top { get; set; }

        public GraphConnectorViewModel(Connector conn, BaseNodeViewModel start, BaseNodeViewModel end, GraphPaneViewModel g)
        {
            StartNode = start;
            EndNode = end;
            Owner = g;
            _conn = conn;
            start.ChildLines.Add(this);
            end.ParentLines.Add(this);

            OnMouseUpCommand = new DelegateCommand<MouseEventArgs>(OnMouseUp);
        }

        public GraphPaneViewModel Owner { get; set; }
        private Connector _conn;
        public Connector Connector => _conn;

        private bool _isSelect;
        public bool IsSelect
        {
            get => _isSelect;
            set
            {
                SetProperty(ref _isSelect, value, nameof(IsSelect));
                if(_isSelect)
                    Console.WriteLine("line is selcected");
            }
        }

        private bool _isFocused;
        public bool IsFocused { get => _isFocused; set => SetProperty(ref _isFocused, value, nameof(IsFocused)); }

        #region DRAW
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


            PathFigure figure = new PathFigure(arrowPoints[0], new []{ new LineSegment(arrowPoints[1], true), new LineSegment(arrowPoints[2], true)}, true);
            return figure;
        }

        public void CreatePath(List<Curve> curves)
        {
            PathGeometry geometry = new PathGeometry();
            if (curves.Count == 0)
                return;

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
                        Points = new PointCollection(curve.Points.Skip(1).ToList().ConvertAll( p => new Point(p.x, p.y)))
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
            else if(endCurve.Type == Curve.CurveType.SPLINE)
            {
                int pointCount = endCurve.Points.Count;
                geometry.Figures.Add(_genArrow(endCurve.Points[pointCount-2], endCurve.Points[pointCount-1]));
            }

            Path = geometry;
            RaisePropertyChanged(nameof(Path));
            RaisePropertyChanged(nameof(Left));
            RaisePropertyChanged(nameof(Top));
        }

        public void StaightLineConnect(Point start, Point end)
        {
            var curves = new List<Curve>();
            curves.Add(new Curve(){Type = Curve.CurveType.Line, Points = new List<Position>() {new(start.X, start.Y), new (end.X, end.Y)}});
            CreatePath(curves);
        }

        #endregion

        public void OnConnectTypeChange(Connector conn)
        {
            RaisePropertyChanged(nameof(ConnType));
        }

        public DelegateCommand<MouseEventArgs> OnMouseUpCommand { get; set; }

        public void OnMouseUp(MouseEventArgs arg)
        {
            Console.WriteLine($"connector mouse up {arg}");
            Owner.SetCurrentConnector(this);
        }

        public void ChangeToNextType()
        {
            Connector.ConnectorType newValue = Connector.ConnectorType.ALWAYS;
            switch (ConnType)
            {
                case Connector.ConnectorType.FAILURE:
                    newValue = Connector.ConnectorType.SUCCESS;
                    break;
                case Connector.ConnectorType.SUCCESS:
                    newValue = Connector.ConnectorType.ALWAYS;
                    break;
                case Connector.ConnectorType.ALWAYS:
                    newValue = Connector.ConnectorType.FAILURE;
                    break;
                case Connector.ConnectorType.DELETE:
                    newValue = Connector.ConnectorType.ALWAYS;
                    break;
            }

            Owner.ChangeConnectorType_Operation(Connector, newValue);
        }


        public void OnThemeSwitch()
        {
            RaisePropertyChanged(nameof(ConnType));
        }


        public void OnKeyDown(KeyEventArgs args)
        {
            bool isCtrlDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            Console.WriteLine($"connector key down {args.Key}");
            switch (args.Key)
            {
                case Key.Tab:
                    ChangeToNextType();
                    args.Handled = true;
                    break;
                case Key.Delete:
                    Owner.RemoveConnectorOperation(this);
                    break;
                case Key.Down:
                case Key.Up:
                case Key.Left:
                case Key.Right:
                    Owner.FindNearestItem(X, Y, args.Key, isCtrlDown);
                    args.Handled = true;
                    break;
            }
           
        }
    }
}
