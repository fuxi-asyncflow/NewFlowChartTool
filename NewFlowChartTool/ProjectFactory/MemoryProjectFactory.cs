using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart;
using FlowChart.Core;
using FlowChart.Type;
using ProjectFactory.DefaultProjectFactory;
using Type = FlowChart.Type.Type;

namespace ProjectFactory
{
    public class MemoryProjectFactory : EmptyProjectFactory
    {
        FlowChart.Type.Type CreateMonsterType()
        {
            var tp = new FlowChart.Type.Type("Monster") {};
            tp.AddMember(new Method("Say")
                { Type = BuiltinTypes.VoidType, Parameters = new List<Parameter>() { new Parameter("msg") {Type = BuiltinTypes.AnyType} } });
            tp.AddMember(new Method("GetHp")
                { Type = BuiltinTypes.NumberType, Parameters = new List<Parameter>() });
            tp.AddMember(new Property("age") { Type = BuiltinTypes.NumberType });
            return tp;
        }

        void AddEvents(Project p)
        {
            p.AddEvent(new EventType("Start") { EventId = 1 });
            p.AddEvent(new EventType("Tick") { EventId = 2 });
            p.AddEvent(new EventType("Event0Arg") { EventId = 3 });

            p.AddEvent(new EventType("Event1Arg") { EventId = 4, Parameters = new List<Parameter>() {new Parameter("msg") {Type = BuiltinTypes.StringType}}});
            p.AddEvent(new EventType("Event2Arg") { EventId = 5, Parameters = new List<Parameter>()
            {
                new Parameter("msg") { Type = BuiltinTypes.StringType }
                , new Parameter("id") {Type = BuiltinTypes.NumberType}
            } });

        }

        FlowChart.Core.Graph CreateTestGraph_1(Type tp)
        {
            var g = new FlowChart.Core.Graph("test_0") { Path = "MonsterAI.test_0", Type = tp, Uid = Project.GenUUID()};
            g.AddNode(new StartNode());
            g.AddNode(new TextNode() {Text = "Say(\"hello\")", Uid = Project.GenUUID() });
            g.AddNode(new TextNode() { Text = "Say(1)", Uid = Project.GenUUID() });
            g.AddNode(new TextNode() {Text = "$a = 1", Uid = Project.GenUUID() });
            g.AddNode(new TextNode() {Text = "self.age = 1", Uid = Project.GenUUID() });
            g.AddNode(new TextNode() { Text = "$tbl = {1, 2}", Uid = Project.GenUUID() });
            for(int i=0; i<g.Nodes.Count -1 ; i++)
                g.Connect(g.Nodes[i], g.Nodes[i+1], Connector.ConnectorType.ALWAYS);
            return g;
        }

        FlowChart.Core.Graph CreateTestGraph_2(Type tp)
        {
            var g = new FlowChart.Core.Graph("big_graph") { Path = "MonsterAI.big_graph", Type = tp, Uid = Project.GenUUID() };
            var startNode = new StartNode();
            var nodes = new List<Node>();
            int rows = 3;
            int cols = 2;

            int nodeCount = rows * cols;
            g.AddNode(startNode);
            for (int i = 0; i < nodeCount; i++)
            {
                nodes.Add(new TextNode() { Text = $"Say(\"hello world abc {i}\")", Uid = Project.GenUUID() });
            }
            nodes.ForEach(g.AddNode);
            for (int i = 0; i < nodeCount; i++)
            {
                if(i % rows == 0)
                    g.Connect(startNode, nodes[i], Connector.ConnectorType.ALWAYS);
                else
                {
                    g.Connect(nodes[i-1], nodes[i], Connector.ConnectorType.ALWAYS);
                }
            }
            return g;
        }

        public override void Create(Project project)
        {
            if (project.Config == null)
            {
                project.Config = ProjectConfig.CreateDefaultConfig();
                var config = project.Config;
                var rootConfig = config.GraphRoots.Count > 0 ? config.GraphRoots[0].Clone() : new GraphRootConfig();
                rootConfig.Name = "MonsterAI";
                config.GraphRoots.Add(rootConfig);
                config.GetGraphRoot(rootConfig.Name);
            }

            var monsterType = CreateMonsterType();
            project.AddType(monsterType);
            AddEvents(project);
            project.AddGraph(CreateTestGraph_1(monsterType));
            project.AddGraph(CreateTestGraph_2(monsterType));
        }

        public IProjectFactory Clone()
        {
            return new MemoryProjectFactory();
        }
    }
}
