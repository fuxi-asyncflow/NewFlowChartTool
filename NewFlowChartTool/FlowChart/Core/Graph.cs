using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Core
{
    public class Graph : Item
    {
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
        public string Path { get; set; }
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

        public void AddNode(Node node)
        {
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

        public void Connect(string uidStart, string uidEnd)
        {
            Connect(GetNode(uidStart), GetNode(uidEnd));
        }

        public void Connect(Node? start, Node? end)
        {
            if (start == null || end == null)
                return;
            var con = new Connector() { Start = start, End = end };
            Connectors.Add(con);
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
                Variables.Add(v);
                return v;
            }
        }

        public Group? CreateGroup(List<Node> nodes)
        {
            var group = new Group("--");
            group.Nodes.AddRange(nodes);
            return group;
        }
    }
}
