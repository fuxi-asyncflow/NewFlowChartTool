using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Tree;
using FlowChart.AST;
using FlowChart.AST.Nodes;
using FlowChart.Parser.ASTGenerator;
using FlowChart.Parser.NodeParser;

namespace FlowChart.Parser
{
    public class Parser : IParser
    {
        public List<TextToken>? Tokens { get; set; }
        public ASTNode? Parse(string text, ParserConfig cfg)
        {
            AntlrInputStream input = new AntlrInputStream(text);
            NodeParserLexer lexer = new NodeParserLexer(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            var parser = new NodeParserParser(tokens);
            parser.Interpreter.PredictionMode = PredictionMode.SLL;

            // 使用二次解析，提高解析速度
            // 参见 https://github.com/antlr/antlr4/blob/master/doc/faq/general.md#why-is-my-expression-parser-slow
            NodeParserParser.StatContext tree;
            try
            {
                tree = parser.stat();
            }
            catch (Exception ex)
            {
                tokens.Reset();
                parser.Reset();
                parser.Interpreter.PredictionMode = PredictionMode.LL;
                tree = parser.stat();
            }
            // System.Console.WriteLine(tree.ToStringTree(parser));
            Tokens = null;
            if (cfg.GetTokens)
            {
                Tokens = new List<TextToken>();
                int pos = -1;
                GetTokens(text, tree, ref pos);
#if DEBUG
                Console.WriteLine($"----- {text}");
                Tokens.ForEach(token =>
                {
                    Console.WriteLine($"token: {text.Substring(token.Start, token.End - token.Start)}");
                });
#endif
            }

            NodeParserBaseVisitor<ASTNode> visitor = new NodeCommandVisitor();
            return visitor.Visit(tree);
        }

        public TextToken.TokenType GetTokenType(int symbolType)
        {
            switch (symbolType)
            {
                case NodeParserLexer.VARIABLE:
                    return TextToken.TokenType.Variable;
                case NodeParserLexer.NUMBER:
                    return TextToken.TokenType.Number;
                case NodeParserLexer.STRING:
                    return TextToken.TokenType.String;
                case NodeParserLexer.NAME:
                    return TextToken.TokenType.Member;
                default:
                    return TextToken.TokenType.Default;
            }
            
        }

        public void GetTokens(string text, ParserRuleContext root, ref int pos)
        {
            if (root.ChildCount == 0)
                return;
            foreach (var child in root.children)
            {
                if (child is ParserRuleContext ruleContext)
                {
                    GetTokens(text, ruleContext, ref pos);
                }
                else if(child is TerminalNodeImpl term)
                {
                    var symbol = term.Symbol;
                    if (symbol.StartIndex > pos + 1)
                    {
                        Tokens.Add(new TextToken(){Start = pos+1, End = symbol.StartIndex, Type = TextToken.TokenType.Default});
                    }
                    Tokens.Add(new TextToken()
                    {
                        Start = symbol.StartIndex, End = symbol.StartIndex + symbol.Text.Length, Type = GetTokenType(symbol.Type)
                    });
                    pos = symbol.StopIndex;
                    Debug.Assert(pos <= text.Length);
                    Debug.Assert(symbol.StartIndex < text.Length);
                }
            }
        }
    }
}