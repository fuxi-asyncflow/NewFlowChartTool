using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FlowChart.AST;
using FlowChart.AST.Nodes;
using FlowChart.Core;

namespace FlowChart.Parser
{
    public interface ICodeGenFactory
    {
        public ICodeGenerator CreateCodeGenerator(Project p, Graph g);
    }
    // Build = Parse + TypeCheck + CodeGen
    public class Builder
    {
        public Builder(IParser p, ICodeGenFactory f)
        {
            parser = p;
            factory = f;
        }

        private IParser parser;
        private ICodeGenFactory factory;
        public void Build(Project p)
        {
            ParserConfig cfg = new ParserConfig();
            foreach (var graphKV in p.GraphDict)
            {
                BuildGraph(graphKV.Value, cfg);
            }
        }

        public void BuildGraph(Graph g, ParserConfig cfg)
        {
            if(cfg == null)
                cfg = new ParserConfig();
            var generator = factory.CreateCodeGenerator(g.Project, g);
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
                var generator = factory.CreateCodeGenerator(g.Project, g);
                var pr = generator.GenerateCode(ast, cfg);
                return pr.Type as FlowChart.Type.Type;
            }

            return null;

        }
    }
}
