using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.AST;
using FlowChart.Type;
using FlowChart.Common;

namespace FlowChart.Core
{
    public class TreeItem : Item
    {
        public TreeItem(string name) : base(name)
        {
            Path = "";
        }

        public virtual void Rename(string newName)
        {
            throw new NotImplementedException();
        }

        public Guid Uid;

        #region REF PROPERTY
        public Project Project { get; set; }
        private Type.Type _type;
        public Type.Type Type
        {
            get
            {
                if(_type != null)
                    return _type;
                if (this is Folder folder)
                {
                    foreach (var child in folder.Children)
                    {
                        if (child.Type != null)
                        {
                            _type = child.Type;
                            return _type;
                        }
                    }
                }
                return null;

            }
            set
            {
                if (_type == value)
                    return;
                var oldType = _type;
                _type = value;
                TypeChangeEvent?.Invoke(this, oldType);
            }
        }

        public virtual string Path { get; set; }

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
        public event Action<TreeItem, Type.Type>? TypeChangeEvent;

        public void RaiseRenameEvent(string newName)
        {
            NameChangeEvent?.Invoke(this, newName);
        }

        public Folder? GetRoot()
        {
            var parent = Parent;
            while (true)
            {
                if (parent == null)
                    return null;
                if (parent.IsRootFolder)
                    return parent;
                parent = parent.Parent;
            }
        }
    }
    public class Graph : TreeItem
    {
        public enum SubGraphTypeEnum
        {
            NONE = 0,
            LOCAL = 1,
            GLOBAL = 2
        }

        static Graph()
        {
            EmptyGraph = new Graph("empty") {Uid = Guid.Empty};
            EmptyGraph.AddNode(new StartNode());
        }
        public Graph(string Name)
        : base(Name)
        {
            Nodes = new List<Node>();
            NodeDict = new Dictionary<Guid, Node>();
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
        public delegate void GraphSwitchChildNodeOrderDelegate(Node node, int ida, int idb);

        public event GraphPathChangeHandler GraphPathChangeEvent;
        public event GraphNodeDelegate? GraphRemoveNodeEvent;
        public event GraphNodeDelegate GraphAddNodeEvent;
        public event GraphConnectorDelegate GraphConnectEvent;
        public event GraphConnectorDelegate ConnectorRemoveEvent;
        public event GraphConnectorTypeChangeDelegate ConnectorTypeChangeEvent;
        public event GraphAddVariableDelegate? GraphAddVariableEvent;
        public event GraphSwitchChildNodeOrderDelegate? GraphSwitchChildNodeOrderEvent;
        public event Action<Node, string, string>? GraphNodeChangeEvent;
        public event Action<Node, Group?, Group?>? GraphNodeGroupChangeEvent;
        public event Action<Group>? GraphAddGroupEvent;
        public event Action<Group>? GraphRemoveGroupEvent;

        public void OnNodeChange(Node node, string oldText, string newText)
        {
            GraphNodeChangeEvent?.Invoke(node, oldText, newText);
        }
        #endregion


        #region PROPERTY

        private string _path;
        public override string Path
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
            Project?.RenameItem(this, oldPath, path);
            GraphPathChangeEvent?.Invoke(this, oldPath, path);
        }

        public List<Node> Nodes { get; set; }
        public Dictionary<Guid, Node> NodeDict { get; set; }
        public Node Root => Nodes[0];
        public List<Connector> Connectors { get; set; }
        public List<Variable> Variables { get; set; }
        public List<Group> Groups { get; set; }
        public bool AutoLayout { get; set; }

        public bool IsSubGraph => SubGraphType != SubGraphTypeEnum.NONE;
        public bool IsGlobalSubGraph => SubGraphType == SubGraphTypeEnum.GLOBAL;
        public bool IsLocalSubGraph => SubGraphType == SubGraphTypeEnum.LOCAL;
        public SubGraphTypeEnum SubGraphType { get; set; }
        private Type.Type? _returnType;
        public FlowChart.Type.Type? ReturnType
        {
            get => _returnType;
            set
            {
                if (value == _returnType)
                    return;
                _returnType = value;
                if (IsLocalSubGraph)
                {
                    var member = Parent.LocalSubGraphs.Find(method => method.RelativeGraph == this);
                    if (member != null)
                    {
                        member.Type = _returnType;
                    }
                }
                else if (IsGlobalSubGraph)
                {
                    var member = Type.FindMember(Name, false);
                    if (member != null)
                    {
                        member.Type = _returnType;
                    }
                }
            }
        }
        public string? SaveFilePath;
        public string? GenerateFilePath;
        #endregion

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

