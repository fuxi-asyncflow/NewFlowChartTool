﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.AST;
using FlowChart.AST.Nodes;
using FlowChart.Type;

namespace FlowChart.LuaCodeGen
{
    public class PyCodeGenerator : CodeGenerator
    {
        public override string Lang => "python";
        public override char InvokeOperator => '.';

        public PyCodeGenerator()
        {
            Operator.Strcat.Text = "+";
            Operator.And.Text = "and";
            Operator.Or.Text = "or";

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
            if (_prefixCodes.Count > 0)
                content.Contents.AddRange(_prefixCodes);
            if (Pr.IsWait)
                content.Type = GenerateContent.ContentType.EVENT;
            if (info.Type == BuiltinTypes.VoidType)
            {
                content.Contents.Add(info.Code);
                content.Contents.Add("return True");
            }
            else
            {
                var retName = "__ret__";
                if (info.Code.StartsWith("_v"))
                    retName = info.Code;
                else
                    content.Contents.Add($"__ret__ = {info.Code}");
                if (info.Type == BuiltinTypes.NumberType)
                    content.Contents.Add($"return {retName} != 0");
                else if (info.Type == BuiltinTypes.ArrayType)
                    content.Contents.Add($"return {retName} and len({retName}) > 0");
                else
                    content.Contents.Add($"return {retName}");
            }
        }

        protected override string GenLocalVarCode(int idx, string content)
        {
            return $"_v{idx} = {content}";
        }
    }
}
