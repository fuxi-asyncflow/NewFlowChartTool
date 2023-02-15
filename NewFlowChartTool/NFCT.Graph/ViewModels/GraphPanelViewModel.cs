using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FlowChart.AST;
using Prism.Mvvm;
using FlowChart.Core;
using FlowChart.Layout;
using FlowChart.Common;
using NFCT.Common;
using NFCT.Common.Events;
using NFCT.Common.ViewModels;
using Prism.Commands;

namespace NFCT.Graph.ViewModels
{
    public class GraphLayoutAdapter : IGraph
    {
        GraphPaneViewModel _g;
        public GraphLayoutAdapter(GraphPaneViewModel g) { _g = g; }

        public IEnumerable<INode> Nodes => _g.Nodes;

        public IEnumerable<IEdge> Edges => _g.Connectors;

        public IEnumerable<IGroup> Groups => _g.Groups;

        public double Width { set => _g.Width = value; }
        public double Height { set => _g.Height = value; }
    }

    public class SubGraphTypeViewModel
    {
        public SubGraphTypeViewModel(FlowChart.Core.Graph.SubGraphTypeEnum type, string name, string description)
        {
            SubGraphType = type;
            Name = name;
            Description = description;
        }
        public FlowChart.Core.Graph.SubGraphTypeEnum SubGraphType;
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public partial class GraphPaneViewModel : BindableBase
    {

        private FlowChart.Core.Graph _graph;

        public FlowChart.Core.Graph Graph => _graph;

        static GraphPaneViewModel()
        {
            GraphClipboard = new List<BaseNodeViewModel>();
            SubGraphTypes = new ObservableCollection<SubGraphTypeViewModel>();
            SubGraphTypes.Add(new SubGraphTypeViewModel(FlowChart.Core.Graph.SubGraphTypeEnum.NONE, "    ", "不可在其它流程图中调用"));
            SubGraphTypes.Add(new SubGraphTypeViewModel(FlowChart.Core.Graph.SubGraphTypeEnum.LOCAL, "局部子图", "可在同目录下的其它流程图中使用"));
            SubGraphTypes.Add(new SubGraphTypeViewModel(FlowChart.Core.Graph.SubGraphTypeEnum.GLOBAL, "全局子图", "可在任意流程图中调用，谨慎使用"));
        }

        public  static ObservableCollection<SubGraphTypeViewModel> SubGraphTypes { get; set; }

        public GraphPaneViewModel(FlowChart.Core.Graph graph)
        {
            _graph = graph;
            Nodes = new ObservableCollection<BaseNodeViewModel>();
            NodeDict = new Dictionary<Node, BaseNodeViewModel>();
            Connectors = new ObservableCollection<GraphConnectorViewModel>();
            ConnectorDict = new Dictionary<Connector, GraphConnectorViewModel>();
            Groups = new ObservableCollection<GroupBoxViewModel>();
            SelectedNodes = new List<BaseNodeViewModel>();
            SelectedConnectors = new List<GraphConnectorViewModel>();
            VariablesPanel = new GraphVariablesPanelViewModel(this);
            UndoRedoManager = new UndoRedoManagerViewModel();

            ChangeLayoutCommand = new DelegateCommand(ChangeAutoLayout);
            CreateGroupCommand = new DelegateCommand(CreateGroupFromSelectedNodes);
            OnCloseCommand = new DelegateCommand(OnClose);
            OnPreviewKeyDownCommand = new DelegateCommand<KeyEventArgs>(OnPreviewKeyDown);

            EventHelper.Sub<ThemeSwitchEvent, Theme>(OnThemeSwitch);

            GraphPaneViewModel_Layout_Init();
            GraphPanelViewModel_Debug_Init();

            Initialize();
        }

        public void Initialize()
        {
            Nodes.Clear();
            NodeDict.Clear();
            _graph.Nodes.ForEach(node => _addNodeViewModel(node));
            _graph.Connectors.ForEach(conn => _createConnectorViewModel(conn));
            _graph.Groups.ForEach(group => _addGroupViewModel(group));
            IsFirstLayout = true;
            NeedLayout = true;

            foreach (var nodeVm in Nodes)
            {
                if (nodeVm is ControlNodeViewModel controlVm)
                    controlVm.ParseText(null);
            }

            TypeNames = _graph.Project.GetCustomTypeNames();

            _graph.GraphPathChangeEvent += OnGraphPathChange;
            _graph.GraphRemoveNodeEvent += OnRemoveNode;
            _graph.ConnectorRemoveEvent += OnRemoveConnector;
            _graph.GraphConnectEvent += OnConnect;
            _graph.GraphAddNodeEvent += OnAddNode;
            _graph.ConnectorTypeChangeEvent += OnConnectorTypeChange;
            _graph.GraphSwitchChildNodeOrderEvent += OnSwitchChildNodeOrder;
            IsDirty = false;
        }

        public string Name => _graph.Name;
        public string FullPath => _graph.Path;

        public string TypeName
        {
            get => _graph.Type.Name;
            set
            {
                if (value == _graph.Type.Name)
                    return;
                var tp = _graph.Project.GetType(value);
                if (tp == null)
                    return;
                _graph.Type = tp;
                RaisePropertyChanged(nameof(TypeName));
            }
        }

        public List<string> TypeNames { get; set; }

        public string? Description
        {
            get => _graph.Description;
            set
            {
                if (_graph.Description == value)
                    return;
                _graph.Description = string.IsNullOrEmpty(value) ? null : value;
            }
        }

        public string TabHeaderName => IsDirty ? _graph.Name + "*" : _graph.Name;
        private bool _isDirty;
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                SetProperty(ref _isDirty, value); 
                RaisePropertyChanged(nameof(TabHeaderName));
            }
        }

