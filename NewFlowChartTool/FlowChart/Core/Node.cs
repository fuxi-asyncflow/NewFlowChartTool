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
        public Group? OwnerGroup { get; set; }
        public string? Description { get; set; }

        #region EVENTS
        public delegate void OnParseDelegate(Node sender, ParseResult pr);
        public event OnParseDelegate ParseEvent;

        public void OnParse(ParseResult pr)
        {
            Content = pr.Content;
            ParseEvent?.Invoke(this, pr);
        }
        public GenerateContent? Content { get; set; }

        public virtual Node Clone(Graph graph)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class StartNode : Node
    {
        public override Node Clone(Graph graph)
        {
            return new StartNode();
        }
    }

    public class TextNode : Node
    {
        public string Text { get; set; }
        public override Node Clone(Graph graph)
        {
            var node = new TextNode();
            node.Text = Text;
            node.Description = Description;
            return node;
        }
    }
}
