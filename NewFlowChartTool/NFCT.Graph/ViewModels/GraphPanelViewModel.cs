using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Prism.Mvvm;
using FlowChart.Core;
using FlowChart.Layout;
using FlowChartCommon;
using NLog.Fluent;
using Prism.Commands;

namespace NFCT.Graph.ViewModels
{
    public class GraphLayoutAdapter : IGraph
    {
        GraphPaneViewModel _g;
        public GraphLayoutAdapter(GraphPaneViewModel g) { _g = g; }
      
        public IEnumerable<INode> Nodes => _g.Nodes;

        public IEnumerable<IEdge> Edges => _g.Connectors;

        public double Width { set => _g.Width = value; }
        public double Height { set => _g.Height = value; }
    }

    public class GraphPaneViewModel : BindableBase
    {

        private FlowChart.Core.Graph _graph;

        public FlowChart.Core.Graph Graph => _graph;

        public GraphPaneViewModel(FlowChart.Core.Graph graph)
        {
            _graph = graph;
            Nodes = new ObservableCollection<BaseNodeViewModel>();
            NodeDict = new Dictionary<Node, BaseNodeViewModel>();
            Connectors = new ObservableCollection<GraphConnectorViewModel>();
            Groups = new ObservableCollection<GroupBoxViewModel>();
            SelectedNodes = new List<BaseNodeViewModel>();
            SelectedConnectors = new List<GraphConnectorViewModel>();
            VariablesPanel = new GraphVariablesPanelViewModel(this);
            ChangeLayoutCommand = new DelegateCommand(ChangeAutoLayout);
            CreateGroupCommand = new DelegateCommand(CreateGroupFromSelectedNodes);
            OnPreviewKeyDownCommand = new DelegateCommand<KeyEventArgs>(OnPreviewKeyDown);
            Initialize();
        }

        public BaseNodeViewModel? AddNode(Node node)
        {
            var vm = BaseNodeViewModel.CreateNodeViewModel(node, this);
            if (vm == null)
                return null;
            Nodes.Add(vm);
            NodeDict.Add(node, vm);
            return vm;
        }

        // used when GraphViewModel create
        public void Connect(Connector conn)
        {
            var start = conn.Start;
            var end = conn.End;
            BaseNodeViewModel? startVm, endVm;
            if (NodeDict.TryGetValue(start, out startVm) && NodeDict.TryGetValue(end, out endVm))
            {
                Connectors.Add(new GraphConnectorViewModel(conn, startVm, endVm, this));
            }
        }

        public void Connect(Node start, Node end)
        {
            BaseNodeViewModel? startVm, endVm;
            if (!NodeDict.TryGetValue(start, out startVm) || !NodeDict.TryGetValue(end, out endVm))
            {
                Logger.WARN("connect nodes error: node has no viewmodel");
                return;
            }

            if (startVm.ChildLines.Any(line => line.End == endVm))
            {
                Logger.WARN("connect nodes error: line exist");
                return;
            }
            
            var conn = Graph.Connect(start, end, Connector.ConnectorType.SUCCESS);
            if (conn == null)
                return;
            Connectors.Add(new GraphConnectorViewModel(conn, startVm, endVm, this));
        }

        public void Initialize()
        {
            Nodes.Clear();
            NodeDict.Clear();
            _graph.Nodes.ForEach(node => AddNode(node));
            _graph.Connectors.ForEach(Connect);
            IsFirstLayout = true;
            NeedLayout = true;

            _graph.GraphPathChangeEvent += OnGraphPathChange;
        }

        public string Name => _graph.Name;
        public string FullPath => _graph.Path;
        

        public string Description => _graph.Description;

        public ObservableCollection<BaseNodeViewModel> Nodes { get; set; }
        public Dictionary<Node, BaseNodeViewModel> NodeDict { get; set; }
        public ObservableCollection<GraphConnectorViewModel> Connectors { get; set; }
        public ObservableCollection<GroupBoxViewModel> Groups { get; set; }
        public GraphVariablesPanelViewModel VariablesPanel { get; set; }
        
        public bool NeedLayout { get; set; }
        public bool IsFirstLayout { get; set; }
        private double _width;
        public double Width { get => _width; set => SetProperty(ref _width, value); }

        public double _height;
        public double Height { get => _height; set => SetProperty(ref _height, value); }

        public bool AutoLayout
        {
            get => _graph.AutoLayout;
        }

        public DelegateCommand ChangeLayoutCommand { get; set; }
        public DelegateCommand CreateGroupCommand { get; set; }

