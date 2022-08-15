using System;
using System.Collections.Generic;
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
        }

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
            var con = new Connector() { Start = start, End = end, ConnType = connType };
            Connectors.Add(con);
            return con;
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
