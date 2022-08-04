using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Type;
using ProjectFactory.DefaultProjectFactory;
using Type = FlowChart.Type.Type;

namespace ProjectFactory
{
    public class MemoryProjectFactory : IProjectFactory
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

        FlowChart.Core.Graph CreateTestGraph_1(Type tp)
        {
            var g = new FlowChart.Core.Graph("test_0") { Path = "MonsterAI.test_0", Type = tp};
            g.AddNode(new StartNode());
            g.AddNode(new TextNode() {Text = "Say(\"hello\")"});
            g.AddNode(new TextNode() { Text = "Say(1)" });
            g.AddNode(new TextNode() {Text = "$a = 1"});
            g.AddNode(new TextNode() {Text = "self.age = 1"});
            g.AddNode(new TextNode() { Text = "$tbl = {1, 2}" });
            for(int i=0; i<g.Nodes.Count -1 ; i++)
                g.Connect(g.Nodes[i], g.Nodes[i+1]);
            return g;
        }

        FlowChart.Core.Graph CreateTestGraph_2(Type tp)
        {
            var g = new FlowChart.Core.Graph("big_graph") { Path = "MonsterAI.big_graph", Type = tp };
            var startNode = new StartNode();
            var nodes = new List<Node>();
            int rows = 3;
            int cols = 2;

            int nodeCount = rows * cols;
            g.AddNode(startNode);
            for (int i = 0; i < nodeCount; i++)
            {
                nodes.Add(new TextNode() { Text = $"Say(\"hello world abc {i}\")" });
            }
            nodes.ForEach(g.AddNode);
            for (int i = 0; i < nodeCount; i++)
            {
                if(i % rows == 0)
                    g.Connect(startNode, g.Nodes[i]);
                else
                {
                    g.Connect(g.Nodes[i-1], g.Nodes[i]);
                }
            }
            return g;
        }

        public void Create(Project project)
        {
            var monsterType = CreateMonsterType();
            project.AddType(monsterType);
            project.AddGraph(CreateTestGraph_1(monsterType));
            project.AddGraph(CreateTestGraph_2(monsterType));

            Save(project);
        }

        public void Save(Project project)
        {
            //Console.WriteLine("nothing to do when save memory project");
            var saver = new Saver();
            saver.SaveProject(project);
        }
    }
}
