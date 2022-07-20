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
        public virtual T OnVisit<T>(IASTNodeVisitor<T> visitor)
        {
            throw new NotImplementedException("should not visit ASTNode");
        }
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
        public override T OnVisit<T>(IASTNodeVisitor<T> visitor) { return visitor.Visit(this); }
    }

    public class StringNode : LiteralNode
    {
        public override T OnVisit<T>(IASTNodeVisitor<T> visitor) { return visitor.Visit(this); }
    }

    public class VariableNode : LiteralNode
    {
        public override T OnVisit<T>(IASTNodeVisitor<T> visitor) { return visitor.Visit(this); }
    }

    public class BoolNode : LiteralNode
    {
        public override T OnVisit<T>(IASTNodeVisitor<T> visitor) { return visitor.Visit(this); }
    }

    public class SelfNode : LiteralNode
    {
        public override T OnVisit<T>(IASTNodeVisitor<T> visitor) { return visitor.Visit(this); }
    }



    public class BinOpNode : ASTNode
    {
        public override T OnVisit<T>(IASTNodeVisitor<T> visitor) { return visitor.Visit(this); }
        public ASTNode Left => ChildNodes[0];
        public ASTNode Right => ChildNodes[1];
        public Operator Op;

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            var b = (BinOpNode)obj;
            if (!Equals(Op, b.Op))
                return false;
            if (!Equals(Left, b.Left))
                return false;
            if (!Equals(Right, b.Right))
                return false;

            return base.Equals(obj);
        }
    }

    public class ArgNode : ASTNode
    {
        public override T OnVisit<T>(IASTNodeVisitor<T> visitor) { return visitor.Visit(this); }

        public bool IsNamed;
        public ASTNode Expr;
        public string? Name;
        public ArgNode(bool isNamed)
        {
            IsNamed = isNamed;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            var b = (ArgNode)obj;
            if(IsNamed != b.IsNamed) return false;
            if(!Equals(Name, b.Name)) return false;
            if(!Equals(Expr, b.Expr)) return false;
            return true;
        }
    }

    public class ArgListNode : ASTNode
    {
        public override T OnVisit<T>(IASTNodeVisitor<T> visitor) { return visitor.Visit(this); }

        public List<ASTNode> Args => ChildNodes;

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            var b = (ArgListNode)obj;
            int count = Args.Count;
            if(count != b.Args.Count) return false;
            for (int i = 0; i < count; i++)
            {
                if (!Equals(Args[i], b.Args[i]))
                    return false;
            }
            return true;
        }
    }

    public class FuncNode : ASTNode
    {
        public override T OnVisit<T>(IASTNodeVisitor<T> visitor) { return visitor.Visit(this); }

        public ASTNode Caller => ChildNodes[0];
        public string FuncName;
        public ASTNode Args => ChildNodes[1];

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            var b = (FuncNode)obj;
            if (!Equals(Caller, b.Caller))
                return false;
            if (!Equals(FuncName, b.FuncName))
                return false;
            if (!Equals(Args, b.Args))
                return false;
            return true;
        }
    }

    public class MemberNode : ASTNode
    {
        public override T OnVisit<T>(IASTNodeVisitor<T> visitor) { return visitor.Visit(this); }
        public ASTNode Owner => ChildNodes[0];
        public string MemberName;

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            var b = (MemberNode)obj;
            if (!Equals(Owner, b.Owner))
                return false;
            if (!Equals(MemberName, b.MemberName))
                return false;
            return true;
        }
    }

    public class SubscriptNode : ASTNode
    {
        public override T OnVisit<T>(IASTNodeVisitor<T> visitor) { return visitor.Visit(this); }
        public ASTNode Owner => ChildNodes[0];
        public ASTNode Key => ChildNodes[1];

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            var b = (SubscriptNode)obj;
            if (!Equals(Owner, b.Owner))
                return false;
            if (!Equals(Key, b.Key))
                return false;
            return true;
        }
    }
}
