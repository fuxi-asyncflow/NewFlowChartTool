using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FlowChart.AST;
using Prism.Mvvm;
using FlowChart.Core;
using FlowChart.Layout;
using FlowChartCommon;
using NFCT.Common;
using Prism.Commands;
using Prism.Ioc;
using Color = System.Drawing.Color;

namespace NFCT.Graph.ViewModels
{
    public static class CanvasNodeResource
    {
        static CanvasNodeResource()
        {
            EventHelper.Sub<NFCT.Common.Events.ThemeSwitchEvent, NFCT.Common.Theme>(OnThemeSwitch);
            BackgroundColors = new Color[]
            {
                Color.White, 
                Color.LightGray, 
                Color.FromArgb(0xECB2AB), 
                Color.FromArgb(0xB6D8EC), 
                Color.FromArgb(0xC2E4C6), 
                Color.FromArgb(0xECE1B0)
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
                //BackgroundBrushes[i] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(BackgroundColors[i].R, BackgroundColors[i].G, BackgroundColors[i].B));
                BorderBrushes[i] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(BorderColors[i].R, BorderColors[i].G, BorderColors[i].B));
            }

            

            LineColors = new Color[]
            {
                Color.FromArgb(0xD5362E), Color.FromArgb(0x03950F), Color.FromArgb(0x0A6CC1), Color.FromArgb(0x808080)
            };
            LineBrushes = new Brush[LineColors.Length];
            for (int i = 0; i < LineBrushes.Length; i++)
            {
                LineBrushes[i] =
                    new SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(LineColors[i].R, LineColors[i].G, LineColors[i].B));
            }

        }
        public static double DefaultBorderWidth { get => 1.0; }
        public static double SelectedBorderWidth { get => 3.0; }

        public static Color[] BackgroundColors;
        public static Brush[] BackgroundBrushes;
        public static Color[] BorderColors;
        public static Brush[] BorderBrushes;

        public static Color[] LineColors;
        public static Brush[] LineBrushes;

        public static void OnThemeSwitch(NFCT.Common.Theme theme)
        {
            
            BackgroundBrushes[0] = Application.Current.FindResource("NodeBackGround") as SolidColorBrush;
            BackgroundBrushes[1] = Application.Current.FindResource("NodeBackGround") as SolidColorBrush;
            BackgroundBrushes[2] = Application.Current.FindResource("NodeErrorBackGround") as SolidColorBrush;
            BackgroundBrushes[3] = Application.Current.FindResource("NodeConditionBackGround") as SolidColorBrush;
            BackgroundBrushes[4] = Application.Current.FindResource("NodeActionBackGround") as SolidColorBrush;
            BackgroundBrushes[5] = Application.Current.FindResource("NodeWaitBackGround") as SolidColorBrush;
        }
    }

    public class BaseNodeViewModel : BindableBase, INode
    {
        public Node Node;
       
        public BaseNodeViewModel(Node node, GraphPaneViewModel g)
        {
            Node = node;
            node.ParseEvent += OnParse;
            Owner = g;

            OnKeyDownCommand = new DelegateCommand<KeyEventArgs>(OnKeyDown);
            BeginConnectCommand = new DelegateCommand(() => Owner.BeginConnect());
            ParentLines = new List<GraphConnectorViewModel>();
            ChildLines = new List<GraphConnectorViewModel>();
        }

        public GraphPaneViewModel Owner { get; set; }
        public GroupBoxViewModel? OwnerGroup { get; set; }
        public List<GraphConnectorViewModel> ParentLines;
        public List<GraphConnectorViewModel> ChildLines;

        private double _left;
        public double Left{ get => _left - BorderWidth ; set => SetProperty(ref _left, value, nameof(Left)); }

        private double _top;
        public double Top { get => _top - BorderWidth; set => SetProperty(ref _top, value, nameof(Top)); }

        //private double _width;
        public double Width { get => ActualWidth + (CanvasNodeResource.DefaultBorderWidth - BorderWidth) * 2.0; }

        //private double _height;
        public double Height { get => ActualHeight + (CanvasNodeResource.DefaultBorderWidth - BorderWidth) * 2.0; }
        public double X { set { Left= value; }
            get => _left + 0.5 * ActualWidth;
        }
        public double Y { set { Top = value; }
            get => _top + 0.5 * ActualHeight;
        }
        public List<INode> Children => ChildLines.ConvertAll(line => line.End);

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
            bool isCtrlDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            Logger.DBG($"{this} KeyDown : {args.Key}");
            switch (args.Key)
            {
                case Key.Space:
                    EnterEditingMode();
                    args.Handled = true;
                    break;
                case Key.Enter:
                    if (!IsEditing)
                        Owner.AddNewNodeOperation();
                    args.Handled = true;
                    break;
                case Key.Delete:
                    Owner.RemoveNodeOperation(this);
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

        #endregion

        public bool _isEditing;
        public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value, nameof(IsEditing)); }

        public virtual void EnterEditingMode()
        {
            if (IsEditing) return;
            Logger.DBG($"[{nameof(BaseNodeViewModel)}] EnterEditingMode");
            IsEditing = true;
        }

        public virtual void ExitEditingMode(NodeAutoCompleteViewModel acVm, bool save)
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
            OwnerGroup?.Resize();
        }

        #region MENU COMMAND
        public DelegateCommand BeginConnectCommand { get; set; }


        #endregion
    }

    public class TextNodeViewModel : BaseNodeViewModel
    {
        public new TextNode Node { get; set; }
        public string Text { get => Node.Text; }
        public TextNodeViewModel(TextNode node, GraphPaneViewModel g) :base(node, g)
        {
            Node = node;
        }

        public override void ExitEditingMode(NodeAutoCompleteViewModel acVm, bool save)
        {
            Logger.DBG($"[{nameof(TextNodeViewModel)}] ExitEditingMode");
            if (save)
            {
                Node.Text = acVm.Text;
                RaisePropertyChanged(nameof(Text));
                Owner.NeedLayout = true;
                Owner.Graph.Build();
            }

            IsEditing = false;
        }

        public override string ToString()
        {
            return $"[{nameof(TextNodeViewModel)}] {Text}";
        }
    }

    public class StartNodeViewModel : BaseNodeViewModel
    {
        public new StartNode Node { get; set; }
        public StartNodeViewModel(StartNode node, GraphPaneViewModel g) : base(node, g)
        {
            Node = node;
        }
    }

}
