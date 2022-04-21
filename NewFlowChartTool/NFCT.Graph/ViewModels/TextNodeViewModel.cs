using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using FlowChart.Core;
using FlowChart.Layout;

namespace NFCT.Graph.ViewModels
{
    public class TextNodeViewModel : BindableBase, INode
    {
        private TextNode _node;
        public TextNodeViewModel(TextNode node)
        {
            _node = node;
        }
        
        public string Text { get => _node.Text; }

        private double _cx;
        public double CX { get => _cx; set => SetProperty(ref _cx, value, nameof(CX)); }

        private double _cy;
        public double CY { get => _cy; set => SetProperty(ref _cy, value, nameof(CY)); }

        private double _width;
        public double Width { get => _width; set => SetProperty(ref _width, value, nameof(Width)); }

        private double _height;
        public double Height { get => _height; set => SetProperty(ref _height, value, nameof(Height)); }
        public double X { set { CX = value; } }
        public double Y { set { CY = value; } }

        public float ActualHeight { get; set; }
        public float ActualWidth { get; set; }
    }
}