        public int SubGraphType
        {
            get => (int)_graph.SubGraphType;
            set
            {
                //TODO check if can set as SubGraph
                if (value < 0 || value > 2)
                    return;
                var newValue = (FlowChart.Core.Graph.SubGraphTypeEnum)value;
                if (newValue == FlowChart.Core.Graph.SubGraphTypeEnum.GLOBAL)
                {
                    if (_graph.Type.FindMember(Name) != null)
                    {
                        return;
                    }
                }
                _graph.SetSubGraph(newValue);
                RaisePropertyChanged(nameof(SubGraphType));
            }
        }

        public ObservableCollection<BaseNodeViewModel> Nodes { get; set; }
        public Dictionary<Node, BaseNodeViewModel> NodeDict { get; set; }
        public ObservableCollection<GraphConnectorViewModel> Connectors { get; set; }
        public Dictionary<Connector, GraphConnectorViewModel> ConnectorDict { get; set; }
        public ObservableCollection<GroupBoxViewModel> Groups { get; set; }
        public GraphVariablesPanelViewModel VariablesPanel { get; set; }
        public UndoRedoManagerViewModel UndoRedoManager { get; set; }

        public BaseNodeViewModel GetNodeVm(Node node)
        {
            Debug.Assert(node.OwnerGraph == _graph);
            Debug.Assert(NodeDict.ContainsKey(node));
            return NodeDict[node];
        }

        public GraphConnectorViewModel GetConnVm(Connector conn)
        {
            Debug.Assert(conn.OwnerGraph == _graph);
            Debug.Assert(ConnectorDict.ContainsKey(conn));
            return ConnectorDict[conn];
        }
        
        

        public DelegateCommand ChangeLayoutCommand { get; set; }
        public DelegateCommand CreateGroupCommand { get; set; }
        public DelegateCommand OnCloseCommand { get; set; }

        public DelegateCommand SubgraphCommand { get; set; }

        
        public void Build()
        {
            Graph.Build(new ParserConfig(){GetTokens = true});
        }