        #region SubGraph Type

        public Action<SubGraphTypeEnum>? SubGraphChangeEvent;
        public bool SetSubGraph(SubGraphTypeEnum subType)
        {
            var oldType = SubGraphType;
            if (oldType == subType)
                return false;

            // will conflict with global member, return false
            if (subType == SubGraphTypeEnum.GLOBAL)
            {
                if (Type.FindMember(Name) != null)
                    return false;
            }


            if (oldType == SubGraphTypeEnum.LOCAL)
            {
                Parent?.RemoveLocalSubGraph(this);
            }
            else if (oldType == SubGraphTypeEnum.GLOBAL)
            {
                var method = Type.FindMember(Name, false);
                if (method != null)
                {
                    Type.RemoveMember(Name);
                }
            }

            SubGraphType = subType;
            if (subType == SubGraphTypeEnum.LOCAL)
            {
                Parent?.AddLocalSubGraph(this);
            }
            else if (subType == SubGraphTypeEnum.GLOBAL)
            {
                var method = ToMethod();
                Type.AddMember(method);
            }
            SubGraphChangeEvent?.Invoke(SubGraphType);
            return true;
        }

        public SubGraphMethod ToMethod()
        {
            var method = new SubGraphMethod(Name)
            {
                Type = ReturnType ?? BuiltinTypes.VoidType,
                SaveToFile = false,
                RelativeGraph = this
            };
            foreach (var v in Variables)
            {
                if (v.IsParameter)
                {
                    var para = new Parameter(v.Name)
                    {
                        Type = v.Type,
                        Description = v.Description,
                        Default = v.DefaultValue
                    };
                    method.Parameters.Add(para);
                }
                    
            }

            method.Type = Type;

            //TODO when subgraph path changes, template should update
            method.IsAsync = true;
            method.Update();
            return method;
        }

        public Member? FindMember(FlowChart.Type.Type type, string name)
        {
            if (Parent != null)
            {
                var method = Parent.FindSubGraphMethod(name);
                if(method != null && method.RelativeGraph.Type == type)
                    return method;
            }

            return type.FindMember(name);
        }
        #endregion


        public void ResetUid()
        {
            Uid = Project.GenUUID();
            Nodes.ForEach(node => node.Uid = Project.GenUUID());
        }


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

