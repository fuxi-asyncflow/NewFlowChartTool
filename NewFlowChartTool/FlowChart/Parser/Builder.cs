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
            foreach (var graphKV in p.GraphDict)
            {
                BuildGraph(graphKV.Value);
            }
        }

        public void BuildGraph(Graph g)
        {
            var generator = factory.CreateCodeGenerator(g.Project, g);
            foreach (var node in g.Nodes)
            {
                if (node is TextNode textNode)
                {
                    BuildNode(textNode, generator);
                }
            }
        }

        public void BuildNode(TextNode node, ICodeGenerator generator)
        {
            var ast = parser.Parse(node.Text);
            Console.WriteLine($"[parse]: {node.Text}");
            //Console.WriteLine($"ast: {ast}");
            if(ast != null)
                generator.GenerateCode(ast);
        }
    }
}
