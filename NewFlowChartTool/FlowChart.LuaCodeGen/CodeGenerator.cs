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
        public bool OnlyGetType;

        public ParseResult GenerateCode(ASTNode ast, ParserConfig cfg)
        {
            OnlyGetType = cfg.OnlyGetType;
            Pr = new ParseResult();
            try
            {
                var nodeInfo = ast.OnVisit(this);
                if (OnlyGetType)
                {
                    Pr.Type = nodeInfo.Type;
                    return Pr;
                }
                PrepareCode(nodeInfo);
                
                if (!string.IsNullOrEmpty(Pr.ErrorMessage))
                    Console.WriteLine(Pr.ErrorMessage);
                else
                {
                    Console.WriteLine(nodeInfo.Code);
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return Pr;
        }

        private void PrepareCode(NodeInfo info)
        {
            var content = Pr.Content;

            if (Pr.IsError)
            {
                content.Type = GenerateContent.ContentType.ERROR;
                content.Contents.Add(Pr.ErrorMessage);
                return;
            }

            if (content.Type == GenerateContent.ContentType.CONTROL)
                return;

            content.Type = GenerateContent.ContentType.FUNC;
            if (Pr.IsWait)
                content.Type = GenerateContent.ContentType.EVENT;
            if (info.Type == BuiltinTypes.VoidType)
            {
                content.Contents.Add(info.Code);
                content.Contents.Add("return true");
            }
            else
            {
                content.Contents.Add($"local __ret__ = {info.Code}");
                if (info.Type == BuiltinTypes.NumberType)
                    content.Contents.Add("return __ret__ ~= 0");
                else if (info.Type == BuiltinTypes.ArrayType)
                    content.Contents.Add("return (__ret__ ~= nil) and (next(__ret__) ~= nil)");
                else
                    content.Contents.Add("return __ret__");
            }
        }

        public void Error(string msg)
        {
            Pr.ErrorMessage = msg;
            throw new Exception(msg);
        }

        #region CodeGen

        public static string GenEventCode(string obj, string eventName)
        {
            return $"asyncflow.wait_event({obj}, asyncflow.EventId.{eventName})";
        }

        public static string GenGetVarCode(string varName)
        {
            return $"asyncflow.get_var(\"{varName}\")";
        }

        public static string GenSetVarCode(string varName, string expr)
        {
            return $"asyncflow.set_var(\"{varName}\", {expr})";
        }

        public static string GenEventParamCode(string evName, int paramIdx)
        {
            return $"asyncflow.get_event_param(asyncflow.EventId.{evName}, {paramIdx})";
        }

        #endregion

        #region Visitor
        public NodeInfo Visit(NumberNode node)
        {
            return new NodeInfo() { Code = node.Text, Type = Type.BuiltinTypes.NumberType };
        }

        public NodeInfo Visit(StringNode node)
        {
            return new NodeInfo() { Code = node.Text, Type = Type.BuiltinTypes.StringType };
        }

        public NodeInfo Visit(NameNode node)
        {
            var text = node.Text;
            // name node could be property
            Member? member = G.Type.FindMember(text);
            string code = "";
            if (member is Property)
            {
                code = "self.";
            }
            else
            {
                member = P.GetGlobalType().FindMember(text);
            }

            if (member is Property prop)
            {
                var propCode = prop.Template ?? prop.Name;
                return new NodeInfo() { Type = prop.Type, Code = code + propCode };
                
            }

            // event
            if (text.StartsWith("On"))
            {
                var evName = text.Substring(2);
                var ev = P.GetEvent(evName);
                if (ev != null)
                {
                    Pr.EventName = ev.Name;
                    return new NodeInfo() { Type = ev, Code = GenEventCode("self", evName) };
                }
            }
            else
            {
                var evName = text;
                var ev = P.GetEvent(evName);
                if (ev != null)
                {
                    return new NodeInfo() { Type = ev, Code = "this text should not be used" };
                }
            }

            Error($"cannot find property or EventType `{node.Text}`");
            return NodeInfo.ErrorNodeInfo;

        }

        public NodeInfo Visit(VariableNode node)
        {
            Variable? v = null;
            if (OnlyGetType)
            {
                v = G.GetVar(node.Text);
                if (v == null)
                    return new NodeInfo() { Code = GenGetVarCode(node.Text), Type = v.Type };

            }
            else
                v = G.GetOrAddVariable(node.Text);
            return new NodeInfo()
            {
                Code = GenGetVarCode(node.Text),
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
            var nis = node.ChildNodes.ConvertAll(_node => _node.OnVisit(this));
            //TODO handle compare operator
            if (node.Op == Operator.Add && nis[0].Type == BuiltinTypes.StringType)
            {
                return new NodeInfo() { Code = $"{nis[0].Code} .. {nis[1].Code}", Type = nis[0].Type };
            }
            var nodeInfo = new NodeInfo() { Code = $"{nis[0].Code} {node.Op.Text} {nis[1].Code}", Type = nis[0].Type };
            return nodeInfo;
        }

        public NodeInfo Visit(ArgNode node)
        {
            return node.Expr.OnVisit(this);
        }

        public NodeInfo Visit(ArgListNode node)
        {
            var nis = node.ChildNodes.ConvertAll(_node => _node.OnVisit(this));
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

            if (OnlyGetType)
            {
                return new NodeInfo()
                {
                    Type = method.Type
                };
            }

            // check func args
            var inputArgsNodeInfo = node.Args.ChildNodes.ConvertAll(node => node.OnVisit(this));
            var argsDef = method.Parameters;
            if (argsDef.Count == inputArgsNodeInfo.Count)
            {
                for (int i = 0; i < inputArgsNodeInfo.Count; i++)
                {
                    var defType = argsDef[i].Type;
                    if(defType == null)
                        continue;
                    if (!defType.CanAccept(inputArgsNodeInfo[i].Type))
                    {
                        Error(string.Format("function args type unmatch: arg[{0}] expect `{1}` but receive `{2}`"
                        , i, argsDef[i].Type.Name, inputArgsNodeInfo[i].Type.Name));
                    }
                }
            }

            if (method.IsCustomGen)
            {
                Pr.Content.Type = GenerateContent.ContentType.CONTROL;
                Pr.Content.Contents.Add(method.Name);
                inputArgsNodeInfo.ForEach(ni =>
                {
                    if(ni.Code.StartsWith('"'))
                        Pr.Content.Contents.Add(ni.Code.Trim('"'));
                    else
                        Pr.Content.Contents.Add(ni.Code);
                });
                return new NodeInfo() { Type = method.Type, Code = "" };
            }

            string argsString = string.Join(", ", inputArgsNodeInfo.ConvertAll(ni => ni.Code));
            
            Pr.IsAction = method.IsAction;

            var funcName = method.Template ?? method.Name;

            if (funcName.Contains('$'))
            {
                var codeStr = funcName.Replace("$caller", code.Substring(0, code.Length - 1));
                codeStr = codeStr.Replace("$params", argsString);
                return new NodeInfo()
                {
                    Type = method.Type,
                    Code = codeStr
                };

            }

            return new NodeInfo()
            {
                Type = method.Type,
                Code = $"{code}{funcName}({argsString})"
            };
        }

        public NodeInfo Visit(MemberNode node)
        {
            //var nis = node.ChildNodes.ConvertAll(node => node.OnVisit(this));
            var ownerNodeInfo = node.Owner.OnVisit(this);
            if (ownerNodeInfo.Type is EventType ev)
            {
                var idx = ev.GetParamIndex(node.MemberName);
                if(idx == -1)
                    Error($"cannot find param `{node.MemberName}` in event `{ev.Name}`");
                return new NodeInfo() { Code = GenEventParamCode(ev.Name, idx), Type = ev.Parameters[idx].Type };
            }
            var member = ownerNodeInfo.Type.FindMember(node.MemberName);
            if (member == null)
            {
                // member can be event
                if (node.MemberName.StartsWith("On"))
                {
                    var evType = P.GetEvent(node.MemberName.Substring(2));
                    if (evType != null)
                    {
                        Pr.EventName = evType.Name;
                        return new NodeInfo() { Code = GenEventCode(ownerNodeInfo.Code, evType.Name), Type = BuiltinTypes.VoidType };
                    }
                }

                Error($"cannot find method `{node.MemberName}` in type `{ownerNodeInfo.Type.Name}`");
                return null;
            }
            var nodeInfo = new NodeInfo() { Code = $"{ownerNodeInfo.Code}.{member.Name}", Type = member.Type };
            return nodeInfo;
        }

        public NodeInfo Visit(SubscriptNode node)
        {
            var ownerInfo = node.Owner.OnVisit(this);
            var keyInfo = node.Key.OnVisit(this);
            // TODO check condition
            if (ownerInfo.Type == BuiltinTypes.ArrayType )
            {
                throw new NotImplementedException("not supported generic type");
            }
            else if(ownerInfo.Type is InstanceType instType)
            {
                if (instType.GenType == BuiltinTypes.ArrayType)
                    ownerInfo.Type = instType.templateTypes.First();
                else
                {
                    throw new NotImplementedException("not supported generic type");
                }
            }
            else
            {
                Error($"{ownerInfo.Code} should be an array");
                return null;
            }

            ownerInfo.Code = $"({ownerInfo.Code})[{keyInfo.Code}]";
            return ownerInfo;
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
                    varNodeInfo.Code = GenSetVarCode(varNode.Text, exprNodeInfo.Code);
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

        public NodeInfo Visit(ContainerNode node)
        {
            // TODO handle dictionary
            var nis = node.ChildNodes.ConvertAll(n => n.OnVisit(this));
            var nodeInfo = new NodeInfo();
            if (node.ChildNodes.Count == 0)
            {
                nodeInfo.Type = BuiltinTypes.UndefinedType;
            }
            else
            {
                nodeInfo.Type = nis[0].Type;
                for (int i = 1; i < nis.Count; i++)
                {
                    if (!nodeInfo.Type.CanAccept(nis[i].Type))
                    {
                        Error(string.Format("type unmatch in list: first item is `{0}` but [{1}] item is {2}"
                            , nodeInfo.Type.Name, i, nis[i].Type.Name));
                    }
                }
            }

            nodeInfo.Type = BuiltinTypes.ArrayType.GetInstance(new List<Type.Type> { nodeInfo.Type });
            nodeInfo.Code = $"{{{string.Join(", ", nis.ConvertAll(n => n.Code))}}}";
            return nodeInfo;
        }
        #endregion
    }
}