using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.AST;
using FlowChart.Type;
using FlowChartCommon;

namespace FlowChart.Core
{
    public class TreeItem : Item
    {
        public TreeItem(string name) : base(name)
        {
        }

        public virtual void Rename(string newName)
        {
            throw new NotImplementedException();
        }

        public Guid Uid;

        #region REF PROPERTY
        public Project Project { get; set; }
        public Type.Type Type { get; set; }

        public Folder? Parent { get; set; }
        #endregion

        public string JoinPath()
        {
            var list = new List<string>();
            var item = this;

            int i = 0;
            while (item != Project.Root)
            {
                list.Add(item.Name);
                item = item.Parent;
                if (i++ > 100)
                {
                    Logger.ERR("JoinPath Error");
                    break;
                }
            }

            list.Reverse();
            return string.Join('.', list);
        }

        public delegate void NameChangeDelegate(TreeItem item, string name);

        public NameChangeDelegate? NameChangeEvent;

        public void RaiseRenameEvent(string newName)
        {
            NameChangeEvent?.Invoke(this, newName);
        }
    }
    public class Graph : TreeItem
    {
        static Graph()
        {
            EmptyGraph = new Graph("empty") {Uid = Guid.Empty};
            EmptyGraph.AddNode(new StartNode());
        }
        public Graph(string Name)
        : base(Name)
        {
            Nodes = new List<Node>();
            NodeDict = new Dictionary<string, Node>();
            Connectors = new List<Connector>();
            Variables = new List<Variable>();
            Groups = new List<Group>();
            AutoLayout = true;
            IsLoaded = true;

#if DEBUG
            //GraphAddNodeEvent += node => Logger.DBG($"[event] add node event: {node}");
            //GraphRemoveNodeEvent += node => Logger.DBG($"[event] remove node event: {node}");
            //GraphConnectEvent += (conn, s, e) => Logger.DBG($"[event] connect event: {conn.Start} -> {conn.End}");
            //ConnectorRemoveEvent += (conn, s, e) => Logger.DBG($"[event] disconnect event: {conn.Start} -> {conn.End}");
#endif
        }

        #region Events
        public delegate void GraphNodeDelegate(Node node);
        public delegate void GraphConnectorDelegate(Connector conn, int startIdx, int endIdx);
        public delegate void GraphPathChangeHandler(Graph graph, string oldPath, string newPath);
        public delegate void GraphAddVariableDelegate(Graph graph, Variable variable);
        public delegate void GraphConnectorTypeChangeDelegate(Connector conn, Connector.ConnectorType oldValue);

        public event GraphPathChangeHandler GraphPathChangeEvent;
        public event GraphNodeDelegate? GraphRemoveNodeEvent;
        public event GraphNodeDelegate GraphAddNodeEvent;
        public event GraphConnectorDelegate GraphConnectEvent;
        public event GraphConnectorDelegate ConnectorRemoveEvent;
        public event GraphConnectorTypeChangeDelegate ConnectorTypeChangeEvent;
        public event GraphAddVariableDelegate? GraphAddVariableEvent;



        #endregion


        #region PROPERTY

        private string _path;
        public string Path
        {
            get => _path;
            set => SetPath(value);
        }
        
        public void SetPath(string path)
        {
            if (string.Equals(_path, path))
                return;
            var oldPath = _path;
            _path = path;
            GraphPathChangeEvent?.Invoke(this, oldPath, path);
        }

        public List<Node> Nodes { get; set; }
        public Dictionary<string, Node> NodeDict { get; set; }
        public Node Root => Nodes[0];
        public List<Connector> Connectors { get; set; }
        public List<Variable> Variables { get; set; }
        public List<Group> Groups { get; set; }
        public bool AutoLayout { get; set; }

        public bool IsSubGraph { get; set; }
        public FlowChart.Type.Type? ReturnType { get; set; }

        #region lazy load
        public bool IsLoaded { get; set; }
        public void LazyLoad()
        {
            if (LazyLoadFunc == null)
                return;
            lock (LazyLoadFunc)
            {
                if (IsLoaded)
                    return;
                LazyLoadFunc.Invoke();
                IsLoaded = true;
                LazyLoadCompletionSource?.SetResult();
            }
        }

        private Action? _lazyLoadFunc;
        public Action? LazyLoadFunc
        {
            get => _lazyLoadFunc;
            set
            {
                _lazyLoadFunc = value;
                if (_lazyLoadFunc != null)
                {
                    IsLoaded = false;
                    LazyLoadCompletionSource = new TaskCompletionSource();
                }
            }
        }
        public TaskCompletionSource? LazyLoadCompletionSource;
        #endregion


        public bool SetSubGraph(bool b)
        {
            if (IsSubGraph == b)
                return false;
            IsSubGraph = b;
            if (IsSubGraph)
            {
                ToMethod();
            }
            else
            {
                var method = Type.FindMember(Name, false);
                if (method != null)
                {
                    Type.RemoveMember(Name);
                }
            }
            return true;
        }

