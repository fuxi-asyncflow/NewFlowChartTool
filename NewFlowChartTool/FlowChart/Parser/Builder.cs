using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FlowChart.AST;
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
            
            var ast = parser.Parse(node.Text, cfg);
            //var color = Console.ForegroundColor;
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine($"[parse]: {node.Text}");
            //Console.ForegroundColor = color;
            //Console.WriteLine($"ast: {ast}");
            if (ast != null)
            {
                var pr = generator.GenerateCode(ast);
                pr.Tokens = parser.Tokens;
                node.OnParse(pr);
            }

        }
    }
}
