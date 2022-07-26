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
    
    public class BaseNodeViewModel : BindableBase, INode
    {
        private Node _node;
        public BaseNodeViewModel(Node node, GraphPaneViewModel g)
        {
            _node = node;
            Owner = g;
        }

        public GraphPaneViewModel Owner { get; set; }

        private double _cx;
        public double CX { get => _cx; set => SetProperty(ref _cx, value, nameof(CX)); }

        private double _cy;
        public double CY { get => _cy; set => SetProperty(ref _cy, value, nameof(CY)); }

        //private double _width;
        public double Width { get => ActualWidth; }

        //private double _height;
        public double Height { get => ActualHeight; }
        public double X { set { CX = value; } }
        public double Y { set { CY = value; } }

        private double _actualHeight;
        public double ActualHeight { get => _actualHeight; set { if (_actualHeight == value) return; _actualHeight = value; Owner.NeedLayout = true; } }
        private double _actualWidth;
        public double ActualWidth { get => _actualWidth; set { if (_actualWidth == value) return; _actualWidth = value; Owner.NeedLayout = true; } }

        public static BaseNodeViewModel? CreateNodeViewModel(Node node, GraphPaneViewModel graphVm)
        {
            if (node is TextNode textNode)
                return new TextNodeViewModel(textNode, graphVm);
            else if(node is StartNode startNode)
                return new StartNodeViewModel(startNode, graphVm);
            return null;
        }
    }

    public class TextNodeViewModel : BaseNodeViewModel
    {
        public TextNode Node { get; set; }
        public string Text { get => Node.Text; }
        public TextNodeViewModel(TextNode node, GraphPaneViewModel g) :base(node, g)
        {
            Node = node;
        }
    }

    public class StartNodeViewModel : BaseNodeViewModel
    {
        public StartNode Node { get; set; }
        public StartNodeViewModel(StartNode node, GraphPaneViewModel g) : base(node, g)
        {
            Node = node;
        }
    }

}
