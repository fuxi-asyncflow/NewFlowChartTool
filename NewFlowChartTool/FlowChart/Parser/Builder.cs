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
    // Build = Parse + TypeCheck + CodeGen
    public class Builder
    {
        public Builder(IParser p, ICodeGenerator g)
        {
            parser = p;
            generator = g;
        }

        private IParser parser;
        private ICodeGenerator generator;
        public void Build(Project p)
        {
            foreach (var graphKV in p.GraphDict)
            {
                BuildGraph(graphKV.Value);
            }
        }

        public void BuildGraph(Graph g)
        {
            foreach (var node in g.Nodes)
            {
                if (node is TextNode textNode)
                {
                    BuildNode(textNode);
                }
            }
        }

        public void BuildNode(TextNode node)
        {
            var ast = parser.Parse(node.Text);
            Console.WriteLine($"ast: {ast}");
        }
    }
}