        public Dictionary<Node, Node> AddNodes(Node parent, List<Node> nodes)
        {
            Dictionary<Node, Node> nodesMap = new Dictionary<Node, Node>();
            if (nodes.Count == 0)
                return nodesMap;
            var startNodes = GetRoots(nodes);

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

            return nodesMap;
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
            if (node is StartNode)
                return;
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
            int idx = -1;
            if (idxMap.ContainsKey(parent))
                idx = idxMap[parent];

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

        private bool IsNodeConnectToRoot(Node node, Node startNode, Node obstacleNode)
        {
            Queue<Node> nodeQueue = new Queue<Node>();
            var handledNodes = new HashSet<Node>();
            nodeQueue.Enqueue(node);
            handledNodes.Add(node);
            handledNodes.Add(obstacleNode);

            while (nodeQueue.Count > 0)
            {
                var curNode = nodeQueue.Dequeue();
                if (curNode == startNode)
                    return true;

                curNode.Parents.ForEach(conn =>
                {
                    var parent = conn.Start;
                    if (!handledNodes.Contains(parent))
                    {
                        handledNodes.Add(parent);
                        nodeQueue.Enqueue(parent);
                    }
                });
            }

            return false;
        }

        public List<Node> FindSubGraph(Node root)
        {
            var startNode = Nodes[0];
            var subgraphNodes = new HashSet<Node>();
            var outsideNodes = new HashSet<Node>();
            var handledNodes = new HashSet<Node>();
            var nodeQueue = new Queue<Node>();
            
            handledNodes.Add(root);
            subgraphNodes.Add(root);
            root.Children.ForEach(conn => nodeQueue.Enqueue(conn.End));

            while (nodeQueue.Count > 0)
            {
                var curNode = nodeQueue.Dequeue();
                if (handledNodes.Contains(curNode))
                    continue;

                var isParentOutside = false;

                foreach (var conn in curNode.Parents)
                {
                    var parentNode = conn.Start;
                    if (!subgraphNodes.Contains(parentNode))
                    {
                        if (outsideNodes.Contains(parentNode))
                        {
                            isParentOutside = true;
                            break;
                        }
                        else
                        {
                            if (IsNodeConnectToRoot(parentNode, startNode, root))
                            {
                                outsideNodes.Add(parentNode);
                                isParentOutside = true;
                                break;
                            }
                            else
                            {
                                subgraphNodes.Add(parentNode);
                            }
                        }
                    }
                }

                if (isParentOutside)
                {
                    outsideNodes.Add(curNode);
                }
                else
                {
                    subgraphNodes.Add(curNode);
                    curNode.Children.ForEach(conn =>
                    {
                        nodeQueue.Enqueue(conn.End);
                    });
                }
            }

            subgraphNodes.Remove(root);
            return subgraphNodes.ToList();
        }

        public Node? GetNode(Guid uid)
        {
            Node node;
            if (NodeDict.TryGetValue(uid, out node))
                return node;
            if(uid != Guid.Empty)
                Logger.WARN($"invalid uid {uid}");
            return null;
        }

        public void Connect(Guid uidStart, Guid uidEnd, Connector.ConnectorType connType)
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
            var conn = start.Children.Find(conn => conn.End == end);
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

        public void RemoveVariable(string name)
        {
            var v = GetVar(name);
            if (v != null)
            {
                Variables.Remove(v);
            }
        }

        #region Group Operations

        public Group AddGroup_atom(Group group)
        {
            group.Name = $"group_{group.GetHashCode():X04}";
            Groups.Add(group);
            GraphAddGroupEvent?.Invoke(group);
            return group;
        }

        public Group? CreateGroup(List<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.OwnerGroup != null)
                {
                    Logger.WARN($"create group failed, node `{node}` is in another group `{node.OwnerGroup.Name}`");
                    return null;
                }
            }
            var group = new Group("--");
            AddGroup_atom(group);
            nodes.ForEach(node => SetGroupForNode_atom(node, group));
            return group;
        }

        public void RemoveGroup_atom(Group group)
        {
            Groups.Remove(group);
            GraphRemoveGroupEvent?.Invoke(group);
        }

        public void RemoveGroup(Group group)
        {
            var nodes = new List<Node>(group.Nodes);
            nodes.ForEach(node => SetGroupForNode_atom(node, null));
            RemoveGroup_atom(group);
        }

        public void SetGroupForNode_atom(Node node, Group? group)
        {
            var oldGroup = node.OwnerGroup;
            if (oldGroup == group)
                return;
            if(oldGroup != null)
                oldGroup.RemoveNode(node);
            node.OwnerGroup = group;
            if(group != null)
                group.Nodes.Add(node);
            GraphNodeGroupChangeEvent?.Invoke(node, oldGroup, group);
        }