        #region SCROLL

        private double _scrollX;
        private double _scrollY;

        public double ScrollX
        {
            get => _scrollX;
            set => SetProperty(ref _scrollX, value);
        }

        public double ScrollY
        {
            get => _scrollY;
            set => SetProperty(ref _scrollY, value);
        }

        private double _scrollViewerWidth;
        private double _scrollViewerHeight;

        public double ScrollViewerWidth
        {
            get => _scrollViewerWidth;
            set => SetProperty(ref _scrollViewerWidth, value);
        }

        public double ScrollViewerHeight
        {
            get => _scrollViewerHeight;
            set => SetProperty(ref _scrollViewerHeight, value);
        }

        public void MoveNodeToCenter(BaseNodeViewModel? nodeVm)
        {
            if (nodeVm == null)
                return;
            // Logger.DBG($"MoveNodeToCenter {nodeVm.Left} {ScrollViewerWidth} {nodeVm}");
            ScrollX = nodeVm.Left + (nodeVm.Width - ScrollViewerWidth) / 2;
            ScrollY = nodeVm.Top - ScrollViewerHeight / 2;

        }
        #endregion

        #region FUNCTION

        // private BaseNodeViewModel? SelectedNode { get; set; }
        public List<BaseNodeViewModel> SelectedNodes { get; set; }
        public List<GraphConnectorViewModel> SelectedConnectors { get; set; }
        public BaseNodeViewModel? CurrentNode { get; set; } // node will has keyboard focus
        public GraphConnectorViewModel? CurrentConnector { get; set; }
        public void SelectNode(BaseNodeViewModel nodeVm, bool clearOthers = true)
        {
            if (nodeVm.IsSelect)
            {
                return;
            }

            if (clearOthers)
            {
                ClearSelectedItems(nodeVm);
            }

            nodeVm.IsSelect = true;
            SelectedNodes.Add(nodeVm);
        }

        //public void SelectConnector(GraphConnectorViewModel connectorVm, bool clearOthers = true)
        //{
        //    if (connectorVm.IsSelect)
        //        return;
        //    if (clearOthers)
        //    {
        //        ClearSelectedItems("all");
        //    }
        //    connectorVm.IsSelect = true;
        //    SelectedConnectors.Add(connectorVm);
        //}

        public void ClearSelectedItems(object? exclude = null)
        {

            SelectedNodes.ForEach(node =>
            {
                if(node != exclude)
                    node.IsSelect = false;
            });
            SelectedNodes.RemoveAll(node => node != exclude);

            SelectedConnectors.ForEach(conn =>
            {
                if(conn != exclude)
                    conn.IsSelect = false;
            });
            SelectedConnectors.RemoveAll(conn => conn != exclude);
        }

        public void MoveSelectedItems(double dx, double dy)
        {
            SelectedNodes.ForEach(node => node.Move(dx, dy));
        }

        public void ClearCurrentItem()
        {
            if (CurrentNode != null)
            {
                CurrentNode.IsFocused = false;
                CurrentNode.IsSelect = false;
                CurrentNode = null;
            }

            if (CurrentConnector != null)
            {
                CurrentConnector.IsFocused = false;
                CurrentConnector.IsSelect = false;
                CurrentConnector = null;
            }
        }

        public void SetCurrentNode(BaseNodeViewModel? nodeVm, bool clearOthers = true)
        {
            if (CurrentNode == nodeVm)
                return;

            // clear focus on CurrentNode
            var currentNode = CurrentNode;
            ClearCurrentItem();
            if(!clearOthers && currentNode != null)
                SelectNode(currentNode, false);

            CurrentNode = nodeVm;
            if (CurrentNode != null)
            {
                SelectNode(CurrentNode, clearOthers);
                CurrentNode.IsFocused = true;
            }

            Console.WriteLine("set current node");
        }

