using System.ComponentModel.Design;
using Antlr4.Runtime.Atn;
using FlowChart.AST;
using FlowChart;
using FlowChart.AST.Nodes;
using FlowChart.Core;
using FlowChart.Type;

namespace FlowChart.LuaCodeGen
{
    public class NodeInfo
    {
        public string Code;
        public Type.Type Type;

        public static NodeInfo ErrorNodeInfo = new NodeInfo();
    }
    public class CodeGenerator : IASTNodeVisitor<NodeInfo>, ICodeGenerator
    {
        public Project P;
        public Graph G;
        public ParseResult Pr;

        public ParseResult GenerateCode(ASTNode ast)
        {
            Pr = new ParseResult();
            var nodeInfo = ast.OnVisit(this);
            if(!string.IsNullOrEmpty(Pr.ErrorMessage))
                Console.WriteLine(Pr.ErrorMessage);
            else
            {
                Console.WriteLine(nodeInfo.Code);
            }
            return Pr;
        }

        public void Error(string msg)
        {
            Pr.ErrorMessage = msg;
            throw new Exception(msg);
        }

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
            var v = G.GetOrAddVariable(node.Text);
            return new NodeInfo()
            {
                Code = $"get_var(\"{node.Text}\")",
                Type = v.Type
            };
        }

        public NodeInfo Visit(BoolNode node)
        {
            return new NodeInfo() { Code = node.Text, Type = Type.BuiltinTypes.BoolType };
        }

        public NodeInfo Visit(SelfNode node)
        {
            return new NodeInfo() { Code = "self", Type = G.Type };
        }

        public NodeInfo Visit(BinOpNode node)
        {
            var nis = node.ChildNodes.ConvertAll(node => node.OnVisit(this));
            //TODO handle compare operator
            var nodeInfo = new NodeInfo() { Code = $"{nis[0].Code} {node.Op.Text} {nis[1].Code}", Type = nis[0].Type };
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
            Member? member = null;
            string code = "";
            if (!node.HasCaller)
            {
                member = G.Type.FindMember(node.FuncName);
                if (member is Method)
                {
                    code = "self:";
                }
                else
                {
                    member = P.GetGlobalType().FindMember(node.FuncName);
                }
            }
            else
            {
                var callerNodeInfo = node.Caller.OnVisit(this);
                member = callerNodeInfo.Type.FindMember(node.FuncName);
                code = callerNodeInfo.Code + ":";

            }

            if (member is not Method method)
            {
                Error($"cannot find method `{node.FuncName}`");
                return NodeInfo.ErrorNodeInfo;
            }

            // check func args
            var inputArgsNodeInfo = node.Args.ChildNodes.ConvertAll(node => node.OnVisit(this));
            var argsDef = method.Parameters;
            if (argsDef.Count == inputArgsNodeInfo.Count)
            {
                for (int i = 0; i < inputArgsNodeInfo.Count; i++)
                {
                    if (!argsDef[i].Type.CanAccept(inputArgsNodeInfo[i].Type))
                    {
                        Error(string.Format("function args type unmatch: arg[{0}] expect `{1}` but receive `{2}`"
                        , i, argsDef[i].Type.Name, inputArgsNodeInfo[i].Type.Name));
                    }
                }
            }
            string argsString = string.Join(", ", inputArgsNodeInfo.ConvertAll(ni => ni.Code));

            return new NodeInfo()
            {
                Type = method.Type,
                Code = $"{code}{node.FuncName}({argsString})"
            };
        }

        public NodeInfo Visit(MemberNode node)
        {
            //var nis = node.ChildNodes.ConvertAll(node => node.OnVisit(this));
            var ownerNodeInfo = node.Owner.OnVisit(this);
            var member = ownerNodeInfo.Type.FindMember(node.MemberName);
            if (member == null)
            {
                Error($"cannot find method `{node.MemberName}` in type `{ownerNodeInfo.Type.Name}`");
                return null;
            }
            var nodeInfo = new NodeInfo() { Code = $"{ownerNodeInfo.Code}.{member.Name}", Type = member.Type };
            return nodeInfo;
        }

        public NodeInfo Visit(SubscriptNode node)
        {
            throw new NotImplementedException();
        }

        public NodeInfo Visit(AssignmentNode node)
        {
            if (node.Left is VariableNode varNode)
            {
                var varNodeInfo = varNode.OnVisit(this);
                var exprNodeInfo = node.Right.OnVisit(this);
                var v = G.GetOrAddVariable(varNode.Text);
                if (v.Initialized && !varNodeInfo.Type.CanAccept(exprNodeInfo.Type))
                {
                    Error(string.Format("assignment type unmatch: left expect `{0}` but receive `{1}`"
                    , varNodeInfo.Type.Name, exprNodeInfo.Type.Name));
                }
                else
                {
                    v.Type = exprNodeInfo.Type;
                    varNodeInfo.Code = $"set_var(\"{varNode.Text}\", {exprNodeInfo.Code})";
                    return varNodeInfo;
                }
            }
            else if (node.Left is SubscriptNode subNode)
            {
                throw new NotImplementedException();
            }
            else if (node.Left is MemberNode memberNode)
            {
                var memberNodeInfo = memberNode.OnVisit(this);
                var exprNodeInfo = node.Right.OnVisit(this);
                if (memberNodeInfo.Type.CanAccept(exprNodeInfo.Type))
                {
                    memberNodeInfo.Code = $"{memberNodeInfo.Code} = {exprNodeInfo.Code}";
                    return memberNodeInfo;
                }
                else
                {
                    Error(string.Format("assignment type unmatch: left expect `{0}` but receive `{1}`"
                        , memberNodeInfo.Type.Name, exprNodeInfo.Type.Name));
                }
            }

            return null;

        }
    }
}