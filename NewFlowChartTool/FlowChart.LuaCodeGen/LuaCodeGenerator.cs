using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.AST;
using FlowChart.AST.Nodes;
using FlowChart.Type;

namespace FlowChart.LuaCodeGen
{
    public class LuaCodeGenerator : CodeGenerator
    {
        public override string Lang => "lua";

        public LuaCodeGenerator()
        {
            Operator.Strcat.Text = "..";

            KeyWordMap = new Dictionary<string, string>();
            KeyWordMap.Add("true", "True");
            KeyWordMap.Add("false", "False");
            KeyWordMap.Add("nil", "None");
        }
        protected override void PrepareCode(NodeInfo info)
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
    }
}