        public void ChangeAutoLayout()
        {
            if (AutoLayout)
            {
                _graph.AutoLayout = false;
                RaisePropertyChanged(nameof(AutoLayout));
            }
            else
            {
                var result = MessageBox.Show("Switch to AutoLayout, all nodes position will change", "Be careful",
                    MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                    return;
                _graph.AutoLayout = true;
                NeedLayout = true;
                RaisePropertyChanged(nameof(AutoLayout));
            }
        }

        public bool Relayout()
        {
            Logger.DBG($"Relayout for graph {Name}");
            var graph = new GraphLayoutAdapter(this);
            if (NodeDict.Count <= 0.0 || NodeDict.First().Value.Width <= 0.0)
                return false;
            MsaglLayout layout = new MsaglLayout();
            try
            {
                layout.Layout(graph);
                if (Height < 0.001 || Width < 0.001)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[error] layout failed {e.Message}");
                return false;
            }

            return true;
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
                ClearSelectedItems("all");
                SetCurrentNode(nodeVm);
            }

            nodeVm.IsSelect = true;
            SelectedNodes.Add(nodeVm);
        }

        public void SelectConnector(GraphConnectorViewModel connectorVm, bool clearOthers = true)
        {
            if (connectorVm.IsSelect)
                return;
            if (clearOthers)
            {
                ClearSelectedItems("all");
            }
            connectorVm.IsSelect = true;
            SelectedConnectors.Add(connectorVm);
        }

        public void ClearSelectedItems(string items)
        {
            if (items == "all")
            {
                foreach (var node in SelectedNodes)
                {
                    node.IsSelect = false;
                }
                SelectedNodes.Clear();

                SelectedConnectors.ForEach(conn => conn.IsSelect = false);
                SelectedConnectors.Clear();
            }
        }

        public void MoveSelectedItems(double dx, double dy)
        {
            SelectedNodes.ForEach(node => node.Move(dx, dy));
        }

        public void SetCurrentNode(BaseNodeViewModel? nodeVm)
        {
            if (CurrentNode == nodeVm)
                return;

            // clear focus on CurrentNode
            if (CurrentNode != null)
            {
                CurrentNode.IsFocused = false;
                CurrentNode = null;
            }

            CurrentNode = nodeVm;
            if (CurrentNode != null)
            {
                CurrentNode.IsFocused = true;
            }
            
        }

        public void SetCurrentConnector(GraphConnectorViewModel? connVm)
        {
            if (CurrentConnector == connVm)
                return;

            if (CurrentConnector != null)
            {
                CurrentConnector.IsFocused = false;
                CurrentConnector = null;
            }

            if (CurrentNode != null)
            {
                CurrentNode.IsFocused = false;
                CurrentNode = null;
            }

            CurrentConnector = connVm;
            if (CurrentConnector != null)
            {
                CurrentConnector.IsFocused = true;
            }
        }

        public void SelectNodeInBox(Rect box)
        {
            if (box.Width < 10 && box.Height < 10) return;
            //TODO Ctrl
            ClearSelectedItems("all");

            double l = box.Left;
            double r = box.Right;
            double t = box.Top;
            double b = box.Bottom;
            var selectNodeVms = new List<BaseNodeViewModel>();

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
                    selectNodeVms.Add(nodeVm);
                    SelectNode(nodeVm, false);
                }
            }
        }

        public void CreateGroupFromSelectedNodes()
        {
            var nodes = new List<Node>();
            SelectedNodes.ForEach(node => nodes.Add(node.Node));
            var group = Graph.CreateGroup(nodes);
            if (group != null)
            {
                var groupVm = new GroupBoxViewModel(group, this);
                groupVm.Nodes = SelectedNodes;
                Groups.Add(groupVm);
                groupVm.Resize();
            }
        }

        public void AddNewNode()
        {
            // add new node
            Debug.Assert(CurrentNode != null);
            var newNode = Node.DefaultNode.Clone(Graph);
            var vm = AddNode(newNode);
            if (vm == null)
                return;
            Graph.AddNode(newNode);
            Connect(CurrentNode.Node, newNode);
            NeedLayout = true;

            // set newnode as currentnode
            SelectNode(vm, true);

            Graph.Build();
        }

        #endregion

        #region CallBack

        void OnGraphPathChange(FlowChart.Core.Graph graph, string oldPath, string newPath)
        {
            RaisePropertyChanged(nameof(FullPath));
            RaisePropertyChanged(nameof(Name));
        }

        #endregion

        public void OnPreviewKeyDown(KeyEventArgs arg)
        {
            if (arg.Key == Key.Tab)
            {
                if (CurrentConnector != null)
                    CurrentConnector.OnKeyDown(arg);
                arg.Handled = true; // disable tab navigation
            }
        }
        public DelegateCommand<KeyEventArgs> OnPreviewKeyDownCommand { get; set; }
    }
}
