using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Core
{
    public class Group : Item
    {
        public Group(string name) : base(name)
        {
            Nodes = new List<Node>();
            Uid = Project.GenUUID().ToString();
        }

        public Group(string name, string uid) : base(name)
        {
            Nodes = new List<Node>();
            Uid = uid;
        }

        public string Uid { get; set; }

        public List<Node> Nodes { get; set; }
    }
}
