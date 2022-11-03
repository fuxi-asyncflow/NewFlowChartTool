using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FlowChart.AST;
using FlowChart.Core;
using FlowChart.Layout;
using FlowChartCommon;
using NFCT.Common;
using NFCT.Common.Services;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;

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
            CopyNodesCommand = new DelegateCommand(() => Owner.CopySelectedNodes());
            CutNodesCommand = new DelegateCommand(() => Owner.CopySelectedNodes(true));
            PasteNodesCommand = new DelegateCommand(() => Owner.PasteNodes(this));
            BreakPointCommand = new DelegateCommand(delegate
            {
                IsBreakPoint = !IsBreakPoint; 
                ContainerLocator.Current.Resolve<IDebugService>().SetBreakPoint(Node, IsBreakPoint);
            });
            ContinueBreakPointCommand = new DelegateCommand(delegate { Owner.ContinueBreakPoint(); });
        }
        public string? Description => Node.Description;
        private string? _errorMessage;
        public string? ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
        public GraphPaneViewModel Owner { get; set; }
        public GroupBoxViewModel? OwnerGroup { get; set; }
        public List<GraphConnectorViewModel> ParentLines => Node.Parents.ConvertAll(Owner.GetConnVm);
        public List<GraphConnectorViewModel> ChildLines => Node.Children.ConvertAll(Owner.GetConnVm);

        private double _left;
        public double Left { get => _left - BorderWidth; set => SetProperty(ref _left, value, nameof(Left)); }

        private double _top;
        public double Top { get => _top - BorderWidth; set => SetProperty(ref _top, value, nameof(Top)); }

        //private double _width;
        public double Width { get => ActualWidth + (CanvasNodeResource.DefaultBorderWidth - BorderWidth) * 2.0; }

        //private double _height;
        public double Height { get => ActualHeight + (CanvasNodeResource.DefaultBorderWidth - BorderWidth) * 2.0; }
        public double X
        {
            set { Left = value; }
            get => _left + 0.5 * ActualWidth;
        }
        public double Y
        {
            set { Top = value; }
            get => _top + 0.5 * ActualHeight;
        }
        public List<INode> Children => ChildLines.ConvertAll(line => line.End);

        private double _actualHeight;
        public double ActualHeight { get => _actualHeight; set => SetProperty(ref _actualHeight, value); }
        private double _actualWidth;
        public double ActualWidth { get => _actualWidth; set => SetProperty(ref _actualWidth, value); }

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
            {
                if (ControlNodeViewModel.MaybeControlNodeViewModel(textNode.Text))
                {
                    var controlNodeVm = new ControlNodeViewModel(node, graphVm);
                    return controlNodeVm;
                }
                return new TextNodeViewModel(textNode, graphVm);
            }
            else if (node is StartNode startNode)
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

        #region DEBUG STATUS
        public bool IsDebugMode => Owner.IsDebugMode;
        public void ChangeDebugMode() { RaisePropertyChanged(nameof(IsDebugMode)); }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }

        public void ExitDebugMode()
        {
            SuccessCount = 0;
            FailureCount = 0;
            RaisePropertyChanged(nameof(SuccessCount));
            RaisePropertyChanged(nameof(FailureCount));
            StopDebugEvent?.Invoke();
            //RaisePropertyChanged(nameof(BgType));
        }

        public void ChangeDebugStatus(DebugNodeStatus status)
        {
            switch (status)
            {
                case DebugNodeStatus.IDLE:
                    break;
                case DebugNodeStatus.RUNNING:
                    break;
                case DebugNodeStatus.SUCCESS:
                    SuccessCount++;
                    RaisePropertyChanged(nameof(SuccessCount));
                    break;
                case DebugNodeStatus.FAILURE:
                    FailureCount++;
                    RaisePropertyChanged(nameof(FailureCount));
                    break;
                case DebugNodeStatus.ERROR:
                    break;
            }
            DebugStatusChangeEvent?.Invoke(status);
        }

        private bool _isBreakPoint;
        public bool IsBreakPoint { get => _isBreakPoint; set => SetProperty(ref _isBreakPoint, value); }

        public delegate void NodeDebugStatusChange(DebugNodeStatus status);
        public event NodeDebugStatusChange? DebugStatusChangeEvent;

        public event Action? StopDebugEvent;
        #endregion

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

        private bool _isCut = false;
        public bool IsCut { get => _isCut; set => SetProperty(ref _isCut, value); }

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
                    Owner.RemoveNodesOperation(Owner.SelectedNodes);
                    args.Handled = true;
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
                //------------------------------------------------------
                case Key.C:
                    if(isCtrlDown)
                        CopyNodesCommand.Execute();
                    break;
                case Key.X:
                    if (isCtrlDown)
                        CutNodesCommand.Execute();
                    break;
                case Key.V:
                    if (isCtrlDown)
                        PasteNodesCommand.Execute();
                    break;
            }

        }

        #endregion

        public bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (value == _isEditing) return;
                SetProperty(ref _isEditing, value, nameof(IsEditing));
                EditingModeChangeEvent?.Invoke(value);
            }
        }

        public virtual void EnterEditingMode()
        {
            if (IsEditing) return;
            Logger.DBG($"[{nameof(BaseNodeViewModel)}] EnterEditingMode");
            IsEditing = true;
        }

        public virtual void ExitEditingMode(NodeAutoCompleteViewModel acVm, bool save)
        {

        }

        public event Action<bool> EditingModeChangeEvent;

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
        public DelegateCommand CopyNodesCommand { get; set; }
        public DelegateCommand PasteNodesCommand { get; set; }
        public DelegateCommand CutNodesCommand { get; set; }

        public DelegateCommand BreakPointCommand { get; set; }
        public DelegateCommand ContinueBreakPointCommand { get; set; }

        #endregion
    }
}
