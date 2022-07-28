using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using FlowChart.AST;
using Prism.Mvvm;
using FlowChart.Core;
using FlowChart.Layout;
using Color = System.Drawing.Color;

namespace NFCT.Graph.ViewModels
{

    public class BaseNodeViewModel : BindableBase, INode
    {
        private Node _node;

        static BaseNodeViewModel()
        {
            BgColors = new Color[]{
                Color.White, Color.LightGray, Color.FromArgb(0xECB2AB)
                    , Color.FromArgb(0xB6D8EC), Color.FromArgb(0xC2E4C6), Color.FromArgb(0xECE1B0) };
            BgBrushes = new Brush[BgColors.Length];
            for (int i = 0; i < BgColors.Length; i++)
            {
                BgBrushes[i] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(BgColors[i].R, BgColors[i].G, BgColors[i].B));
            }
        }
        public BaseNodeViewModel(Node node, GraphPaneViewModel g)
        {
            _node = node;
            node.ParseEvent += OnParse;
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

        #region BACKGROUND COLOR

        public enum NodeBgType
        {
            NONE = 0,
            IDLE = 1,
            ERROR = 2,
            CONDITION = 3,
            ACTION = 4,
            WAIT = 5
        }

        public static Color[] BgColors;
        public static Brush[] BgBrushes;
        private Brush _bgColor;
        public Brush BgColor
        {
            get => _bgColor; set => SetProperty(ref _bgColor, value, nameof(BgColor));
        }
        #endregion

        public void OnParse(Node node, ParseResult pr)
        {
            if (pr.IsError)
                BgColor = BgBrushes[(int)NodeBgType.ERROR];
            else if(pr.IsWait)
                BgColor = BgBrushes[(int)NodeBgType.WAIT];
            else if(pr.IsAction)
                BgColor = BgBrushes[(int)NodeBgType.ACTION];
            else
            {
                BgColor = BgBrushes[(int)NodeBgType.CONDITION];
            }
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