        public void SetCurrentConnector(GraphConnectorViewModel? connVm)
        {
            if (CurrentConnector == connVm)
                return;

            ClearCurrentItem();

            CurrentConnector = connVm;
            if (CurrentConnector != null)
            {
                CurrentConnector.IsFocused = true;
                CurrentConnector.IsSelect = true;
            }
            Console.WriteLine("set current connector");
        }

        public void SelectNodeInBox(Rect box)
        {
            if (box.Width < 10 && box.Height < 10) return;
            //TODO Ctrl
            ClearSelectedItems();
            ClearCurrentItem();

            double l = box.Left;
            double r = box.Right;
            double t = box.Top;
            double b = box.Bottom;
            

            foreach (var nodeVm in Nodes)
            {
                //if (nodeVm.Cx > l && nodeVm.Cx < r && nodeVm.Cy > t && nodeVm.Cy < b)
                //{
                //    selNodeVm.Add(nodeVm);
                //}
                double nl = nodeVm.Left;
                double nt = nodeVm.Top;
                double nr = nl + nodeVm.Width;
                double nb = nt + nodeVm.Height;

                double ll = nl > l ? nl : l;
                double tt = nt > t ? nt : t;
                double rr = nr < r ? nr : r;
                double bb = nb < b ? nb : b;

                if (ll < rr && tt < bb)
                {
                    if (CurrentNode == null)
                        SetCurrentNode(nodeVm);
                    SelectNode(nodeVm, false);
                }
            }
        }

        void OnClose()
        {
            if (IsDirty)
            {
                var result = MessageBox.Show($"graph {FullPath} has unsaved changes.\nclose it will discard them", "Warning", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                    return;
            }
            EventHelper.Pub<GraphCloseEvent, FlowChart.Core.Graph>(Graph);
        }
        #endregion

        #region CallBack

        void OnGraphPathChange(FlowChart.Core.Graph graph, string oldPath, string newPath)
        {
            RaisePropertyChanged(nameof(FullPath));
            RaisePropertyChanged(nameof(Name));
        }

        void OnThemeSwitch(Theme theme)
        {
            Logger.DBG($"graph theme switch fo {FullPath}");
            foreach (var nodeVm in Nodes)
            {
                nodeVm.OnThemeSwitch();
            }

            foreach (var connector in Connectors)
            {
                connector.OnThemeSwitch();
            }
        }

        #endregion

        public void OnPreviewKeyDown(KeyEventArgs arg)
        {
            if (CurrentConnector != null)
            {
                CurrentConnector.OnKeyDown(arg);
                arg.Handled = true;
            }
        }
        public DelegateCommand<KeyEventArgs> OnPreviewKeyDownCommand { get; set; }

        public BaseNodeViewModel? FindNearestItem(double cx, double cy, Key direction, bool targetIsLine)
        {
            Console.WriteLine($"findnearestitem {targetIsLine}");
            double angle = 0.0;
            switch (direction)
            {
                case Key.Left: angle = 180.0; break;
                case Key.Right: angle = 0.0f; break;
                case Key.Up: angle = 90.0; break;
                case Key.Down: angle = 270.0f; break;
            }
            angle = angle / 180.0 * Math.PI;    // 角度转弧度
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);

            // 计算权重的函数
            const double k = 16;
            Func<double, double, double> weightFunc = (x, y) =>
            {
                double dx = x - cx;
                double dy = -(y - cy); // 因为坐标系与数学上的坐标系Y轴是相反的
                double dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist < 10.0) return 0.0; // 自身的权值设为0

                double cosTheta = (dx * cosAngle + dy * sinAngle) / dist;
                if (cosTheta < 0) return 0.0;
                return (cosTheta + 1 / (k - 1)) / dist;
            };

