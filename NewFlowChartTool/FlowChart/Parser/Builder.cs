using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.AST;
using FlowChart.AST.Nodes;
using FlowChart.Common;
using FlowChart.Core;
using FlowChart.Plugin;

namespace FlowChart.Parser
{
    public interface ICodeGenerator
    {
        public ParseResult GenerateCode(ASTNode ast, ParserConfig cfg, Core.Node? node);
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
        public static Func<string, string>? PreBuildConvert;
        public void Build(Project p)
        {
            generator.P = p;
            ParserConfig cfg = new ParserConfig();
            foreach (var graphKV in p.GraphDict)
            {
                if(graphKV.Value is Graph graph)
                    BuildGraph(graph, cfg);
                p.RaiseGraphBuildEvent(graphKV.Value);
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
                    try
                    {
                        BuildNode(textNode, generator, cfg);
                    }
                    catch (Exception e)
                    {
                        Logger.ERR($"exception in parse node {g.Path}[{textNode.Id}]: {textNode.Text}");
#if DEBUG
                        throw;
#endif
                    }
                    
                }
            }
        }

        public void BuildNode(TextNode node, ICodeGenerator generator, ParserConfig cfg)
        {
            ASTNode? ast = null;
            ParseResult pr;


            var text = PreBuildConvert==null ? node.Text : PreBuildConvert(node.Text);

            ast = parser.Parse(text, cfg);
            
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
                try
                {
                    pr = generator.GenerateCode(ast, cfg, node);
                }
                catch (Exception e)
                {
                    pr = new ParseResult();
                }
                
                pr.Tokens = parser.Tokens;
                var parserError = parser.Error;
                if (parserError != null)
                {
                    if (parserError.IsWarning)
                        pr.WarningMessage = parserError.Message;
                    else
                        pr.ErrorMessage = parserError.Message;
                }
                node.OnParse(pr);
            }
        }

        public FlowChart.Type.Type? GetTextType(Graph g, string text)
        {
            var cfg = new ParserConfig() { OnlyGetType = true };

            var ast = parser.Parse(text, cfg);
            if (ast != null)
            {
                var pr = generator.GenerateCode(ast, cfg, null);
                return pr.Type as FlowChart.Type.Type;
            }

            return null;

        }
    }
}
