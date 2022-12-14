using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.AST;
using FlowChart.Misc;

namespace FlowChart.Core
{
    public class Node
    {
        static Node()
        {
            DefaultNode = new TextNode() {Text = ""};
        }

        public Node()
        {
            Parents = new List<Connector>();
            Children = new List<Connector>();
        }
        public Guid Uid { get; set; }
        public int Id { get; set; } // be careful, only used for display
        public Graph OwnerGraph { get; set; }
        public Group? OwnerGroup { get; set; }
        private string? _description;
        public string? Description
        {
            get => _description;
            set
            {
                if (_description == value)
                    return;
                _description = value;
                DescriptionChangedEvent?.Invoke(_description);
            }
        }
        public event Action<string?> DescriptionChangedEvent;

        #region REF PROPERTYIES

        public List<Connector> Parents { get; set; }
        public List<Connector> Children { get; set; }

        #endregion

        #region EVENTS
        public delegate void OnParseDelegate(Node sender, ParseResult pr);
        public event OnParseDelegate ParseEvent;

        public void OnParse(ParseResult pr)
        {
            Content = pr.Content;
            if(pr.IsError)
                OutputMessage.Inst?.Output(pr.ErrorMessage, OutputMessageType.Error, this, OwnerGraph);
            if (pr.Content.Type == GenerateContent.ContentType.CONTROL)
            {
                var contents = pr.Content.Contents;
                if (contents.Count > 0 && contents[0] == "waitall")
                {
                    Parents.ForEach(conn => contents.Add(conn.Start.Uid.ToString()));
                }
            }
            ParseEvent?.Invoke(this, pr);
        }
        public GenerateContent? Content { get; set; }
        #endregion

        public virtual Node Clone(Graph graph)
        {
            throw new NotImplementedException();
        }

        public static Node DefaultNode { get; set; }
    }

    public class StartNode : Node
    {
        public override Node Clone(Graph graph)
        {
            return new StartNode() {Uid = Project.GenUUID()};
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;
            if (obj is not StartNode node)
            {
                return false;
            }
            return true;
        }
    }

    public class TextNode : Node
    {
        public string Text { get; set; }
        public override Node Clone(Graph graph)
        {
            var node = new TextNode();
            node.Uid = Project.GenUUID();
            node.Text = Text;
            node.Description = Description;
            return node;
        }

        public override string ToString()
        {
            return $"`[TextNode] {Text}`";
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;
            if (obj is not TextNode node)
            {
                return false;
            }

            if (Text != node.Text)
                return false;
            return true;
        }
    }
}
