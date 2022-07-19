using Antlr4.Runtime.Misc;
using FlowChart.Parser.NodeParser;
using FlowChart.AST.Nodes;
using FlowChart.Parser.Node;

namespace FlowChart.Parser.ASTGenerator
{
    public class NodeCommandVisitor : NodeParserBaseVisitor<ASTNode>
    {
        static NodeCommandVisitor()
        {
            OpDict = new Dictionary<string, Operator>();
            OpDict.Add("+", Operator.ADD);
            OpDict.Add("-", Operator.SUB);
            OpDict.Add("*", Operator.MUL);
            OpDict.Add("/", Operator.DIV);
        }
        #region statement
        // assignment statement
        public override ASTNode VisitStat_assign([NotNull] NodeParserParser.Stat_assignContext context)
        {
            return base.VisitStat_assign(context);
        }

        public override ASTNode VisitStat_expr([NotNull] NodeParserParser.Stat_exprContext context)
        {
            return base.VisitStat_expr(context);
        }
        #endregion

        #region atom expr

        public override ASTNode VisitAtom_number(NodeParserParser.Atom_numberContext context)
        {
            return new NumberNode() { Text = context.NUMBER().GetText() };
        }

        //public override ASTNode VisitAtom_expr(NodeParserParser.Atom_exprContext context)
        //{
        //    var child = context.children[0];
        //    return base.VisitAtom_expr(context);
        //}

        #endregion

        #region operator

        public override ASTNode VisitExpr_add_sub(NodeParserParser.Expr_add_subContext context)
        {
            var node = new BinOpNode() { Op = Str2Op(context.operatorAddSub().GetText()) };
            node.Add(Visit(context.expr(0)));
            node.Add(Visit(context.expr(1)));
            return node;
        }

        public override ASTNode VisitExpr_mul_div(NodeParserParser.Expr_mul_divContext context)
        {
            var node = new BinOpNode() { Op = Str2Op(context.operatorMulDivMod().GetText()) };
            node.Add(Visit(context.expr(0)));
            node.Add(Visit(context.expr(1)));
            return node;
        }

        #endregion

        private static readonly Dictionary<string, Operator> OpDict;

        public static Operator Str2Op(string st)
        {
            return OpDict.TryGetValue(st, out var op) ? op : Operator.UNKOWN;
        }

    }
}