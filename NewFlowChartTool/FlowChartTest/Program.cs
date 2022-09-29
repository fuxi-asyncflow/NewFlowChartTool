// See https://aka.ms/new-console-template for more information

using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using FlowChart.Parser.NodeParser;
using FlowChart.Parser.ASTGenerator;
using FlowChart.AST;
using FlowChart.AST.Nodes;
using FlowChart.Core;
using FlowChart.Debug;
using FlowChart.LuaCodeGen;
using FlowChart.Parser;
using ProjectFactory;
using WebSocketSharp;
using LogLevel = NLog.LogLevel;

namespace FlowChartTest // Note: actual namespace depends on the project name.
{
    public class Logger
    {
        static Logger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "file.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole") { 
                Layout = "${longdate}|${level:uppercase=true} ${message:withexception=true}"
            };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
            MyLogger = NLog.LogManager.GetCurrentClassLogger();
        }

        public static NLog.Logger MyLogger ;
    }

    class CodeGenFactory : ICodeGenFactory
    {
        public ICodeGenerator CreateCodeGenerator(Project p, Graph g)
        {
            return new CodeGenerator() { G = g, P = p };
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Logger.MyLogger.Info("hello");
            //ConvertProject(@"F:\asyncflow\asyncflow_new\test\flowchart");
            //OpenProjectTest();
            //ParserTest();            
            //CodeGenTest();
            DebugTest();
        }

        static void OpenProjectTest()
        {
            //var p = new FlowChart.Core.Project(new ProjectFactory.TestProjectFactory());
            //var p = new FlowChart.Core.Project(new ProjectFactory.MemoryProjectFactory());
            var p = new FlowChart.Core.Project(new ProjectFactory.LegacyProjectFactory());
            p.Path = @"F:\asyncflow\asyncflow_new\test\flowchart";
            p.Load();

            var builder = new Builder(new FlowChart.Parser.Parser(), new CodeGenFactory());
            builder.Build(p);
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
            catch (Exception /*ex*/)
            {

            }

            return null;
        }


        static void ParserTest()
        {
            FuncNode node;
            node = new FuncNode();
            node.FuncName = "foo";
            node.Add(null);
            node.Add(new ArgListNode());

            var argNode = new ArgNode(false);
            argNode.Add(new NumberNode() { Text = "1" });
            node.Args.Add(argNode);

            Console.WriteLine(Equals(Parse("foo(1)"), node));
        }

        static void GenCode(ASTNode ast)
        {
            var gen = new CodeGenerator();
            var nodeInfo = ast.OnVisit(gen);
            Console.WriteLine(nodeInfo.Code);
        }

        static void CodeGenTest()
        {
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

            GenCode(node_10);
        }

        static void ConvertProject(string path)
        {
            var project = new Project(new TestProjectFactory()) { Path = path };
            project.Load();
            project.Save();
        }

        static void DebugTest()
        {
            var manager = new FlowChart.Debug.WebSocket.Manager();
            manager.BroadCast("127.0.0.1", 9000, 9003, new GetChartListMessage(){ChartName = "", ObjectName = ""});
            while (true)
            {
                Thread.Sleep(10);
            }
        }
    }
}