        public Method ToMethod()
        {
            var method = new Method(Name)
            {
                Type = ReturnType ?? BuiltinTypes.VoidType,
                SaveToFile = false
            };
            foreach (var v in Variables)
            {
                if (v.IsParameter)
                    method.Parameters.Add(new Parameter(v.Name)
                    {
                        Type = v.Type
                    });
            }

            //TODO when subgraph path changes, template should update
            method.IsAsync = true;
            if (method.Parameters.Count == 0)
                method.Template = $"asyncflow.call_sub(\"{Path}\", $caller)";
            else
                method.Template = $"asyncflow.call_sub(\"{Path}\", $caller, $params)";
            Type.AddMember(method);
            return method;
        }

        #endregion

        

        public void Build(ParserConfig? cfg = null)
        {
            if (cfg == null)
                cfg = new ParserConfig();
            Project.BuildGraph(this, cfg);
        }

        public void AddNode(Node? node)
        {
            if (node == null)
                return;
            if (NodeDict.ContainsKey(node.Uid))
                return;
            node.OwnerGraph = this;
            NodeDict.Add(node.Uid, node);
            Nodes.Add(node);
        }

        public void AddNode_atom(Node node, int idx = -1)
        {
            if(idx < 0)
                Nodes.Add(node);
            else
                Nodes.Insert(idx, node);
            node.OwnerGraph = this;
            NodeDict.Add(node.Uid, node);
            Logger.DBG($"AddNode_atom: {node}");
            GraphAddNodeEvent?.Invoke(node);
        }

        public void AddNodes(Node parent, List<Node> nodes)
        {
            if (nodes.Count == 0)
                return;
            var startNodes = new List<Node>();
            nodes.ForEach(node =>
            {
                var hasInnerParent = false;
                var hasOuterParent = false;
                foreach (var line in node.Parents)
                {
                    if (line.Start != node)
                    {
                        if (nodes.Contains(line.Start))
                            hasInnerParent = true;
                        else
                            hasOuterParent = true;
                    }
                }
                if (!hasInnerParent || hasOuterParent)
                    startNodes.Add(node);
            });

            if(startNodes.Count == 0)
                startNodes.Add(nodes[0]);

            Dictionary<Node, Node> nodesMap = new Dictionary<Node, Node>();

            
            Action<Node> addNodeFunc = null;
            addNodeFunc = node =>
            {
                if (nodesMap.ContainsKey(node))
                    return;

                var newNode = node.Clone(this);
                AddNode_atom(newNode);
                nodesMap[node] = newNode;

                node.Children.ForEach(line =>
                {
                    if (nodes.Contains(line.End))
                    {
                        addNodeFunc(line.End);
                        Connect_atom(newNode, nodesMap[line.End], line.ConnType);
                    }
                });
                
            };

            // 递归添加所有节点及线条
            foreach (var node in startNodes)
            {
                addNodeFunc(node);
                Connect_atom(parent, nodesMap[node], Connector.ConnectorType.ALWAYS);
            }
        }

        public void RemoveNode_atom(Node? node)
        {
            if (node == null)
                return;
            var tmp = GetNode(node.Uid);
            Debug.Assert(tmp == node);
            NodeDict.Remove(node.Uid);
            Nodes.Remove(node);
            Logger.DBG($"RemoveNode_atom: {node}");
            GraphRemoveNodeEvent?.Invoke(node);
        }

        public void RemoveNode(Node node)
        {
            var idxMap = new Dictionary<Node, int>();
            // remove connectors
            var parents = node.Parents.ConvertAll(conn => conn.Start);

            parents.ForEach(p =>
            {
                int idx = p.Children.FindIndex(conn => conn.End == node);
                idxMap.Add(p, idx);
                RemoveConnector_atom(p, node);
            });

            var children = node.Children.ConvertAll(conn => conn.End);
            children.ForEach(c => RemoveConnector_atom(node, c));

            // remove node
            RemoveNode_atom(node);

            // find a parent node who can connect to start node after node deleted
            var parent = parents.Find(IsNodeConnectToRoot) ?? Root;
            int idx = idxMap[parent];

            // add orphan nodes to parent node
            foreach (var child in children)
            {
                if (!IsNodeConnectToRoot(child))
                {
                    Connect_atom(parent, child, Connector.ConnectorType.DELETE, idx);
                }
            }
        }

        public void RaiseConnectorTypeChangeEvent(Connector conn, Connector.ConnectorType oldValue)
        {
            ConnectorTypeChangeEvent?.Invoke(conn, oldValue);
        }

