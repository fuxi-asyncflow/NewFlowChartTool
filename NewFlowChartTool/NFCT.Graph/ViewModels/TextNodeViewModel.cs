using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using FlowChart.Core;

namespace NFCT.Graph.ViewModels
{
    public class TextNodeViewModel : BindableBase
    {
        private Node _node;
        public TextNodeViewModel(Node node)
        {
            node = _node;
        }
        public string Text { get; set; }

        private float _cx;
        public float CX { get => _cx; set => SetProperty(ref _cx, value, nameof(CX)); }

        private float _cy;
        public float CY { get => _cy; set => SetProperty(ref _cy, value, nameof(CY)); }

        private float _width;
        public float Width { get => _width; set => SetProperty(ref _width, value, nameof(Width)); }

        private float _height;
        public float Height { get => _height; set => SetProperty(ref _height, value, nameof(Height)); }
    }
}
