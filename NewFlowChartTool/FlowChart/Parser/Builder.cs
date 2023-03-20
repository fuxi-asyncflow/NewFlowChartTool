using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.AST;
using FlowChart.AST.Nodes;
using FlowChart.Core;
using FlowChart.Plugin;

namespace FlowChart.Parser
{
    public interface ICodeGenerator
    {
        public ParseResult GenerateCode(ASTNode ast, ParserConfig cfg);
        public Project P { get; set; }
        public Graph G { get; set; }
        public string Lang { get; }
    }

    // Build = Parse + TypeCheck + CodeGen
    public class Builder
    {
        public Builder(IParser p, ICodeGenerator gen)
        {
            parser = p;
            generator = gen;
        }

        private IParser parser;
        private ICodeGenerator generator;
        public void Build(Project p)
        {
            generator.P = p;
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
            generator.P = g.Project;
            generator.G = g;
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

            var text = node.Text;
            if (text.Length > 1 && text[0] == '-' && text[1] == '-')
            {
                pr = new ParseResult();
                pr.Tokens = new List<TextToken>() { new TextToken()
                {
                    Start = 0,
                    End = text.Length,
                    Type = TextToken.TokenType.Default
                }};
                node.OnParse(pr);
                return;
            }

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
                var pr = generator.GenerateCode(ast, cfg);
                return pr.Type as FlowChart.Type.Type;
            }

            return null;

        }
    }
}
