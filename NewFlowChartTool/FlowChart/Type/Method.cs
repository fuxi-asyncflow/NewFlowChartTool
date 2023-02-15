using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;

namespace FlowChart.Type
{
    public class Method : Member
    {
        public Method(string name)
        : base(name)
        {
            Parameters = new List<Parameter>();
        }
        public List<Parameter> Parameters { get; set; }
        public bool IsAction => Type == BuiltinTypes.VoidType;
        public bool IsCustomGen { get; set; } // generation code for this method is customized
        public bool IsAsync { get; set; }
        public bool IsVariadic { get; set; }
    }

    public class SubGraphMethod : Method
    {
        public SubGraphMethod(string name)
            : base(name)
        {

        }

        public Type? ObjectType => RelativeGraph.Type;
        public Graph? RelativeGraph { get; set; }
        public override Type Type
        {
            get => RelativeGraph.ReturnType;
        }
    }
}
