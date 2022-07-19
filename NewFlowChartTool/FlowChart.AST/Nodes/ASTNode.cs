using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.AST.Nodes
{
    public class ASTNode
    {
        public string Text;

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var b = (ASTNode)obj;
            return Text.Equals(b.Text);
        }
    }

    public class NumberNode : ASTNode
    {


    }
}
