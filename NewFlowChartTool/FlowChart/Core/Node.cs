using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.AST;

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

        #region EVENTS
        public delegate void OnParseDelegate(Node sender, ParseResult pr);
        public event OnParseDelegate ParseEvent;
        public void OnParse(ParseResult pr) { ParseEvent?.Invoke(this, pr); }
        #endregion


    }

    public class StartNode : Node
    {

    }

    public class TextNode : Node
    {
        public string Text { get; set; }
    }
}
