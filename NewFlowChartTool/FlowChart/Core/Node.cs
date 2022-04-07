using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Core
{
    public class Node
    {
        public string Uid { get; set; }

    }

    public class TextNode : Node
    {
        public string Text { get; set; }
    }
}
