using FlowChart.AST;
using FlowChart;
using FlowChart.AST.Nodes;
using FlowChart.Core;
using Microsoft.VisualBasic.CompilerServices;

namespace FlowChart.LuaCodeGen
{
    public class NodeInfo
    {
        public string Code;
        public Type.Type Type;
    }
    public class CodeGenerator : IASTNodeVisitor<NodeInfo>
    {
        public Project P;

        public NodeInfo Visit(NumberNode node)
        {
            return new NodeInfo() { Code = node.Text, Type = Type.BuiltinTypes.NumberType };
        }

        public NodeInfo Visit(StringNode node)
        {
            return new NodeInfo() { Code = node.Text, Type = Type.BuiltinTypes.StringType };
        }

        public NodeInfo Visit(VariableNode node)
        {
            throw new NotImplementedException();
        }

        public NodeInfo Visit(BoolNode node)
        {
            return new NodeInfo() { Code = node.Text, Type = Type.BuiltinTypes.BoolType };
        }

        public NodeInfo Visit(SelfNode node)
        {
            throw new NotImplementedException();
        }

        public NodeInfo Visit(BinOpNode node)
        {
            var nis = node.ChildNodes.ConvertAll(node => node.OnVisit(this));
            //TODO handle compare operator
            var nodeInfo = new NodeInfo() { Code = $"{nis[0].Code} {node.Op} {nis[1].Code}", Type = nis[0].Type };
            return nodeInfo;
        }

        public NodeInfo Visit(ArgNode node)
        {
            return node.Expr.OnVisit(this);
        }

        public NodeInfo Visit(ArgListNode node)
        {
            var nis = node.ChildNodes.ConvertAll(node => node.OnVisit(this));
            var nodeInfo = new NodeInfo() { Code = string.Join(", ", nis.ConvertAll(ni => ni.Code)), Type = nis[0].Type };
            return nodeInfo;
        }

        public NodeInfo Visit(FuncNode node)
        {
            throw new NotImplementedException();
        }

        public NodeInfo Visit(MemberNode node)
        {
            var nis = node.ChildNodes.ConvertAll(node => node.OnVisit(this));
            var nodeInfo = new NodeInfo() { Code = string.Join(", ", nis.ConvertAll(ni => ni.Code)), Type = nis[0].Type };
            return nodeInfo;
        }

        public NodeInfo Visit(SubscriptNode node)
        {
            throw new NotImplementedException();
        }
    }
}