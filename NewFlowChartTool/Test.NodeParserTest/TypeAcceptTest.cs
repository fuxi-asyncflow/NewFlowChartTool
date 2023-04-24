using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Type;
using Microsoft.VisualBasic.CompilerServices;
using Type = FlowChart.Type.Type;

namespace Test.NodeParserTest
{
    public class TypeAcceptTest
    {
        static TypeAcceptTest()
        {
            Project = new Project();

            NumberType = BuiltinTypes.NumberType;
            StringType = BuiltinTypes.StringType;
            BoolType = BuiltinTypes.BoolType;
            Type_A = new Type(nameof(Type_A));
            Type_B = new Type(nameof(Type_B));
            Type_C = new Type(nameof(Type_C));
            Project.AddType(Type_A);
            Project.AddType(Type_B);
            Project.AddType(Type_C);
        }

        public static Type NumberType;
        public static Type StringType;
        public static Type BoolType;
        public static Type Type_A;
        public static Type Type_B;
        public static Type Type_C;
        public static Project Project;


        [Fact]
        public void TestUnionTypeAccept()
        {
            var unionType = BuiltinTypes.UnionType;
            var AB = unionType.GetInstance(new List<Type?>() { Type_A, Type_B });
            Assert.True(AB.CanAccept(Type_A));
            Assert.True(AB.CanAccept(Type_B));
            Assert.False(AB.CanAccept(NumberType));

            Assert.False(Type_A.CanAccept(AB));

            var ABC = unionType.GetInstance(new List<Type?>() { Type_A, Type_B, Type_C });
            Assert.True(ABC.CanAccept(AB));
            Assert.False(AB.CanAccept(ABC));

            Assert.Same(AB , Project.GetType("Union<Type_A, Type_B>"));
        }
    }
}
