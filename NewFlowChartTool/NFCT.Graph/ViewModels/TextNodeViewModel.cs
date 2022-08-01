﻿using System;
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
    public static class CanvasNodeResource
    {
        static CanvasNodeResource()
        {
            BackgroundColors = new Color[]{
                Color.White, Color.LightGray, Color.FromArgb(0xECB2AB)
                , Color.FromArgb(0xB6D8EC), Color.FromArgb(0xC2E4C6), Color.FromArgb(0xECE1B0) };
            BackgroundBrushes = new Brush[BackgroundColors.Length];
            for (int i = 0; i<BackgroundColors.Length; i++)
            {
                BackgroundBrushes[i] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(BackgroundColors[i].R, BackgroundColors[i].G, BackgroundColors[i].B));
            }
        }
        public static double DefaultBorderWidth { get => 2.0; }
        public static double SelectedBorderWidth { get => 6.0; }

        public static Color[] BackgroundColors;
        public static Brush[] BackgroundBrushes;
    }

    public class BaseNodeViewModel : BindableBase, INode
    {
        private Node _node;
       
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

        private bool _isSelect;
        public bool IsSelect { get => _isSelect; set => SetProperty(ref _isSelect, value, nameof(IsSelect)); }

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

        private NodeBgType _bgType;
        public NodeBgType BgType { get => _bgType; set => SetProperty(ref _bgType, value, nameof(BgType)); }

        #endregion

        public void OnParse(Node node, ParseResult pr)
        {
            if (pr.IsError)
                BgType = NodeBgType.ERROR;
            else if (pr.IsWait)
                BgType = NodeBgType.WAIT;
            else if (pr.IsAction)
                BgType = NodeBgType.ACTION;
            else
                BgType = NodeBgType.CONDITION;
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
