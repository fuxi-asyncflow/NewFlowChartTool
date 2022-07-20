using System.Collections;
using System.Reflection.Metadata.Ecma335;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using FlowChart.AST.Nodes;
using FlowChart.Parser.ASTGenerator;
using FlowChart.Parser.NodeParser;

namespace Test.NodeParserTest
{
   
    public class SimpleNodeTest
    {
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
            //Assert.NotEqual(Parse("1 + 2d"), node);
            Assert.NotEqual(Parse("1-2"), node);
            Assert.NotEqual(Parse("1.0+2"), node);
        }

        [Fact]
        public void TestFuncNode()
        {
            FuncNode node;
            node = new FuncNode();
            node.FuncName = "foo";
            node.Caller = null;
            node.Args = new ArgListNode();

            // no arg
            Assert.Equal(Parse("foo()"), node);
            Assert.Equal(Parse("foo( )"), node);

            // one arg
            node.Args.Add(new ArgNode(false) { Expr = new NumberNode() { Text = "1"} });
            Assert.Equal(Parse("foo(1)"), node);
            Assert.Equal(Parse("foo( 1 )"), node);

            // two args
            node.Args.Add(new ArgNode(false) { Expr = new NumberNode() { Text = "2" } });
            Assert.Equal(Parse("foo(1,2)"), node);
            Assert.Equal(Parse("foo(1, 2)"), node);
            Assert.NotEqual(Parse("foo(1.2)"), node);

            // named arg
            node.Args.Add(new ArgNode(true) { Expr = new NumberNode() { Text = "3" }, Name = "arg"});
            Assert.Equal(Parse("foo(1,2,arg=3)"), node);
            Assert.Equal(Parse("foo(1,2, arg=3)"), node);
            Assert.NotEqual(Parse("foo(1,2,args=3)"), node);

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