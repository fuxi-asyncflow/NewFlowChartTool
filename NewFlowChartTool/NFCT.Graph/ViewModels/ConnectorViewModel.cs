using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using FlowChart.Layout;
using FlowChart.Core;
using System.Windows;


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

        private List<Point> _points;
        public List<Point> Points { get => _points; set => SetProperty(ref _points, value, nameof(Points)); }
        public List<Position> PathPoints { set =>  Points = value.ConvertAll(p => new Point(p.x, p.y));  }
    }
}
