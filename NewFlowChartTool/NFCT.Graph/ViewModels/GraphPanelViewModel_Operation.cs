﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FlowChart.Core;
using FlowChart.Common;
using System.Windows.Media;

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
            {
                vm.Id = Nodes.Count;
                Nodes.Add(vm);
            }
            else
            {
                vm.Id = idx;
                Nodes.Insert(idx, vm);
            }

            NodeDict.Add(node, vm);
            NeedLayout = true;
            IsDirty = true;
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
            AddParentNodeGroup(CurrentNode.Node, vm);
            NeedLayout = true;
            IsDirty = true;
            // set newnode as currentnode
            SetCurrentNode(vm);
            //Build();
            UndoRedoManager.End();
        }

        public Dictionary<Node, Node> PasteNodesOperation(Node parent, List<Node> nodes, bool isClip)
        {
            UndoRedoManager.Begin("Paste Nodes");
            var nodesMap = Graph.AddNodes(parent, nodes);
            if (isClip)
            {
                nodes.ForEach(Graph.RemoveNode);
            }
            NeedLayout = true;
            IsDirty = true;
            UndoRedoManager.End();
            return nodesMap;
        }

        public void OnAddNode(Node node)
        {
            var idx = _graph.Nodes.IndexOf(node);
            _tmpNewBaseNodeViewModel = _addNodeViewModel(node, idx);
            UndoRedoManager.AddAction(
                () => 
                {
                    Graph.AddNode_atom(node, idx);
                },
                () =>
                {
                    Graph.RemoveNode_atom(node);
                });
            NeedLayout = true;
            IsDirty = true;
        }

        // invoked when keydown a selected connector, will insert a node in the connector
        public void InsertNewNodeOperation()
        {
            UndoRedoManager.Begin("Insert New Node");
            // add new node
            Debug.Assert(CurrentConnector != null);
            var newNode = Node.DefaultNode.Clone(Graph);
            Graph.AddNode_atom(newNode);

            var vm = _tmpNewBaseNodeViewModel;
            _tmpNewBaseNodeViewModel = null;
            if (vm == null)
                return;
            //Connect(CurrentNode.Node, newNode);
            var conn = CurrentConnector.Connector;
            AddParentNodeGroup(conn.Start, vm);
            int index = conn.Start.Children.IndexOf(conn);
            Graph.Connect_atom(conn.Start, newNode, conn.ConnType, index);
            Graph.Connect_atom(newNode, conn.End, conn.ConnType);
            Graph.RemoveConnector_atom(conn.Start, conn.End);
            NeedLayout = true;
            // set newnode as currentnode
            SetCurrentNode(vm);
            Build();
            UndoRedoManager.End();
        }

        void AddParentNodeGroup(Node Node, BaseNodeViewModel newBaseNode)
        {
            var curNodeGroup = Node.OwnerGroup;
            if (curNodeGroup != null)
            {
                curNodeGroup.Nodes.Add(newBaseNode.Node);
                newBaseNode.Node.OwnerGroup = curNodeGroup;
                foreach (var g in Groups)
                {
                    if (g.Group == curNodeGroup)
                    {
                        g.Nodes.Add(newBaseNode);
                        g.Resize();
                        break;
                    }
                }
            }
        }
        #endregion

        #region remove nodes
        public void RemoveNodesOperation(List<BaseNodeViewModel> nodeVms)
        {
            if (nodeVms.Count == 0)
            {
                Logger.WARN("delete nodes failed, no nodes selected");
                return;
            }

            UndoRedoManager.Begin("Remove Nodes");
            foreach (var nodeVm in nodeVms)
            {
                var node = nodeVm.Node;
                if (node.OwnerGroup != null)
                {
                    node.OwnerGroup.Nodes.Remove(node);
                    foreach (var g in Groups)
                    {
                        var rn = g.Nodes.Find(n => n.Node == node);
                        if (rn != null)
                        {
                            g.Nodes.Remove(rn);
                        }
                    }
                    for (int i = Groups.Count - 1; i >= 0; i--)
                    {
                        if (Groups[i].Nodes.Count == 0)
                        {
                            Groups.RemoveAt(i);
                            Graph.Groups.RemoveAt(i);
                        }
                    }
                }
                Graph.RemoveNode(node);
            }

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
                    Graph.RemoveNode_atom(node);
                },
                () =>
                {
                    //AddNodeViewModel(node, idx);
                    Graph.AddNode_atom(node, idx);
                });
            NeedLayout = true;
            IsDirty = true;
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

        #region node change

        public void ChangeNodeContentOperation(Node node, string content)
        {
            UndoRedoManager.Begin("node text change");
            node.SetText(content);
            UndoRedoManager.End();
        }

        void OnNodeContentChange(Node node, string oldText, string newText)
        {
            UndoRedoManager.AddAction(
                () => { node.SetText(newText); },
                () => { node.SetText(oldText); });
            IsDirty = true;
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

        void OnConnect(Connector conn, int startIdx, int endIdx)
        {
            Debug.Assert(conn.OwnerGraph == _graph);
            _tmpNewConnectorViewModel = _createConnectorViewModel(conn);
            UndoRedoManager.AddAction(
                () => { Graph.Connect_atom(conn.Start, conn.End, conn.ConnType, startIdx, endIdx); },
                () => {Graph.RemoveConnector_atom(conn.Start, conn.End); });
            NeedLayout = true;
            IsDirty = true;
        }
        #endregion

        #region disconnect

        public void RemoveConnectorOperation(GraphConnectorViewModel connVm)
        {
            UndoRedoManager.Begin("Disconnect");
            Graph.RemoveConnector(connVm.Connector);
            UndoRedoManager.End();
        }

        void OnRemoveConnector(Connector conn, int startIdx, int endIdx)
        {
            Debug.Assert(conn.OwnerGraph == _graph);
            if (!ConnectorDict.ContainsKey(conn))
            {
                Logger.WARN($"remove connector viewmodel failed! cannot find viewmodel for {conn}");
                return;
            }

            var vm = ConnectorDict[conn];
            Logger.DBG($"connector is remove {vm}");

            Connectors.Remove(vm);
            ConnectorDict.Remove(conn);

            UndoRedoManager.AddAction(
                () => { _graph.RemoveConnector_atom(conn.Start, conn.End); },
                () => { _graph.Connect_atom(conn.Start, conn.End, conn.ConnType, startIdx, endIdx); });
            NeedLayout = true;
            SetCurrentNode(vm.StartNode);
            IsDirty = true;
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
            IsDirty = true;
        }
        #endregion

        #region switch child Nodes

        public void SwitchChildNodeOrder_Operation(GraphConnectorViewModel conn, int direction)
        {
            var lines = conn.StartNode.ChildLines;
            var idx = lines.IndexOf(conn);
            var id = idx + direction;
            if (id < 0 || id >= conn.StartNode.ChildLines.Count)
                return;
            UndoRedoManager.Begin("change connector order");
            Graph.SwitchChildNodeOrder_atom(conn.Connector, lines[id].Connector);
            UndoRedoManager.End();
        }

        void OnSwitchChildNodeOrder(Node node, int ida, int idb)
        {
            var nodeVm = GetNodeVm(node);
            var lines = nodeVm.ChildLines;
            var ca = lines[ida];
            var cb = lines[idb];
            lines[idb] = ca;
            lines[ida] = cb;
            
            Relayout();

            UndoRedoManager.AddAction(
                () => { Graph.SwitchChildNodeOrder_atom(ca.Connector, cb.Connector); },
                () => { Graph.SwitchChildNodeOrder_atom(cb.Connector, ca.Connector); }
            );
            IsDirty = true;
        }

        #endregion

        #region Paste Nodes

        #endregion

        #region Group Operation
        public void CreateGroupFromSelectedNodes()
        {
            if (SelectedNodes.Count == 0) return;

            var nodes = new List<Node>();
            SelectedNodes.ForEach(node =>
            {
                if (node.Node is StartNode)
                    return;
                nodes.Add(node.Node);
            });

            var outerParentNodes = Graph.GetRoots(nodes);
            if (outerParentNodes.Count != 1)
            {
                MessageBox.Show("创建group的节点，应该有公共的根节点");
                return;
            }

            UndoRedoManager.Begin("add group");
            Graph.CreateGroup(nodes);
            UndoRedoManager.End();
            NeedLayout = true;
        }

        public void RemoveGroupOperation()
        {
            if (CurrentGroup == null)
                return;

            UndoRedoManager.Begin("remove group");
            Graph.RemoveGroup(CurrentGroup.Group);
            UndoRedoManager.End();
            NeedLayout = true;
        }

        void OnGroupRemove(Group group)
        {
            var vm = GetGroupVm(group);
            if (vm == null)
                return;
            Groups.Remove(vm);
            UndoRedoManager.AddAction(
                () => { _graph.RemoveGroup_atom(group); },
                () => { _graph.AddGroup_atom(group); });
        }

        void _addGroupViewModel(Group group)
        {
            var groupVm = new GroupBoxViewModel(group, this);
            var nodesVM = group.Nodes.ConvertAll(GetNodeVm);
            nodesVM.ForEach(node => node.IdBackGroundColor = Brushes.Black);
            groupVm.Nodes.AddRange(nodesVM);
            Groups.Add(groupVm);
        }

        void OnGroupAdd(Group group)
        {
            _addGroupViewModel(group);

            UndoRedoManager.AddAction(
                () => { _graph.AddGroup_atom(group); },
                () => { _graph.RemoveGroup_atom(group); });
        }

        void OnNodeGroupChange(Node node, Group? oldGroup, Group? newGroup)
        {
            var nodeVm = GetNodeVm(node);
            var groupVm = GetGroupVm(newGroup);
            var oldGroupVm = GetGroupVm(oldGroup);
            if(oldGroupVm != null)
                oldGroupVm.RemoveNode(nodeVm);
            nodeVm.OwnerGroup = groupVm;
            ChangeGroupBackColor(node);
            if (groupVm != null)
                groupVm.AddNode(nodeVm);
            UndoRedoManager.AddAction(
                () => { _graph.SetGroupForNode_atom(node, newGroup); },
                () => { _graph.SetGroupForNode_atom(node, oldGroup); });
        }

        void ChangeGroupBackColor(Node node)
        {
            Brush brush = node.OwnerGroup != null ? Brushes.Black : Brushes.DarkGoldenrod;
            var nodeVm = NodeDict[node];
            nodeVm.IdBackGroundColor = brush;
        }
        #endregion
    }
}
