using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChartCommon;

namespace NFCT.Graph.ViewModels
{
    public partial class GraphPaneViewModel 
    {
        #region add node
        private BaseNodeViewModel? _tmpNewBaseNodeViewModel;
        public BaseNodeViewModel? _addNodeViewModel(Node node, int idx = -1)
        {
            Debug.Assert(node.OwnerGraph == _graph);
            var vm = BaseNodeViewModel.CreateNodeViewModel(node, this);
            if (vm == null)
                return null;
            if (idx < 0)
                Nodes.Add(vm);
            else
                Nodes.Insert(idx, vm);

            NodeDict.Add(node, vm);
            NeedLayout = true;
            return vm;
        }

        public void AddNewNodeOperation()
        {
            UndoRedoManager.Begin("Add New Node");
            // add new node
            Debug.Assert(CurrentNode != null);
            var newNode = Node.DefaultNode.Clone(Graph);
            Graph.AddNode_atom(newNode);

            var vm = _tmpNewBaseNodeViewModel;
            _tmpNewBaseNodeViewModel = null;
            if (vm == null)
                return;
            //Connect(CurrentNode.Node, newNode);
            Graph.Connect_atom(CurrentNode.Node, newNode, Connector.ConnectorType.ALWAYS);
            NeedLayout = true;
            // set newnode as currentnode
            SetCurrentNode(vm);
            Build();
            UndoRedoManager.End();
        }

        public void OnAddNode(Node node)
        {
            var idx = _graph.Nodes.IndexOf(node);
            _tmpNewBaseNodeViewModel = _addNodeViewModel(node, idx);
            NeedLayout = true;
        }
        #endregion

        #region remove node
        public void RemoveNodeOperation(BaseNodeViewModel nodeVm)
        {
            UndoRedoManager.Begin("Remove Node");
            var node = nodeVm.Node;
            Graph.RemoveNode(node);
            NeedLayout = true;
            UndoRedoManager.End();
        }

        void OnRemoveNode(Node node)
        {
            Logger.DBG($"view model for {node} is removed from graph viewmodel");
            if (!NodeDict.ContainsKey(node))
            {
                Logger.WARN($"remove node viewmodel failed! cannot find viewmodel for {node}");
                return;
            }

            var idx = _removeNodeViewModel(node);
            UndoRedoManager.AddAction(
                () =>
                {
                    Logger.DBG("redo");
                    Graph.RemoveNode_atom(node);
                },
                () =>
                {
                    Logger.DBG("undo");
                    //AddNodeViewModel(node, idx);
                    Graph.AddNode_atom(node, idx);
                });
            NeedLayout = true;
        }

        private int _removeNodeViewModel(Node node)
        {
            var vm = NodeDict[node];
            Debug.Assert(vm.Node == node);
            NodeDict.Remove(vm.Node);
            int index = Nodes.IndexOf(vm);
            Nodes.Remove(vm);
            return index;
        }

        #endregion

        #region connect

        private GraphConnectorViewModel? _tmpNewConnectorViewModel;
        // used when GraphViewModel create
        GraphConnectorViewModel? _createConnectorViewModel(Connector conn)
        {
            var start = conn.Start;
            var end = conn.End;
            BaseNodeViewModel? startVm, endVm;
            if (NodeDict.TryGetValue(start, out startVm) && NodeDict.TryGetValue(end, out endVm))
            {
                var connVm = new GraphConnectorViewModel(conn, startVm, endVm, this);
                Connectors.Add(connVm);
                ConnectorDict.Add(conn, connVm);
                return connVm;
            }

            return null;
        }

        public void ConnectOperation(Node start, Node end)
        {
            UndoRedoManager.Begin("Connect Nodes");
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

            Graph.Connect_atom(start, end, Connector.ConnectorType.ALWAYS);

           
            var connVm = _tmpNewConnectorViewModel;
            _tmpNewConnectorViewModel = null;
            if (connVm != null)
            {
                SetCurrentConnector(connVm);
            }
           
            UndoRedoManager.End();
        }

        void OnConnect(Connector conn)
        {
            Debug.Assert(conn.OwnerGraph == _graph);
            _tmpNewConnectorViewModel = _createConnectorViewModel(conn);
            UndoRedoManager.AddAction(
                () => { Graph.Connect_atom(conn.Start, conn.End, conn.ConnType); },
                () => {Graph.RemoveConnector_atom(conn.Start, conn.End); });
            NeedLayout = true;
        }
        #endregion

        #region disconnect

        public void RemoveConnectorOperation(GraphConnectorViewModel connVm)
        {
            UndoRedoManager.Begin("Disconnect");
            Graph.RemoveConnector(connVm.Connector);
            UndoRedoManager.End();
        }

        void OnRemoveConnector(Connector conn)
        {
            Debug.Assert(conn.OwnerGraph == _graph);
            if (!ConnectorDict.ContainsKey(conn))
            {
                Logger.WARN($"remove connector viewmodel failed! cannot find viewmodel for {conn}");
                return;
            }

            var vm = ConnectorDict[conn];

            var starts = vm.StartNode.ChildLines;
            var startIdx = starts.IndexOf(vm);
            starts.Remove(vm);

            var ends = vm.EndNode.ParentLines;
            var endIdx = ends.IndexOf(vm);
            ends.Remove(vm);

            Logger.DBG($"connector is remove {vm}");

            Connectors.Remove(vm);
            ConnectorDict.Remove(conn);

            UndoRedoManager.AddAction(
                () =>
                {
                    var start = conn.Start;
                    var c = start.Children.Find(_c => _c.End == conn.End);
                    var connVm = ConnectorDict[c];
                    connVm.StartNode.ChildLines.Remove(connVm);
                    connVm.EndNode.ChildLines.Remove(connVm);
                    Connectors.Remove(connVm);
                    ConnectorDict.Remove(c);
                },
                () =>
                {
                    _graph.Connect_atom(conn.Start, conn.End, conn.ConnType, startIdx, endIdx);
                });
            NeedLayout = true;
        }
        #endregion

        #region change connector type

        public void ChangeConnectorType_Operation(Connector conn, Connector.ConnectorType newValue)
        {
            UndoRedoManager.Begin("change connector type");
            Graph.ChangeConnectorType_atom(conn, newValue);
            UndoRedoManager.End();
        }

        public void OnConnectorTypeChange(Connector conn, Connector.ConnectorType oldValue)
        {
            Debug.Assert(ConnectorDict.ContainsKey(conn));
            var vm = ConnectorDict[conn];
            vm.OnConnectTypeChange(conn);
            var newValue = conn.ConnType;

            UndoRedoManager.AddAction(
                () => { Graph.ChangeConnectorType_atom(conn, newValue); },
                () => { Graph.ChangeConnectorType_atom(conn, oldValue); }
            );
        }
        #endregion

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
    }
}
