using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Core
{
    public class Node
    {
        public Node()
        {
            Uid = Project.GenUUID().ToString();
        }
        public string Uid { get; set; }
        public Graph OwnerGraph { get; set; }
        public string? Description { get; set; }

    }

    public class StartNode : Node
    {

    }

    public class TextNode : Node
    {
        public string Text { get; set; }
    }
}
