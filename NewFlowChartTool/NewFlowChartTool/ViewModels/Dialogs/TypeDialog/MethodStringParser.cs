using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Type;
using Sprache;

namespace NewFlowChartTool.ViewModels.TypeDialog
{
    internal class MethodResult
    {
        public MethodResult(string name, string tp)
        {
            Name = name;
            Type = tp;
        }
        public bool IsAsync { get; set; }
        public string Type;
        public string Name;
        public string? Description;
        public List<ParamResult>? Parameters;

        public Member? ToMember(Project project)
        {
            Member? member = null;
            var tp = project.GetType(Type);
            if (tp == null)
            {
                throw new Exception($"unkown type: tp");
            }

            if (Description != null)
                Description = Description.Trim();
            if (Parameters == null)
            {
                member = new Property(Name) { Description = Description, Type = tp};
            }
            else
            {
                var method = new Method(Name) { Description = Description, Type = tp};
                member = method;
                method.IsAsync = IsAsync;

                method.Parameters = Parameters.ConvertAll(p =>
                {
                    if (p.Description != null)
                        p.Description = p.Description.Trim();
                    tp = project.GetType(p.Type);
                    if (tp == null)
                    {
                        throw new Exception($"unkown type: tp");
                    }
                    var para = new Parameter(p.Name) { Description = p.Description, Type = tp, Default = p.Value};
                    return para;
                });
                if (Parameters.Count > 0 && Parameters.Last().IsParams)
                    method.IsVariadic = true;
            }
            return member;
        }
    }

    internal class ParamResult
    {
        public ParamResult(string name, string tp)
        {
            Name = name;
            Type = tp;
        }
        public string Type;
        public string Name;
        public string? Value;
        public string? Description;
        public bool IsParams;
    }

    internal static class MethodStringParser
    {
        public static readonly Parser<IEnumerable<char>> ASYNC = Parse.String("async");
        public static readonly Parser<IEnumerable<char>> PARAMS = Parse.String("params");
        public static readonly Parser<string> Identifier = Parse.Identifier(Parse.Letter, Parse.LetterOrDigit);
        public static readonly CommentParser Comment = new CommentParser();

        public static readonly Parser<string> Description = Comment.AnyComment;


        public static readonly Parser<string> Value =
            from equal in Parse.Char('=')
            from value in Identifier.Token()
            select value;

        public static readonly Parser<ParamResult> Param =
            from isParams in PARAMS.Token().Optional()
            from tp in Identifier.Token()
            from name in Identifier.Token()
            from value in Value.Optional()
            from description in Description.Optional()
            select new ParamResult(name, tp)
            {
                Value = value.IsDefined ? value.Get() : null,
                IsParams = isParams.IsDefined,
                Description = description.IsDefined ? description.Get() : null
            };

        public static readonly Parser<IEnumerable<ParamResult>> Parameters = 
            from open in Parse.Char('(')
            from parameters in Param.DelimitedBy(Parse.Char(',').Token())
            from close in Parse.Char(')')
            select parameters;

        public static readonly Parser<MethodResult> Method = 
            from description in Description.Token().Optional()
            from isAsync in ASYNC.Token().Optional()
            from tp in Identifier.Token()
            from name in Identifier
            from parameters in Parameters.Optional()
            from close in Parse.Char(';')
            select new MethodResult(name, tp)
            {
                IsAsync = isAsync.IsDefined,
                Parameters = parameters.IsDefined ? parameters.Get().ToList() : null,
                Description = description.IsDefined ? description.Get() : null
            };
    }
}