        private bool IsNodeConnectToRoot(Node node)
        {
            var startNode = Nodes[0];
            Queue<Node> nodeQueue = new Queue<Node>();
            HashSet<Node> HandledNodes = new HashSet<Node>();
            nodeQueue.Enqueue(node);
            HandledNodes.Add(node);

            while (nodeQueue.Count > 0)
            {
                var curNode = nodeQueue.Dequeue();
                if (curNode == startNode)
                    return true;

                curNode.Parents.ForEach(conn =>
                {
                    var parent = conn.Start;
                    if (!HandledNodes.Contains(parent))
                    {
                        HandledNodes.Add(parent);
                        nodeQueue.Enqueue(parent);
                    }
                });
            }

            return false;
        }

        public Node? GetNode(string uid)
        {
            Node node;
            if (NodeDict.TryGetValue(uid, out node))
                return node;
            Logger.WARN($"invalid uid {uid}");
            return null;
        }

        public void Connect(string uidStart, string uidEnd, Connector.ConnectorType connType)
        {
            Connect(GetNode(uidStart), GetNode(uidEnd), connType);
        }

        public Connector? Connect(Node? start, Node? end, Connector.ConnectorType connType)
        {
            if (start == null || end == null)
                return null;
            
            return Connect_atom(start, end, connType);
        }

        public Connector? Connect_atom(Node start, Node end, Connector.ConnectorType connType
            , int startIdx = -1, int endIdx = -1)
        {
            // check exist connector
            var conn = start.Parents.Find(conn => conn.End == end);
            if (conn != null)
                return null;
            conn = new Connector(start, end, connType);
            Connectors.Add(conn);
            if (startIdx < 0)
                conn.Start.Children.Add(conn);
            else
                conn.Start.Children.Insert(startIdx, conn);
            if (endIdx < 0)
                conn.End.Parents.Add(conn);
            else
                conn.End.Parents.Insert(endIdx, conn);
            // Logger.DBG($"Connect_atom: {start} -> {end}");
            GraphConnectEvent?.Invoke(conn, startIdx, endIdx);
            return conn;
        }

        public void RemoveConnector(Connector conn)
        {
            var start = conn.Start;
            var end = conn.End;
            var connType = conn.ConnType;

            // check if connector remove
            conn.Start = conn.End;

            bool connectToRoot = IsNodeConnectToRoot(end);
            conn.Start = start;

            if (!connectToRoot)
            {
                conn.ConnType = Connector.ConnectorType.DELETE;
                return;
            }

            RemoveConnector_atom(conn.Start, conn.End);
        }


        public void RemoveConnector_atom(Node start, Node end)
        {
            var conn = start.Children.Find(conn => conn.End == end);
            if (conn == null)
                return;
            Connectors.Remove(conn);
            var startIdx = start.Children.IndexOf(conn);
            start.Children.Remove(conn);
            var endIdx = end.Parents.IndexOf(conn);
            end.Parents.Remove(conn);
            Logger.DBG($"RemoveConnector_atom: {start} -> {end}");
            ConnectorRemoveEvent?.Invoke(conn, startIdx, endIdx);
        }

        public void ChangeConnectorType_atom(Connector conn, Connector.ConnectorType connType)
        {
            Logger.DBG($"ChangeConnectorType_atom: {connType}");
            conn.ConnType = connType;
        }

        public bool AddVariable(Variable v)
        {
            if (GetVar(v.Name) != null)
                return false;
            Variables.Add(v);
            GraphAddVariableEvent?.Invoke(this, v);
            return true;
        }

        public Variable? GetVar(string varName)
        {
            return Variables.Find(v => v.Name == varName);
        }

        public Variable GetOrAddVariable(string name)
        {
            var v = GetVar(name);
            if (v != null)
            {
                return v;
            }
            else
            {
                v = new Variable(name);
                AddVariable(v);
                return v;
            }
        }

        public Group? CreateGroup(List<Node> nodes)
        {
            var group = new Group("--");
            group.Nodes.AddRange(nodes);
            Groups.Add(group);
            nodes.ForEach(node => node.OwnerGroup = group);
            return group;
        }

        public static Graph EmptyGraph { get; set; }

        public virtual Graph Clone()
        {
            var graph = new Graph(Name) {Path = Path, Project = Project, Type = Type, Uid = Project.GenUUID()};
            var nodeDict = new Dictionary<Node, Node>();
            Nodes.ForEach(node =>
            {
                var newNode = node.Clone(graph);
                nodeDict.Add(node, newNode);
                graph.AddNode(newNode);
            });

            Connectors.ForEach(conn =>
            {
                graph.Connect(nodeDict[conn.Start], nodeDict[conn.End], conn.ConnType);
            });
            return graph;
        }

        public override void Rename(string newName)
        {
            var parentPath = Parent.JoinPath();
            Debug.Assert(Path == $"{parentPath}.{Name}");

            Name = newName;
            var newPath = $"{parentPath}.{Name}";
            Project.GraphDict.Remove(Path);
            Path = newPath;
            Project.GraphDict.Add(newPath, this);

            RaiseRenameEvent(newName);
        }
    }
}
