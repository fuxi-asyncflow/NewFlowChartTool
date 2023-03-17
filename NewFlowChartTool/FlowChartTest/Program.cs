// See https://aka.ms/new-console-template for more information

using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using FlowChart.Parser.NodeParser;
using FlowChart.Parser.ASTGenerator;
using FlowChart.AST;
using FlowChart.AST.Nodes;
using FlowChart.Debug;
using FlowChart.Diff;
using FlowChart.Lua;
using FlowChart.LuaCodeGen;
using FlowChart.Parser;
using FlowChart.Plugin;
using ProjectFactory;
using ProjectFactory.DefaultProjectFactory;
using WebSocketSharp;
using XLua;
using LogLevel = NLog.LogLevel;
using Project = FlowChart.Core.Project;

namespace FlowChart
{
    public class Program
    {
        public static void Initialize()
        {
            var pluginManager = PluginManager.Inst;
            pluginManager.RegisterProjectFactory<DefaultProjectFactory>("default");
            pluginManager.RegisterProjectFactory<MemoryProjectFactory>("memory");

            pluginManager.RegisterCodeGenerator<LuaCodeGenerator>("lua");
            pluginManager.RegisterCodeGenerator<PyCodeGenerator>("python");

            pluginManager.RegisterParser<DefaultParser>("default");
        }
    }
}

namespace FlowChartTest // Note: actual namespace depends on the project name.
{
    public class Logger
    {
        static Logger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "log.txt" };
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
            //ConvertProject(@"F:\asyncflow\asyncflow_new\test\flowchart");
            //OpenProjectTest();
            //ParserTest();            
            //CodeGenTest();
            //DebugTest();
            //DiffTest(@"F:\asyncflow\asyncflow_new\test\old", @"F:\asyncflow\asyncflow_new\test\flowchart");
            if (args.Length < 2)
                return;
            if (args[0] == "convert2")
            {
                ConvertProject(args[1]);
            }
            else if (args[0] == "lua")
            {
                var lua = Lua.Inst;
                var p = OpenProject(args[1]);
                
                //RunLuaFile(p, args[2]);
            }

        }

        static Project OpenProject(string path)
        {
            var p = new Project();
            p.Path = path;
            p.Load();
            //p.Build();
            return p;
        }

        static void OpenProjectTest()
        {
            //var p = new FlowChart.Core.Project(new ProjectFactory.TestProjectFactory());
            //var p = new FlowChart.Core.Project(new ProjectFactory.MemoryProjectFactory());
            var p = new FlowChart.Core.Project(new ProjectFactory.LegacyProjectFactory());
            p.Path = @"F:\asyncflow\asyncflow_new\test\flowchart";
            p.Load();
            p.Build();
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

        static void ConvertLegacyProject(string xmlPath, string outputPath)
        {
            var factory = new LegacyProjectFactory() {XmlPath = xmlPath};
            var project = new Project(factory) { Path = outputPath };
            project.Load();
            project.Save();

            //var p2 = new Project(new DefaultProjectFactory()) { Path = outputPath };
            //p2.Load();
        }

        static void ConvertProject(string path)
        {
            var factory = new TestProjectFactory();
            var project = new Project(factory) { Path = path };
            project.Load();
            project.Save();
            //var p2 = new Project(new DefaultProjectFactory()) { Path = outputPath };
            //p2.Load();
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

        static void DiffTest(string oldProjectPath, string newProjectPath)
        {
            var oldProject = new Project(new DefaultProjectFactory()) {Path = oldProjectPath};
            var newProject = new Project(new DefaultProjectFactory()) { Path = newProjectPath };
            oldProject.Load();
            newProject.Load();
            var diff = new CompareProject(oldProject, newProject);
            diff.Compare();
            diff.Print();

        }

        static void RunLuaFile(Project p, string luaFile)
        {
            var L = FlowChart.Lua.Lua.Inst;
            var str = File.ReadAllText(luaFile);
            FlowChart.Lua.Lua.DoString(str);

            var luaP = new FlowChart.Lua.Project(p);
            var mainFunc = L.GetGlobal<LuaFunction>("main");
            mainFunc.Call(luaP);
        }
    }
}





