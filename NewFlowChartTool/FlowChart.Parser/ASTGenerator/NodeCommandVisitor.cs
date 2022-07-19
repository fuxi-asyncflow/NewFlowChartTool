using Antlr4.Runtime.Misc;
using FlowChart.Parser.NodeParser;
using FlowChart.AST.Nodes;
using FlowChart.Parser.Node;

namespace FlowChart.Parser.ASTGenerator
{
    public class NodeCommandVisitor : NodeParserBaseVisitor<ASTNode>
    {
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



    }
}