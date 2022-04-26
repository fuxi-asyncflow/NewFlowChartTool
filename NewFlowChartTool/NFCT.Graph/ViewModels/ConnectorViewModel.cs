using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using FlowChart.Layout;
using FlowChart.Core;
using System.Windows;
using System.Windows.Media;


namespace NFCT.Graph.ViewModels
{
    public class ConnectorViewModel : BindableBase, IEdge
    {
        public INode Start { get; set; }
        public INode End { get; set; }
        public ConnectorViewModel(Connector conn, TextNodeViewModel start, TextNodeViewModel end)
        {
            Start = start;
            End = end;
        }

        public PathGeometry Path { get; set; }
        public List<Curve> Curves
        {
            set => CreatePath(value);
        }

        public void CreatePath(List<Curve> curves)
        {
            PathGeometry geometry = new PathGeometry();

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
                    
                    curve.Points.ForEach(p => Console.WriteLine($"{p}"));
                    var seg = new PolyBezierSegment
                    {
                        Points = new PointCollection(curve.Points.Skip(1).ToList().ConvertAll( p => new Point(p.x, p.y)))
                    };
                    segments.Add(seg);
                    PathFigure figure = new PathFigure(new Point(s.x, s.y), segments, false);
                    geometry.Figures.Add(figure);
                }
                
            }

            
            Path = geometry;
            RaisePropertyChanged(nameof(Path));
        }
    }
}
