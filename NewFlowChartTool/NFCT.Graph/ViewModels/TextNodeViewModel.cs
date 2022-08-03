using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FlowChart.AST;
using Prism.Mvvm;
using FlowChart.Core;
using FlowChart.Layout;
using FlowChartCommon;
using Prism.Commands;
using Prism.Ioc;
using Color = System.Drawing.Color;

namespace NFCT.Graph.ViewModels
{
    public static class CanvasNodeResource
    {
        static CanvasNodeResource()
        {
            BackgroundColors = new Color[]
            {
                Color.White, Color.LightGray, Color.FromArgb(0xECB2AB)
                , Color.FromArgb(0xB6D8EC), Color.FromArgb(0xC2E4C6), Color.FromArgb(0xECE1B0)
            };
            BorderColors = new Color[]
            {
                Color.Black, Color.Black, Color.FromArgb(0xDD786D)
                , Color.FromArgb(0x67ACD4), Color.FromArgb(0x76C07F), Color.FromArgb(0xD5BD52)
            };
            BackgroundBrushes = new Brush[BackgroundColors.Length];
            BorderBrushes = new Brush[BorderColors.Length];
            for (int i = 0; i<BackgroundColors.Length; i++)
            {
                BackgroundBrushes[i] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(BackgroundColors[i].R, BackgroundColors[i].G, BackgroundColors[i].B));
                BorderBrushes[i] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(BorderColors[i].R, BorderColors[i].G, BorderColors[i].B));
            }
        }
        public static double DefaultBorderWidth { get => 1.0; }
        public static double SelectedBorderWidth { get => 3.0; }

        public static Color[] BackgroundColors;
        public static Brush[] BackgroundBrushes;
        public static Color[] BorderColors;
        public static Brush[] BorderBrushes;
    }

    public class BaseNodeViewModel : BindableBase, INode
    {
        private Node _node;
       
        public BaseNodeViewModel(Node node, GraphPaneViewModel g)
        {
            _node = node;
            node.ParseEvent += OnParse;
            Owner = g;

            OnKeyDownCommand = new DelegateCommand<KeyEventArgs>(OnKeyDown);
            ParentLines = new List<ConnectorViewModel>();
            ChildLines = new List<ConnectorViewModel>();
        }

        public GraphPaneViewModel Owner { get; set; }
        public List<ConnectorViewModel> ParentLines;
        public List<ConnectorViewModel> ChildLines;

        private double _left;
        public double Left{ get => _left - BorderWidth ; set => SetProperty(ref _left, value, nameof(Left)); }

        private double _top;
        public double Top { get => _top - BorderWidth; set => SetProperty(ref _top, value, nameof(Top)); }

        //private double _width;
        public double Width { get => ActualWidth + (CanvasNodeResource.DefaultBorderWidth - BorderWidth) * 2.0; }

        //private double _height;
        public double Height { get => ActualHeight + (CanvasNodeResource.DefaultBorderWidth - BorderWidth) * 2.0; }
        public double X { set { Left= value; } }
        public double Y { set { Top = value; } }

        public double ActualHeight { get; set; }
        public double ActualWidth { get; set; }

        public double OriginalX;
        public double OriginalY;

        public void SaveOriginalPos()
        {
            OriginalX = _left;
            OriginalY = _top;
        }

        public static BaseNodeViewModel? CreateNodeViewModel(Node node, GraphPaneViewModel graphVm)
        {
            if (node is TextNode textNode)
                return new TextNodeViewModel(textNode, graphVm);
            else if(node is StartNode startNode)
                return new StartNodeViewModel(startNode, graphVm);
            return null;
        }

        private bool _isSelect;
        public bool IsSelect
        {
            get => _isSelect;
            set
            {
                SetProperty(ref _isSelect, value, nameof(IsSelect));
                IsFocused = _isSelect;
                RaisePropertyChanged(nameof(Left));
                RaisePropertyChanged(nameof(Top));
            }
        }

        private bool _isFocused;
        public bool IsFocused { get => _isFocused; set => SetProperty(ref _isFocused, value, nameof(IsFocused)); }

        double BorderWidth
        {
            get
            {
                return _isSelect ? CanvasNodeResource.SelectedBorderWidth : CanvasNodeResource.DefaultBorderWidth;
            }
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

        #region COMMANDS
        public DelegateCommand<KeyEventArgs> OnKeyDownCommand { get; set; }

        public void OnKeyDown(KeyEventArgs args)
        {
            Logger.DBG($"[{nameof(BaseNodeViewModel)}] KeyDown : {args.Key}");
            if (args.Key == Key.Space)
            {
                Console.WriteLine("textnode keydown");
                EnterEditingMode();
            }
        }

        #endregion

        public bool _isEditing;
        public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value, nameof(IsEditing)); }

        public virtual void EnterEditingMode()
        {
            if (IsEditing) return;
            Logger.DBG($"[{nameof(BaseNodeViewModel)}] EnterEditingMode");
            IsEditing = true;
        }

        public virtual void ExitEditingMode(NodeAutoCompleteViewModel acVm)
        {

        }

        public void Move(double dx, double dy)
        {
            _left = OriginalX + dx;
            _top = OriginalY + dy;
            RaisePropertyChanged(nameof(Left));
            RaisePropertyChanged(nameof(Top));

            double mid = _left + ActualWidth * 0.5;
            double bottom = _top + ActualHeight;
            
            ParentLines.ForEach(line =>
            {
                var parent = (BaseNodeViewModel)line.Start;
                line.StaightLineConnect(new Point(parent.Left + parent.ActualWidth * 0.5, parent.Top + ActualHeight), new Point(mid, _top));
            });
            ChildLines.ForEach(line =>
            {
                var child = (BaseNodeViewModel)line.End;
                line.StaightLineConnect(new Point(mid, bottom), new Point(child.Left + child.ActualWidth * 0.5, child.Top));
            });
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

        public override void ExitEditingMode(NodeAutoCompleteViewModel acVm)
        {
            Logger.DBG($"[{nameof(TextNodeViewModel)}] ExitEditingMode");
            Node.Text = acVm.Text;
            RaisePropertyChanged(nameof(Text));
            Owner.NeedLayout = true;
            IsEditing = false;
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
