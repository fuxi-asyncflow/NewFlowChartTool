using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.AST.Nodes;

namespace FlowChart.AST
{
    public class GenerateContent
    {
        public enum ContentType
        {
            ERROR = 0,
            FUNC = 1,
            TIMER = 2,
            EVENT = 3,
            CONTROL = 4
        }

        public GenerateContent()
        {
            Type = ContentType.ERROR;
            Contents = new List<object>();
        }

        public ContentType Type;
        public List<object> Contents;
    }
    public class ParseResult
    {
        public ParseResult()
        {
            Content = new GenerateContent();
        }
        public bool IsWait => !string.IsNullOrEmpty(EventName);
        public bool IsAction;
        public bool IsError => !string.IsNullOrEmpty(ErrorMessage);
        public string ErrorMessage { get; set; }
        public string EventName { get; set; }
        public GenerateContent Content { get; set; }
    }

    public interface ICodeGenerator
    {
        public ParseResult GenerateCode(ASTNode ast);
    }

    public interface IParser
    {
        public ASTNode? Parse(string text);
    }

    public interface IASTNodeVisitor<T>
    {
        T Visit(NumberNode node);
        T Visit(StringNode node);
        T Visit(VariableNode node);
        T Visit(BoolNode node);
        T Visit(SelfNode node);
        T Visit(NameNode node);
        T Visit(BinOpNode node);
        T Visit(ArgNode node);
        T Visit(ArgListNode node);
        T Visit(FuncNode node);
        T Visit(MemberNode node);
        T Visit(SubscriptNode node);
        T Visit(AssignmentNode node);
        T Visit(ContainerNode node);

    }


}