        #endregion


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
            var oldName = Name;
            Name = newName;
            var newPath = $"{parentPath}.{Name}";
            Path = newPath;
            UpdateSubGraphInfo(oldName);
            RaiseRenameEvent(newName);
        }

        void UpdateSubGraphInfo(string oldName)
        {
            if (IsGlobalSubGraph)
            {
                var member = Type.FindMember(oldName, false);
                if (member is SubGraphMethod method && method.RelativeGraph == this)
                {
                    method.Update();
                    Type.RenameMember(oldName, Name);
                }
                else
                {
                    Logger.WARN("update method info failed: cannot find method when global subgraph rename");
                }
            }

            if (IsLocalSubGraph)
            {
                Parent?.RenameLocalSubGraph(this, oldName);
            }
        }

        public void SwitchChildNodeOrder_atom(Connector ca, Connector cb)
        {
            if (ca.Start != cb.Start)
                return;
            var conns = ca.Start.Children;
            var ida = conns.IndexOf(ca);
            var idb = conns.IndexOf(cb);
            if (ida == -1 || idb == -1)
                return;
            conns[ida] = cb;
            conns[idb] = ca;
            GraphSwitchChildNodeOrderEvent?.Invoke(ca.Start, ida, idb);
        }

        public void SortNodes()
        {
            if (Nodes.Count == 0)
                return;
            var startNode = Nodes[0];

            var nodeStack = new Stack<Node>();
            var nodeSet = new HashSet<Node>();
            nodeStack.Push(startNode);
            int id = 0;
            var nodes = new List<Node>();

            while (nodeStack.Count > 0)
            {
                var node = nodeStack.Pop();

                if (nodeSet.Contains(node))
                    continue;
                nodeSet.Add(node);
                node.Id = nodes.Count;
                nodes.Add(node);
                // left node push last, then pop last
                //node.ChildLines.Sort((a, b) => a.X.CompareTo(b.X));
                var childLines = node.Children;
                childLines.Reverse();
                childLines.ForEach(conn => nodeStack.Push(conn.End));
                childLines.Reverse();
            }

            if (nodes.Count < Nodes.Count)
            {
                // orphan nodes exist
                Logger.ERR($"{startNode.OwnerGraph.Path} exist orphan nodes");
                Nodes.ForEach(node =>
                {
                    if(!nodeSet.Contains(node))
                        nodes.Add(node);
                });
            }

            Nodes = nodes;
        }

        public void ReorderConnectors()
        {
            Connectors.Clear();
            Nodes.ForEach(node =>
            {
                Connectors.AddRange(node.Children);
            });
        }

        //TODO optimize
        public List<Node> GetRoots(List<Node> nodes)
        {
            var trees = UnionFind(nodes);
            var roots = new List<Node>();
            foreach (var tree in trees)
            {
                roots.AddRange(_getRoots(tree));
            }
            return roots;
        }

        List<Node> _getRoots(List<Node> nodes)
        {
            var roots = new List<Node>();
            if (nodes.Count == 0)
                return roots;
            var outerParentNodes = new List<Node>();
            nodes.ForEach(node =>
            {
                var hasInnerParent = false;
                foreach (var line in node.Parents)
                {
                    if (line.Start != node)
                    {
                        if (nodes.Contains(line.Start))
                            hasInnerParent = true;
                        else
                            outerParentNodes.Add(node);
                    }
                }
                if (!hasInnerParent)
                    roots.Add(node);
            });
            //ring node
            if (roots.Count == 0)
                roots.Add(outerParentNodes[0]);

            return roots;
        }

        private IEnumerable<List<Node>> UnionFind(List<Node> nodes)
        {
#if DEBUG
            foreach (var node in nodes)
            {
                Debug.Assert(Nodes[node.Id] == node);
            }
#endif

            var ids = new int[Nodes.Count];
            foreach (var node in nodes)
            {
                ids[node.Id] = node.Id;
            }

            var _root = (int i) =>
            {
                while (i != ids[i])
                {
                    i = ids[i];
                }

                return i;
            };

            var nodeSet = new HashSet<Node>(nodes);


            //TODO optimize
            // quick-union
            foreach (var node in nodes)
            {
                int p = _root(node.Id);
                foreach (var line in node.Children)
                {
                    var child = line.End;
                    if (child != node && nodeSet.Contains(child))
                    {
                        int q = _root(child.Id);
                        if (p != q)
                        {
                            Logger.DBG($"unionfind: union {p} {q}");
                            ids[q] = p; //TODO or ids[p] = q
                        }
                    }
                }
            }

            var result = new Dictionary<int, List<Node>>();

            //TODO optimize
            nodes.ForEach(node =>
            {
                var p = _root(node.Id);
                if(result.TryGetValue(p, out var list))
                    list.Add(node);
                else
                {
                    list = new List<Node>();
                    list.Add(node);
                    result[p] = list;
                }
            });

            return result.Values;
        }
    }
}
