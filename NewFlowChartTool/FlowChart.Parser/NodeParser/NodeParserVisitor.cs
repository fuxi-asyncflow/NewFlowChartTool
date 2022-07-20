//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.10.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from ../../NewFlowChartTool/FlowChart.Parser/g4/NodeParser.g4 by ANTLR 4.10.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace FlowChart.Parser.NodeParser {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="NodeParserParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.10.1")]
[System.CLSCompliant(false)]
public interface INodeParserVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by the <c>stat_assign</c>
	/// labeled alternative in <see cref="NodeParserParser.stat"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStat_assign([NotNull] NodeParserParser.Stat_assignContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>stat_expr</c>
	/// labeled alternative in <see cref="NodeParserParser.stat"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStat_expr([NotNull] NodeParserParser.Stat_exprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_compare</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_compare([NotNull] NodeParserParser.Expr_compareContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_parenthesis</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_parenthesis([NotNull] NodeParserParser.Expr_parenthesisContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_container</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_container([NotNull] NodeParserParser.Expr_containerContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_atom</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_atom([NotNull] NodeParserParser.Expr_atomContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_strcat</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_strcat([NotNull] NodeParserParser.Expr_strcatContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_func_with_caller</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_func_with_caller([NotNull] NodeParserParser.Expr_func_with_callerContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_mul_div</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_mul_div([NotNull] NodeParserParser.Expr_mul_divContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_subscript</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_subscript([NotNull] NodeParserParser.Expr_subscriptContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_member</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_member([NotNull] NodeParserParser.Expr_memberContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_or</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_or([NotNull] NodeParserParser.Expr_orContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_and</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_and([NotNull] NodeParserParser.Expr_andContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_bitwise</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_bitwise([NotNull] NodeParserParser.Expr_bitwiseContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_add_sub</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_add_sub([NotNull] NodeParserParser.Expr_add_subContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_unary</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_unary([NotNull] NodeParserParser.Expr_unaryContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_func_no_caller</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_func_no_caller([NotNull] NodeParserParser.Expr_func_no_callerContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>atom_number</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAtom_number([NotNull] NodeParserParser.Atom_numberContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>atom_string</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAtom_string([NotNull] NodeParserParser.Atom_stringContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>atom_variable</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAtom_variable([NotNull] NodeParserParser.Atom_variableContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>atom_true</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAtom_true([NotNull] NodeParserParser.Atom_trueContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>atom_false</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAtom_false([NotNull] NodeParserParser.Atom_falseContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>atom_nil</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAtom_nil([NotNull] NodeParserParser.Atom_nilContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>atom_self</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAtom_self([NotNull] NodeParserParser.Atom_selfContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="NodeParserParser.container_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitContainer_expr([NotNull] NodeParserParser.Container_exprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_var_assign</c>
	/// labeled alternative in <see cref="NodeParserParser.assign_stat"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_var_assign([NotNull] NodeParserParser.Expr_var_assignContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_member_assign</c>
	/// labeled alternative in <see cref="NodeParserParser.assign_stat"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_member_assign([NotNull] NodeParserParser.Expr_member_assignContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_subscript_assign</c>
	/// labeled alternative in <see cref="NodeParserParser.assign_stat"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_subscript_assign([NotNull] NodeParserParser.Expr_subscript_assignContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_arg</c>
	/// labeled alternative in <see cref="NodeParserParser.argument"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_arg([NotNull] NodeParserParser.Expr_argContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expr_named_arg</c>
	/// labeled alternative in <see cref="NodeParserParser.argument"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_named_arg([NotNull] NodeParserParser.Expr_named_argContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="NodeParserParser.argumentlist"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArgumentlist([NotNull] NodeParserParser.ArgumentlistContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="NodeParserParser.operatorUnary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorUnary([NotNull] NodeParserParser.OperatorUnaryContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="NodeParserParser.operatorMulDivMod"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorMulDivMod([NotNull] NodeParserParser.OperatorMulDivModContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="NodeParserParser.operatorAddSub"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorAddSub([NotNull] NodeParserParser.OperatorAddSubContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="NodeParserParser.operatorStrcat"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorStrcat([NotNull] NodeParserParser.OperatorStrcatContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="NodeParserParser.operatorBitwise"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorBitwise([NotNull] NodeParserParser.OperatorBitwiseContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="NodeParserParser.operatorComparison"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorComparison([NotNull] NodeParserParser.OperatorComparisonContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="NodeParserParser.operatorAnd"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorAnd([NotNull] NodeParserParser.OperatorAndContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="NodeParserParser.operatorOr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorOr([NotNull] NodeParserParser.OperatorOrContext context);
}
} // namespace FlowChart.Parser.NodeParser