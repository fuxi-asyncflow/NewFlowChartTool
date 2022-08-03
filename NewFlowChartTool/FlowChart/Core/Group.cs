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
        }

        public List<Node> Nodes { get; set; }
    }
}
