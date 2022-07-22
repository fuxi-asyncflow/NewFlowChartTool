using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Type;
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
            return tp;
        }

        FlowChart.Core.Graph CreateTestGraph_1(Type tp)
        {
            var g = new FlowChart.Core.Graph("test_0") { Path = "MonsterAI.test_0", Type = tp};
            g.AddNode(new TextNode() {Text = "Say(\"hello\")"});
            g.AddNode(new TextNode() { Text = "Say(1)" });
            return g;
        }

        public void Create(Project project)
        {
            var monsterType = CreateMonsterType();
            project.AddType(monsterType);
            project.AddGraph(CreateTestGraph_1(monsterType));
        }

        public void Save(Project project)
        {
            Console.WriteLine("nothing to do when save memory project");
        }
    }
}
