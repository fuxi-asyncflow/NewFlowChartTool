﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Type;

namespace ProjectFactory
{
    public class MemoryProjectFactory : IProjectFactory
    {
        FlowChart.Type.Type CreateMonsterType()
        {
            var tp = new FlowChart.Type.Type("Monster") {};
            tp.AddMember(new Method("Say")
                { Type = BuiltinTypes.VoidType, Parameters = new List<Parameter>() { new Parameter("msg") {Type = BuiltinTypes.StringType} } });
            tp.AddMember(new Method("GetHp")
                { Type = BuiltinTypes.NumberType, Parameters = new List<Parameter>() });
            return tp;
        }

        FlowChart.Core.Graph CreateTestGraph_1()
        {
            var g = new FlowChart.Core.Graph("test_0") { Path = "MonsterAI.test_0" };
            g.AddNode(new TextNode() {Text = "Say(\"hello\")"});
            return g;
        }

        public void Create(Project project)
        {
            project.AddType(CreateMonsterType());
            project.AddGraph(CreateTestGraph_1());
        }

        public void Save(Project project)
        {
            Console.WriteLine("nothing to do when save memory project");
        }
    }
}
