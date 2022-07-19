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
using IErrorNode = Antlr4.Runtime.Tree.IErrorNode;
using ITerminalNode = Antlr4.Runtime.Tree.ITerminalNode;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="INodeParserListener"/>,
/// which can be extended to create a listener which only needs to handle a subset
/// of the available methods.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.10.1")]
[System.Diagnostics.DebuggerNonUserCode]
[System.CLSCompliant(false)]
public partial class NodeParserBaseListener : INodeParserListener {
	/// <summary>
	/// Enter a parse tree produced by the <c>stat_assign</c>
	/// labeled alternative in <see cref="NodeParserParser.stat"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterStat_assign([NotNull] NodeParserParser.Stat_assignContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>stat_assign</c>
	/// labeled alternative in <see cref="NodeParserParser.stat"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitStat_assign([NotNull] NodeParserParser.Stat_assignContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>stat_expr</c>
	/// labeled alternative in <see cref="NodeParserParser.stat"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterStat_expr([NotNull] NodeParserParser.Stat_exprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>stat_expr</c>
	/// labeled alternative in <see cref="NodeParserParser.stat"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitStat_expr([NotNull] NodeParserParser.Stat_exprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_compare</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_compare([NotNull] NodeParserParser.Expr_compareContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_compare</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_compare([NotNull] NodeParserParser.Expr_compareContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_parenthesis</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_parenthesis([NotNull] NodeParserParser.Expr_parenthesisContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_parenthesis</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_parenthesis([NotNull] NodeParserParser.Expr_parenthesisContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_container</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_container([NotNull] NodeParserParser.Expr_containerContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_container</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_container([NotNull] NodeParserParser.Expr_containerContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_atom</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_atom([NotNull] NodeParserParser.Expr_atomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_atom</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_atom([NotNull] NodeParserParser.Expr_atomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_strcat</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_strcat([NotNull] NodeParserParser.Expr_strcatContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_strcat</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_strcat([NotNull] NodeParserParser.Expr_strcatContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_func_with_caller</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_func_with_caller([NotNull] NodeParserParser.Expr_func_with_callerContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_func_with_caller</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_func_with_caller([NotNull] NodeParserParser.Expr_func_with_callerContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_mul_div</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_mul_div([NotNull] NodeParserParser.Expr_mul_divContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_mul_div</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_mul_div([NotNull] NodeParserParser.Expr_mul_divContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_subscript</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_subscript([NotNull] NodeParserParser.Expr_subscriptContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_subscript</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_subscript([NotNull] NodeParserParser.Expr_subscriptContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_member</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_member([NotNull] NodeParserParser.Expr_memberContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_member</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_member([NotNull] NodeParserParser.Expr_memberContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_or</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_or([NotNull] NodeParserParser.Expr_orContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_or</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_or([NotNull] NodeParserParser.Expr_orContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_and</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_and([NotNull] NodeParserParser.Expr_andContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_and</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_and([NotNull] NodeParserParser.Expr_andContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_bitwise</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_bitwise([NotNull] NodeParserParser.Expr_bitwiseContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_bitwise</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_bitwise([NotNull] NodeParserParser.Expr_bitwiseContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_add_sub</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_add_sub([NotNull] NodeParserParser.Expr_add_subContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_add_sub</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_add_sub([NotNull] NodeParserParser.Expr_add_subContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_unary</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_unary([NotNull] NodeParserParser.Expr_unaryContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_unary</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_unary([NotNull] NodeParserParser.Expr_unaryContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_func_no_caller</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_func_no_caller([NotNull] NodeParserParser.Expr_func_no_callerContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_func_no_caller</c>
	/// labeled alternative in <see cref="NodeParserParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_func_no_caller([NotNull] NodeParserParser.Expr_func_no_callerContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>atom_number</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAtom_number([NotNull] NodeParserParser.Atom_numberContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>atom_number</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAtom_number([NotNull] NodeParserParser.Atom_numberContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>atom_string</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAtom_string([NotNull] NodeParserParser.Atom_stringContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>atom_string</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAtom_string([NotNull] NodeParserParser.Atom_stringContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>atom_variable</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAtom_variable([NotNull] NodeParserParser.Atom_variableContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>atom_variable</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAtom_variable([NotNull] NodeParserParser.Atom_variableContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>atom_true</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAtom_true([NotNull] NodeParserParser.Atom_trueContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>atom_true</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAtom_true([NotNull] NodeParserParser.Atom_trueContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>atom_false</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAtom_false([NotNull] NodeParserParser.Atom_falseContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>atom_false</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAtom_false([NotNull] NodeParserParser.Atom_falseContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>atom_nil</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAtom_nil([NotNull] NodeParserParser.Atom_nilContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>atom_nil</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAtom_nil([NotNull] NodeParserParser.Atom_nilContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>atom_self</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAtom_self([NotNull] NodeParserParser.Atom_selfContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>atom_self</c>
	/// labeled alternative in <see cref="NodeParserParser.atom_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAtom_self([NotNull] NodeParserParser.Atom_selfContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="NodeParserParser.container_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterContainer_expr([NotNull] NodeParserParser.Container_exprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="NodeParserParser.container_expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitContainer_expr([NotNull] NodeParserParser.Container_exprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_var_assign</c>
	/// labeled alternative in <see cref="NodeParserParser.assign_stat"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_var_assign([NotNull] NodeParserParser.Expr_var_assignContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_var_assign</c>
	/// labeled alternative in <see cref="NodeParserParser.assign_stat"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_var_assign([NotNull] NodeParserParser.Expr_var_assignContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_member_assign</c>
	/// labeled alternative in <see cref="NodeParserParser.assign_stat"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_member_assign([NotNull] NodeParserParser.Expr_member_assignContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_member_assign</c>
	/// labeled alternative in <see cref="NodeParserParser.assign_stat"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_member_assign([NotNull] NodeParserParser.Expr_member_assignContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>expr_subscript_assign</c>
	/// labeled alternative in <see cref="NodeParserParser.assign_stat"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr_subscript_assign([NotNull] NodeParserParser.Expr_subscript_assignContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>expr_subscript_assign</c>
	/// labeled alternative in <see cref="NodeParserParser.assign_stat"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr_subscript_assign([NotNull] NodeParserParser.Expr_subscript_assignContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="NodeParserParser.argument"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterArgument([NotNull] NodeParserParser.ArgumentContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="NodeParserParser.argument"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitArgument([NotNull] NodeParserParser.ArgumentContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="NodeParserParser.argumentlist"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterArgumentlist([NotNull] NodeParserParser.ArgumentlistContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="NodeParserParser.argumentlist"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitArgumentlist([NotNull] NodeParserParser.ArgumentlistContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="NodeParserParser.operatorUnary"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOperatorUnary([NotNull] NodeParserParser.OperatorUnaryContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="NodeParserParser.operatorUnary"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOperatorUnary([NotNull] NodeParserParser.OperatorUnaryContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="NodeParserParser.operatorMulDivMod"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOperatorMulDivMod([NotNull] NodeParserParser.OperatorMulDivModContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="NodeParserParser.operatorMulDivMod"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOperatorMulDivMod([NotNull] NodeParserParser.OperatorMulDivModContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="NodeParserParser.operatorAddSub"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOperatorAddSub([NotNull] NodeParserParser.OperatorAddSubContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="NodeParserParser.operatorAddSub"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOperatorAddSub([NotNull] NodeParserParser.OperatorAddSubContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="NodeParserParser.operatorStrcat"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOperatorStrcat([NotNull] NodeParserParser.OperatorStrcatContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="NodeParserParser.operatorStrcat"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOperatorStrcat([NotNull] NodeParserParser.OperatorStrcatContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="NodeParserParser.operatorBitwise"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOperatorBitwise([NotNull] NodeParserParser.OperatorBitwiseContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="NodeParserParser.operatorBitwise"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOperatorBitwise([NotNull] NodeParserParser.OperatorBitwiseContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="NodeParserParser.operatorComparison"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOperatorComparison([NotNull] NodeParserParser.OperatorComparisonContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="NodeParserParser.operatorComparison"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOperatorComparison([NotNull] NodeParserParser.OperatorComparisonContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="NodeParserParser.operatorAnd"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOperatorAnd([NotNull] NodeParserParser.OperatorAndContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="NodeParserParser.operatorAnd"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOperatorAnd([NotNull] NodeParserParser.OperatorAndContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="NodeParserParser.operatorOr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOperatorOr([NotNull] NodeParserParser.OperatorOrContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="NodeParserParser.operatorOr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOperatorOr([NotNull] NodeParserParser.OperatorOrContext context) { }

	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void EnterEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void ExitEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitTerminal([NotNull] ITerminalNode node) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitErrorNode([NotNull] IErrorNode node) { }
}
} // namespace FlowChart.Parser.NodeParser
