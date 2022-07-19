using System.Collections;
using System.Reflection.Metadata.Ecma335;
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
        //[Theory]
        //[ClassData(typeof(SimpleNodeTestData))]
        //public void Test1(string text, ASTNode result)
        //{
        //    Assert.Equal(Parse(text), result);
        //}

        [Fact]
        public void TestNumberNode()
        {
            Assert.Equal(Parse("1"), new NumberNode(){Text = "1"});
            Assert.Equal(Parse("1.01e1"), new NumberNode() { Text = "1.01e1" });
            Assert.NotEqual(Parse("1.01e"), new NumberNode() { Text = "1.01e" });
        }

        [Fact]
        public void TestBinOpNode()
        {
            BinOpNode node;
            node = new BinOpNode() { Op = Operator.ADD };
            node.Add(new NumberNode() {Text = "1"});
            node.Add(new NumberNode() { Text = "2" });
            Assert.Equal(Parse("1+2"), node);
            Assert.Equal(Parse("1 +2"), node);
            Assert.Equal(Parse("1d + 2d"), node);
            Assert.NotEqual(Parse("1-2"), node);
            Assert.NotEqual(Parse("1.0+2"), node);
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

            try
            {
                NodeParserBaseVisitor<ASTNode> visitor = new NodeCommandVisitor();
                return visitor.Visit(tree);
            }
            catch(Exception /*ex*/)
            {
                
            }

            return null;
        }
    }
}