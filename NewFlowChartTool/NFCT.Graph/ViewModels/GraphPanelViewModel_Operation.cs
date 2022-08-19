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
        public BaseNodeViewModel? AddNodeViewModel(Node node, int idx = -1)
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

        // used when GraphViewModel create
        void _createConnectorViewModel(Connector conn)
        {
            var start = conn.Start;
            var end = conn.End;
            BaseNodeViewModel? startVm, endVm;
            if (NodeDict.TryGetValue(start, out startVm) && NodeDict.TryGetValue(end, out endVm))
            {
                var connVm = new GraphConnectorViewModel(conn, startVm, endVm, this);
                Connectors.Add(connVm);
                ConnectorDict.Add(conn, connVm);
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
            var connVm = new GraphConnectorViewModel(conn, startVm, endVm, this);
            Connectors.Add(connVm);
            ConnectorDict.Add(conn, connVm);
        }
        public void AddNewNode()
        {
            // add new node
            Debug.Assert(CurrentNode != null);
            var newNode = Node.DefaultNode.Clone(Graph);
            var vm = AddNodeViewModel(newNode);
            if (vm == null)
                return;
            Graph.AddNode(newNode);
            Connect(CurrentNode.Node, newNode);
            NeedLayout = true;

            // set newnode as currentnode
            SetCurrentNode(vm);

            Graph.Build();
        }

        public void OnAddNode(Node node)
        {
            var idx = _graph.Nodes.IndexOf(node);
            AddNodeViewModel(node, idx);
        }

        public void RemoveNodeOperation(BaseNodeViewModel nodeVm)
        {
            UndoRedoManager.Begin("Remove Node");
            var node = nodeVm.Node;
            Graph.RemoveNode(node);
            NeedLayout = true;
            UndoRedoManager.End();
        }

        public void RemoveNodeViewModel(Node node)
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
                    _removeNodeViewModel(node);
                },
                () =>
                {
                    Logger.DBG("undo");
                    //AddNodeViewModel(node, idx);
                    Graph.AddNode_atom(node, idx);
                });
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

        public void RemoveConnectorViewModel(Connector conn)
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
