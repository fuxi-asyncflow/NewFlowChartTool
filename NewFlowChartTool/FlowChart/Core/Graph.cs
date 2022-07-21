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
        }
        public string Uid { get; set; }
        public string Path { get; set; }
        public List<Node> Nodes { get; set; }
        public Dictionary<string, Node> NodeDict { get; set; }
        public List<Connector> Connectors { get; set; }
        public void AddNode(Node node)
        {
            if (NodeDict.ContainsKey(node.Uid))
                return;
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

    }
}
