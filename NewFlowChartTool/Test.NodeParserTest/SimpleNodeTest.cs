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
            node = new BinOpNode() { Op = Operator.Add };
            node.Add(new NumberNode() {Text = "1"});
            node.Add(new NumberNode() { Text = "2" });
            Assert.Equal(Parse("1+2"), node);
            Assert.Equal(Parse("1 +2"), node);
            //Assert.NotEqual(Parse("1 + 2d"), node);
            Assert.NotEqual(Parse("1-2"), node);
            Assert.NotEqual(Parse("1.0+2"), node);

            // 1 * 2 + 3 * 4
            BinOpNode node_11 = new BinOpNode() { Op = Operator.Mul };
            node_11.Add(new NumberNode() { Text = "1" });
            node_11.Add(new NumberNode() { Text = "2" });

            BinOpNode node_12 = new BinOpNode() { Op = Operator.Mul };
            node_12.Add(new NumberNode() { Text = "3" });
            node_12.Add(new NumberNode() { Text = "4" });

            BinOpNode node_10 = new BinOpNode() { Op = Operator.Add };
            node_10.Add(node_11);
            node_10.Add(node_12);

            Assert.Equal(Parse("1 * 2 + 3 * 4"), node_10);
        }

        [Fact]
        public void TestFuncNode()
        {
            FuncNode node;
            node = new FuncNode();
            node.FuncName = "foo";
            node.Add(null);
            node.Add(new ArgListNode());

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

            //====================================================================================
            
            node = new FuncNode();
            node.FuncName = "foo";
            node.Add(new SelfNode() {Text = "self"});
            node.Add(new ArgListNode());

            // no arg
            Assert.Equal(Parse("self.foo()"), node);
            Assert.Equal(Parse("self.foo( )"), node);

            // one arg
            node.Args.Add(new ArgNode(false) { Expr = new NumberNode() { Text = "1" } });
            Assert.Equal(Parse("self.foo(1)"), node);
            Assert.Equal(Parse("self.foo( 1 )"), node);

            // two args
            node.Args.Add(new ArgNode(false) { Expr = new NumberNode() { Text = "2" } });
            Assert.Equal(Parse("self.foo(1,2)"), node);
            Assert.Equal(Parse("self.foo(1, 2)"), node);
            Assert.NotEqual(Parse("self.foo(1.2)"), node);

            // named arg
            node.Args.Add(new ArgNode(true) { Expr = new NumberNode() { Text = "3" }, Name = "arg" });
            Assert.Equal(Parse("self.foo(1,2,arg=3)"), node);
            Assert.Equal(Parse("self.foo(1,2, arg=3)"), node);
            Assert.NotEqual(Parse("self.foo(1,2,args=3)"), node);

        }

        [Fact]
        public void TestMemberNode()
        {
            MemberNode node = new MemberNode();
            node.Add(new SelfNode() {Text = "self"});
            node.MemberName = "name";

            Assert.Equal(Parse("self.name"), node);

            MemberNode node0 = new MemberNode();
            node0.Add(node);
            node0.MemberName = "foo";
            Assert.Equal(Parse("self.name.foo"), node0);

            // subscript test
            SubscriptNode node_1 = new SubscriptNode();
            node_1.Add(new SelfNode() {Text = "self"});
            node_1.Add(new StringNode() {Text = "\"foo\""});
            Assert.Equal(Parse("self[\"foo\"]"), node_1);

            SubscriptNode node_2 = new SubscriptNode();
            node_2.Add(node_1);
            node_2.Add(new StringNode() { Text = "\"bar\"" });
            Assert.Equal(Parse("self[\"foo\"][\"bar\"]"), node_2);

            // subscript and member case 1
            SubscriptNode node_11 = new SubscriptNode();
            node_11.Add(new SelfNode() { Text = "self" });
            node_11.Add(new StringNode() { Text = "\"foo\"" });

            MemberNode node_12 = new MemberNode();
            node_12.Add(node_11);
            node_12.MemberName = "bar";
            Assert.Equal(Parse("self[\"foo\"].bar"), node_12);

            // subscript and member case 2
            MemberNode node_21 = new MemberNode();
            node_21.Add(new SelfNode() { Text = "self" });
            node_21.MemberName = "foo";

            SubscriptNode node_22 = new SubscriptNode();
            node_22.Add(node_21);
            node_22.Add(new StringNode() { Text = "\"bar\"" });
            
            Assert.Equal(Parse("self.foo[\"bar\"]"), node_22);


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