// See https://aka.ms/new-console-template for more information

using System;
using NLog;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using FlowChart.Parser.NodeParser;
using FlowChart.Parser.ASTGenerator;
using FlowChart.AST;
using FlowChart.AST.Nodes;
using FlowChart.Parser.ASTGenerator;

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
    internal class Program
    {
        static void Main(string[] args)
        {
            Logger.MyLogger.Info("hello");
            ParserTest();            
        }

        static void OpenProjectTest()
        {
            var p = new FlowChart.Core.Project(new ProjectFactory.TestProjectFactory());
            p.Path = @"D:\git\asyncflow_new\test\flowchart";
            p.Load();
        }

        static void ParserTest()
        {
            var text = "1";
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
            var node = visitor.Visit(tree);
            System.Console.WriteLine($"{node}");
            //System.Console.WriteLine(command);
            //node.Print(0);
            //using (var writer = new StreamWriter(new FileStream("D:\\parse.gv", FileMode.OpenOrCreate)))
            //{
            //    writer.WriteLine("digraph g {\ngraph [rankdir = \"TB\"];\nnode[shape=\"rectangle\"]");
            //    node.Dot(writer);
            //    writer.WriteLine("}");
            //}

        }
    }
}





