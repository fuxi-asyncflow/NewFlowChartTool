using System.Collections;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using FlowChart.AST.Nodes;
using FlowChart.Parser.ASTGenerator;
using FlowChart.Parser.NodeParser;

namespace Test.NodeParserTest
{
    public class CaseData
    {
        public string Text;
        public ASTNode Result;
    }
    public class SimpleNodeTestData : IEnumerable<object[]>
    {
        public SimpleNodeTestData()
        {
            Data = new List<object[]>();
            AddCase("1", new NumberNode() { Text = "1" });
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        private List<object[]> Data;

        private void AddCase(string text, ASTNode result)
        {
            Data.Add(new object[] {text, result});
        }



        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }

    public class SimpleNodeTest
    {
        [Theory]
        [ClassData(typeof(SimpleNodeTestData))]
        public void Test1(string text, ASTNode result)
        {
            Assert.Equal(Parse(text), result);
        }

        public static ASTNode Parse(string text)
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

            NodeParserBaseVisitor<ASTNode> visitor = new NodeCommandVisitor();
            return visitor.Visit(tree);
        }
    }
}