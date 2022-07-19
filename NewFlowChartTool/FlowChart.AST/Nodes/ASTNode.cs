﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.AST.Nodes
{
    public enum Operator
    {
        UNKOWN = 0,
        ADD = 1,
        SUB = 2,
        MUL = 3,
        DIV = 4,
        MOD = 5,
        LT = 6,
        GT = 7,
    }
    public class ASTNode
    {
        public List<ASTNode> ChildNodes;

        public ASTNode()
        {
            ChildNodes = new List<ASTNode>();
        }

        public void Add(ASTNode child)
        {
            ChildNodes.Add(child);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var b = (ASTNode)obj;
            if (ChildNodes.Count != b.ChildNodes.Count) return false;
            for (int i = 0; i < ChildNodes.Count; i++)
            {
                if (!ChildNodes[i].Equals(b.ChildNodes[i]))
                    return false;
            }
            return true;
        }
    }

    public class LiteralNode : ASTNode
    {
        public string Text;

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var b = (LiteralNode)obj;

            if (!Text.Equals(b.Text))
                return false;
            return base.Equals(obj);
        }
    }

    public class NumberNode : LiteralNode
    {
        
    }

    public class StringNode : LiteralNode
    {

    }

    public class BinOpNode : ASTNode
    {
        public ASTNode Left => ChildNodes[0];
        public ASTNode Right => ChildNodes[1];
        public Operator Op;

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var b = (BinOpNode)obj;
            if (!Op.Equals(b.Op))
                return false;
            return base.Equals(obj);
        }

        

    }
}
