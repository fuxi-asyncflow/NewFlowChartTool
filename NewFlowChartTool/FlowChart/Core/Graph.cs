using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Core
{
    public class Graph : Item
    {
        static Graph()
        {
            EmptyGraph = new Graph("empty");
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
        }

        public delegate void GraphNodeDelegate(Graph graph, Node node);
        public delegate void GraphConnectorDelegate(Graph graph, Connector conn);

        #region PROPERTY
        public string Uid { get; set; }

        private string _path;
        public string Path
        {
            get => _path;
            set => SetPath(value);
        }
        public delegate void GraphPathChangeHandler(Graph graph, string oldPath, string newPath);
        public event GraphPathChangeHandler GraphPathChangeEvent;
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
        #endregion

        #region REF PROPERTY
        public Project Project { get; set; }
        public Type.Type Type { get; set; }
        #endregion

        public void Build()
        {
            Project.BuildGraph(this);
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

        
        public event GraphNodeDelegate? GraphRemoveNodeEvent;

        public void RemoveNode_atom(Node? node)
        {
            if (node == null)
                return;
            var tmp = GetNode(node.Uid);
            Debug.Assert(tmp == node);
            NodeDict.Remove(node.Uid);
            Nodes.Remove(node);
            GraphRemoveNodeEvent?.Invoke(this, node);
        }

        public void RemoveNode(Node node)
        {
            // remove node
            RemoveNode_atom(node);

            // remove connectors
            var parents = node.Parents.ConvertAll(conn => conn.Start);
            parents.ForEach(p => RemoveConnector_atom(p, node));

            var children = node.Children.ConvertAll(conn => conn.End);
            children.ForEach(c => RemoveConnector_atom(node, c));

            // find a parent node who can connect to start node after node deleted
            var parent = parents.Find(IsNodeConnectToRoot) ?? Root;

            // add orphan nodes to parent node
            foreach (var child in children)
            {
                if (!IsNodeConnectToRoot(child))
                {
                    Connect_atom(parent, child, Connector.ConnectorType.DELETE);
                }
            }
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
            return NodeDict[uid];
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

        public event GraphConnectorDelegate? GraphConnectEvent;
        public Connector? Connect_atom(Node start, Node end, Connector.ConnectorType connType)
        {
            // check exist connector
            var conn = start.Parents.Find(conn => conn.End == end);
            if (conn != null)
                return null;
            conn = new Connector() { Start = start, End = end, ConnType = connType };
            Connectors.Add(conn);
            conn.Start.Children.Add(conn);
            conn.End.Parents.Add(conn);
            GraphConnectEvent?.Invoke(this, conn);
            return conn;
        }

        

        public void RemoveConnector_atom(Node start, Node end)
        {
            var conn = start.Children.Find(conn => conn.End == end);
            if (conn == null)
                return;
            Connectors.Remove(conn);
            start.Children.Remove(conn);
            end.Parents.Remove(conn);
            conn.OnDestroy();
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

        public delegate void GraphAddVariableDelegate(Graph graph, Variable variable);
        public event GraphAddVariableDelegate? GraphAddVariableEvent;


        public Group? CreateGroup(List<Node> nodes)
        {
            var group = new Group("--");
            group.Nodes.AddRange(nodes);
            return group;
        }

        public static Graph EmptyGraph { get; set; }

        public virtual Graph Clone()
        {
            var graph = new Graph(Name) {Path = Path, Project = Project, Type = Type};
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
    }
}
