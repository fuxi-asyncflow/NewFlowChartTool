using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.AST.Nodes;

namespace FlowChart.AST
{
    public class ParseResult
    {
        public string ErrorMessage { get; set; }

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
        T Visit(BinOpNode node);
        T Visit(ArgNode node);
        T Visit(ArgListNode node);
        T Visit(FuncNode node);
        T Visit(MemberNode node);
        T Visit(SubscriptNode node);
    }


}
