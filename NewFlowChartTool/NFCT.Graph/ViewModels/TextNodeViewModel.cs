using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FlowChart.AST;
using Prism.Mvvm;
using FlowChart.Core;
using FlowChart.Layout;
using FlowChart.Misc;
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
            
            BackgroundBrushes = new Brush[6];
            

           
            LineBrushes = new Brush[4];


            NodeTokenBrushes = new Brush[(int)TextToken.TokenType.End];

            OnThemeSwitch(Theme.Dark);

        }
        public static double DefaultBorderWidth { get => 0.0; }
        public static double SelectedBorderWidth { get => 3.0; }

        
        public static Brush[] BackgroundBrushes;
        public static Brush[] LineBrushes;
        public static Brush[] NodeTokenBrushes;
        public static Brush SelectedNodeBorderBrush;

        public static void OnThemeSwitch(NFCT.Common.Theme theme)
        {
            BackgroundBrushes[0] = Application.Current.FindResource("NodeBackGround") as SolidColorBrush;
            BackgroundBrushes[1] = Application.Current.FindResource("NodeBackGround") as SolidColorBrush;
            BackgroundBrushes[2] = Application.Current.FindResource("NodeErrorBackGround") as SolidColorBrush;
            BackgroundBrushes[3] = Application.Current.FindResource("NodeConditionBackGround") as SolidColorBrush;
            BackgroundBrushes[4] = Application.Current.FindResource("NodeActionBackGround") as SolidColorBrush;
            BackgroundBrushes[5] = Application.Current.FindResource("NodeWaitBackGround") as SolidColorBrush;

            LineBrushes[0] = Application.Current.FindResource("LineRedColor") as SolidColorBrush;
            LineBrushes[1] = Application.Current.FindResource("LineGreenColor") as SolidColorBrush;
            LineBrushes[2] = Application.Current.FindResource("LineBlueColor") as SolidColorBrush;
            LineBrushes[3] = Application.Current.FindResource("LineGrayColor") as SolidColorBrush;

            if (theme == Theme.Dark)
            {
                //BorderBrushes[0] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(127, 127, 127));
                //BorderBrushes[1] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(127, 127, 127));
                SelectedNodeBorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 0));
            }
            else
            {
                //BorderBrushes[0] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                //BorderBrushes[1] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                SelectedNodeBorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(127, 127, 127));
            }
            //BorderBrushes[2] = BackgroundBrushes[2];
            //BorderBrushes[3] = BackgroundBrushes[3];
            //BorderBrushes[4] = BackgroundBrushes[4];
            //BorderBrushes[5] = BackgroundBrushes[5];

            NodeTokenBrushes[0] = Application.Current.FindResource("NodeForeGround") as SolidColorBrush;
            NodeTokenBrushes[1] = Application.Current.FindResource("NodeVariableForeGround") as SolidColorBrush;
            NodeTokenBrushes[2] = Application.Current.FindResource("NodeMemberForeGround") as SolidColorBrush;
            NodeTokenBrushes[3] = Application.Current.FindResource("NodeNumberForeGround") as SolidColorBrush;
            NodeTokenBrushes[4] = Application.Current.FindResource("NodeStringForeGround") as SolidColorBrush;
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

        public virtual void OnThemeSwitch()
        {
            //TODO some graph theme take effect after close and open again
            RaisePropertyChanged(nameof(BgType));
        }

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

            if (pr.IsError)
            {
                OutputMessage.Inst?.Output(pr.ErrorMessage, OutputMessageType.Error, node, node.OwnerGraph);
            }

            OnParseEnd(pr);
        }

        public virtual void OnParseEnd(ParseResult pr)
        {
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
                    if (!IsEditing)
                    {
                        Owner.FindNearestItem(X, Y, args.Key, isCtrlDown);
                        args.Handled = true;
                    }
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

        private int _id;
        public int Id { get => _id; set => SetProperty(ref _id, value); }

        #region MENU COMMAND
        public DelegateCommand BeginConnectCommand { get; set; }


        #endregion
    }

    public class TextTokenViewModel : BindableBase
    {
        public string Text { get; set; }
        public TextToken.TokenType Type { get; set; }
        public string  TipText { get; set; }
        public Brush Color => CanvasNodeResource.NodeTokenBrushes[(int)Type];

        public void OnThemeSwitch()
        {
            RaisePropertyChanged(nameof(Color));
        }
    }

    public class TextNodeViewModel : BaseNodeViewModel
    {
        public new TextNode Node { get; set; }
        public string Text { get => Node.Text; }
        public ObservableCollection<TextTokenViewModel> Tokens { get; set; }
        public TextNodeViewModel(TextNode node, GraphPaneViewModel g) :base(node, g)
        {
            Node = node;
            Tokens = new ObservableCollection<TextTokenViewModel>();
        }

        public override void ExitEditingMode(NodeAutoCompleteViewModel acVm, bool save)
        {
            // avoid func is called twice: first called by Key.Esc, then called by LostFocus
            if (IsEditing == false)
                return;
            Logger.DBG($"[{nameof(TextNodeViewModel)}] ExitEditingMode");
            if (save)
            {
                Node.Text = acVm.Text;
                Logger.DBG($"node text is change to {Node.Text}");
                RaisePropertyChanged(nameof(Text));
                Owner.NeedLayout = true;
                Owner.Build();
            }

            IsEditing = false;
        }

        public override string ToString()
        {
            return $"[{nameof(TextNodeViewModel)}] {Text}";
        }

        public override void OnThemeSwitch()
        {
            RaisePropertyChanged(nameof(BgType));
            //TODO some graph theme take effect after close and open again
            foreach (var textTokenViewModel in Tokens)
            {
                textTokenViewModel.OnThemeSwitch();
            }
        }

        public override void OnParseEnd(ParseResult pr)
        {
            if (pr.Tokens != null)
            {
                Tokens.Clear();
                pr.Tokens.ForEach(token =>
                {
                    Tokens.Add(new TextTokenViewModel()
                    {
                        Text = Text.Substring(token.Start, token.End - token.Start),
                        Type = token.Type
                    });
                });
            }
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
