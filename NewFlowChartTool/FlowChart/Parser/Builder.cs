using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.AST;
using FlowChart.AST.Nodes;
using FlowChart.Core;

namespace FlowChart.Parser
{
    public interface ICodeGenerator
    {
        public ParseResult GenerateCode(ASTNode ast, ParserConfig cfg);
        public Project P { get; set; }
        public Graph G { get; set; }
        public string Lang { get; }
    }


    public static class CodeGenFactory
    {
        static CodeGenFactory()
        {
            CodeGeneratorMap = new Dictionary<string, System.Type>();
            //CodeGeneratorMap.Add("lua", typeof(LuaCodeGenerator));
            //CodeGeneratorMap.Add("python", typeof(PyCodeGenerator));
        }

        private static Dictionary<string, System.Type> CodeGeneratorMap;

        public static void Register(string name, System.Type tp)
        {
            CodeGeneratorMap.Add(name, tp);
        }
        public static ICodeGenerator CreateCodeGenerator(Project p, Graph g)
        {
            if (CodeGeneratorMap.TryGetValue(p.Config.CodeGenerator, out var tp))
            {
                var gen = (ICodeGenerator)Activator.CreateInstance(tp);
                gen.G = g;
                gen.P = p;
                p.Config.CodeLang = gen.Lang;
                return gen;
            }
            else
            {
                throw new NotSupportedException($"no code generator named {p.Config.CodeGenerator}");
            }
        }

        public static string? GetLang(string generatorName)
        {
            if (CodeGeneratorMap.TryGetValue(generatorName, out var tp))
            {
                var gen = (ICodeGenerator)Activator.CreateInstance(tp);
                return gen.Lang;
            }

            return null;
        }
    }
    // Build = Parse + TypeCheck + CodeGen
    public class Builder
    {
        public Builder(IParser p)
        {
            parser = p;
        }

        private IParser parser;
        public void Build(Project p)
        {
            ParserConfig cfg = new ParserConfig();
            foreach (var graphKV in p.GraphDict)
            {
                if(graphKV.Value is Graph graph)
                    BuildGraph(graph, cfg);
            }
        }

        public void BuildGraph(Graph g, ParserConfig cfg)
        {
            if(cfg == null)
                cfg = new ParserConfig();
            var generator = CodeGenFactory.CreateCodeGenerator(g.Project, g);
            foreach (var node in g.Nodes)
            {
                if (node is TextNode textNode)
                {
                    BuildNode(textNode, generator, cfg);
                }
            }
        }

        public void BuildNode(TextNode node, ICodeGenerator generator, ParserConfig cfg)
        {
            ASTNode? ast = null;
            ParseResult pr;

            ast = parser.Parse(node.Text, cfg);
            
            if (ast == null)
            {
                pr = new ParseResult();
                pr.ErrorMessage = $"syntax error @{parser.Error?.Position}";
                pr.Tokens = parser.Tokens;
                node.OnParse(pr);
            }
            
            //var color = Console.ForegroundColor;
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine($"[parse]: {node.Text}");
            //Console.ForegroundColor = color;
            //Console.WriteLine($"ast: {ast}");
            else
            {
                pr = generator.GenerateCode(ast, cfg);
                pr.Tokens = parser.Tokens;
                node.OnParse(pr);
            }
        }

        public FlowChart.Type.Type? GetTextType(Graph g, string text)
        {
            var cfg = new ParserConfig() { OnlyGetType = true };

            var ast = parser.Parse(text, cfg);
            if (ast != null)
            {
                var generator = CodeGenFactory.CreateCodeGenerator(g.Project, g);
                var pr = generator.GenerateCode(ast, cfg);
                return pr.Type as FlowChart.Type.Type;
            }

            return null;

        }
    }
}
