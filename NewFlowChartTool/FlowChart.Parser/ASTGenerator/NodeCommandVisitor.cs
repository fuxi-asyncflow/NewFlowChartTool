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
            OpDict.Add("+", Operator.Add);
            OpDict.Add("-", Operator.Sub);
            OpDict.Add("*", Operator.Mul);
            OpDict.Add("/", Operator.Div);
            OpDict.Add("%", Operator.Mod);
            OpDict.Add("<", Operator.Lt);
            OpDict.Add(">", Operator.Gt);
            OpDict.Add("<=", Operator.Le);
            OpDict.Add(">=", Operator.Ge);
            OpDict.Add("!=", Operator.Ne);
            OpDict.Add("~=", Operator.Ne);
            OpDict.Add("==", Operator.Eq);
            OpDict.Add("..", Operator.Strcat);
            OpDict.Add("&&", Operator.And);
            OpDict.Add("||", Operator.Or);
            OpDict.Add("#", Operator.Sharp);
            OpDict.Add("!", Operator.Not);
            OpDict.Add("not", Operator.Not);
            OpDict.Add("^", Operator.Caret);
        }
        #region statement
        // assignment statement
        public override ASTNode VisitStat_assign([NotNull] NodeParserParser.Stat_assignContext context)
        {
            return base.VisitStat_assign(context);
        }

        public override ASTNode VisitStat_expr([NotNull] NodeParserParser.Stat_exprContext context)
        {
            //return base.VisitStat_expr(context);
            return Visit(context.expr());
        }
        #endregion

        #region atom expr
        public override ASTNode VisitAtom_number(NodeParserParser.Atom_numberContext context)
        {
            return new NumberNode() { Text = context.NUMBER().GetText() };
        }

        public override ASTNode VisitAtom_string(NodeParserParser.Atom_stringContext context)
        {
            return new StringNode() { Text = context.STRING().GetText() };
        }

        public override ASTNode VisitAtom_variable(NodeParserParser.Atom_variableContext context)
        {
            return new VariableNode() { Text = context.VARIABLE().GetText().Substring(1) };
        }

        public override ASTNode VisitAtom_true(NodeParserParser.Atom_trueContext context)
        {
            return new BoolNode() { Text = context.TRUE().GetText() };
        }

        public override ASTNode VisitAtom_false(NodeParserParser.Atom_falseContext context)
        {
            return new BoolNode() { Text = context.FALSE().GetText() };
        }

        public override ASTNode VisitAtom_nil(NodeParserParser.Atom_nilContext context)
        {
            return new NullNode() { Text = context.NIL().GetText() };
        }

        public override ASTNode VisitAtom_self(NodeParserParser.Atom_selfContext context)
        {
            return new SelfNode() { Text = context.SELF().GetText() };
        }

        public override ASTNode VisitExpr_name(NodeParserParser.Expr_nameContext context)
        {
            return new NameNode { Text = context.NAME().GetText() };
        }

        #endregion

        #region operator

        public override ASTNode VisitExpr_unary(NodeParserParser.Expr_unaryContext context)
        {
            var node = new UnaryOpNode() { Op = Str2Op(context.operatorUnary().GetText()) };
            node.Exp = Visit(context.expr());
            return node;
        }

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

        public override ASTNode VisitExpr_compare(NodeParserParser.Expr_compareContext context)
        {
            var node = new BinOpNode() { Op = Str2Op(context.operatorComparison().GetText()) };
            node.Add(Visit(context.expr(0)));
            node.Add(Visit(context.expr(1)));
            return node;
        }

        public override ASTNode VisitExpr_strcat(NodeParserParser.Expr_strcatContext context)
        {
            var node = new BinOpNode() { Op = Str2Op(context.operatorStrcat().GetText()) };
            node.Add(Visit(context.expr(0)));
            node.Add(Visit(context.expr(1)));
            return node;
        }

        public override ASTNode VisitExpr_and(NodeParserParser.Expr_andContext context)
        {
            var node = new BinOpNode() { Op =  Operator.And };
            node.Add(Visit(context.expr(0)));
            node.Add(Visit(context.expr(1)));
            return node;
        }

        public override ASTNode VisitExpr_or(NodeParserParser.Expr_orContext context)
        {
            var node = new BinOpNode() { Op = Operator.Or };
            node.Add(Visit(context.expr(0)));
            node.Add(Visit(context.expr(1)));
            return node;
        }

        #endregion

        #region function

        public override ASTNode VisitExpr_arg(NodeParserParser.Expr_argContext context)
        {
            var exprNode = Visit(context.expr());
            var argNode = new ArgNode(false);
            argNode.Add(exprNode);
            return argNode;
        }

        public override ASTNode VisitExpr_named_arg(NodeParserParser.Expr_named_argContext context)
        {
            var exprNode = Visit(context.expr());
            var argNode = new ArgNode(true) { Name = context.NAME().GetText()};
            argNode.Add(exprNode);
            return argNode;
        }

        public override ASTNode VisitArgumentlist(NodeParserParser.ArgumentlistContext context)
        {
            var node = new ArgListNode();
            foreach (var arg in context.argument())
            {
                node.Add(Visit(arg));
            }
            return node;
        }

        public override ASTNode VisitExpr_func_no_caller(NodeParserParser.Expr_func_no_callerContext context)
        {
            var node = new FuncNode();
            node.Add(null); // Caller
            node.Add(Visit(context.argumentlist()));    // Args
            node.FuncName = context.NAME().GetText();
            return node;
        }

        public override ASTNode VisitExpr_func_with_caller(NodeParserParser.Expr_func_with_callerContext context)
        {
            var node = new FuncNode();
            node.Add(Visit(context.expr()));
            node.Add(Visit(context.argumentlist()));    // Args
            node.FuncName = context.NAME().GetText();
            return node;
        }

        #endregion

        #region member and subscript

        public override ASTNode VisitExpr_member(NodeParserParser.Expr_memberContext context)
        {
            var node = new MemberNode() {MemberName = context.NAME().GetText()};
            node.Add(Visit(context.expr()));
            return node;
        }

        public override ASTNode VisitExpr_subscript(NodeParserParser.Expr_subscriptContext context)
        {
            var node = new SubscriptNode();
            node.Add(Visit(context.expr(0)));
            node.Add(Visit(context.expr(1)));
            return node;
            
        }

        #endregion

        #region assignment

        public override ASTNode VisitExpr_var_assign(NodeParserParser.Expr_var_assignContext context)
        {
            var node = new AssignmentNode();
            node.Add(new VariableNode()
            {
                Text = context.VARIABLE().GetText().Substring(1)
            });
            node.Add(Visit(context.expr()));
            return node;
        }

        public override ASTNode VisitExpr_member_assign(NodeParserParser.Expr_member_assignContext context)
        {
            var node = new AssignmentNode();
            var memberNode = new MemberNode() { MemberName = context.NAME().GetText() };
            memberNode.Add(Visit(context.expr(0)));
            node.Add(memberNode);
            node.Add(Visit(context.expr(1)));
            return node;
        }

        public override ASTNode VisitExpr_subscript_assign(NodeParserParser.Expr_subscript_assignContext context)
        {
            var node = new AssignmentNode();
            var subNode = new SubscriptNode();
            subNode.Add(Visit(context.expr(0)));
            subNode.Add(Visit(context.expr(1)));
            node.Add(subNode);
            node.Add(Visit(context.expr(2)));
            return node;
        }

        public override ASTNode VisitExpr_container(NodeParserParser.Expr_containerContext context)
        {
            var node = new ContainerNode();
            foreach (var arg in context.container_expr().argumentlist().argument())
            {
                node.Add(Visit(arg));
            }
            return node;
        }

        public override ASTNode VisitExpr_parenthesis(NodeParserParser.Expr_parenthesisContext context)
        {
            return new ParenthesisNode()
            {
                Content = Visit(context.expr())
            };
        }

        #endregion

        private static readonly Dictionary<string, Operator> OpDict;

        public static Operator Str2Op(string st)
        {
            return OpDict.TryGetValue(st, out var op) ? op : Operator.Unkown;
        }

    }
}