            double maxWeight = 0.0;
            if (targetIsLine)
            {
                GraphConnectorViewModel maxWeightLine = null;
                // 获得权值最大的节点
                foreach (var lineVm in Connectors)
                {
                    double w = weightFunc(lineVm.X, lineVm.Y);
                    if (w > maxWeight)
                    {
                        maxWeight = w;
                        maxWeightLine = lineVm;
                    }
                }
                // 将最大节点设为当前节点
                if (maxWeightLine != null)
                {
                    SetCurrentConnector(maxWeightLine);
                }
            }
            else
            {
                BaseNodeViewModel maxWeightNode = null;
                // 获得权值最大的节点
                foreach (var nodeVm in Nodes)
                {
                    double w = weightFunc(nodeVm.X, nodeVm.Y);
                    if (w > maxWeight)
                    {
                        maxWeight = w;
                        maxWeightNode = nodeVm;
                    }
                }
                // 将最大节点设为当前节点
                if (maxWeightNode != null)
                {
                    SetCurrentNode(maxWeightNode);
                }
            }

            return null;
        }

        public BaseNodeViewModel? ConnectStartNode { get; set; }
        
        public bool IsConnecting { get; set; }
        public void BeginConnect()
        {
            if (CurrentNode == null)
            {
                Logger.WARN("currentnode is null when begin connect");
                return;
            }

            Console.WriteLine($"begin connect {CurrentNode.X} {CurrentNode.Y}");
            ConnectStartNode = CurrentNode;
            IsConnecting = true;
            RaisePropertyChanged(nameof(IsConnecting));
            RaisePropertyChanged(nameof(ConnectStartNode));
        }

        public void EndConnect(bool cancel = false)
        {
            if (!IsConnecting)
                return;
            IsConnecting = false;
            RaisePropertyChanged(nameof(IsConnecting));

            if (ConnectStartNode == null || CurrentNode == null)
                return;
            if(!cancel)
                ConnectOperation(ConnectStartNode.Node, CurrentNode.Node);
            ConnectStartNode = null;
            NeedLayout = true;
        }

        public static List<BaseNodeViewModel> GraphClipboard;
        public static bool IsClip;

        public void CopySelectedNodes(bool isClip = false)
        {
            if (SelectedNodes.Count == 0)
                return;
            GraphClipboard.Clear();
            GraphClipboard.AddRange(SelectedNodes);
            SelectedNodes.ForEach(nodeVm => nodeVm.IsCut = isClip);
            IsClip = isClip;
        }

        public void PasteNodes(BaseNodeViewModel parentVm)
        {
            if (GraphClipboard.Count == 0)
                return;
            var originGraphVm = GraphClipboard[0].Owner;
            GraphClipboard.ForEach(nodeVm => nodeVm.IsCut = false);
            bool needClip = IsClip && originGraphVm == this;
            PasteNodesOperation(parentVm.Node, GraphClipboard.ConvertAll(nodeVm => nodeVm.Node), needClip);
            Build();
            if (IsClip)
            {
                IsClip = false;
                if (originGraphVm != this)
                    originGraphVm.RemoveNodesOperation(GraphClipboard);
            }
        }

        public void ReplaceNodeViewModel(Node node, BaseNodeViewModel newVm)
        {
            var oldVm = GetNodeVm(node);
            oldVm.RemoveEventCallback();
            newVm.ActualHeight = oldVm.ActualHeight;
            newVm.ActualWidth = oldVm.ActualWidth;
            newVm.BgType = oldVm.BgType;
            var idx = Nodes.IndexOf(oldVm);
            Debug.Assert(idx >= 0);
            oldVm.ParentLines.ForEach(conn => conn.EndNode = newVm);
            oldVm.ChildLines.ForEach(conn => conn.StartNode = newVm);

            Nodes[idx] = newVm;
            NodeDict[node] = newVm;
        }

        #region View Methods

        public void ScreenShot()
        {
            ScreenShotEvent?.Invoke();
        }

        public event Action ScreenShotEvent;

        #endregion
    }
}
