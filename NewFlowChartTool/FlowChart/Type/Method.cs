using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Type
{
    public class Method : Member
    {
        public Method(string name)
        : base(name)
        {
            Parameters = new List<Parameter>();
            Type = BuiltinTypes.VoidType;
        }
        public List<Parameter> Parameters { get; set; }
        public bool IsAction => Type == BuiltinTypes.VoidType;
        public bool IsCustomGen { get; set; } // generation code for this method is customized
        public bool IsAsync { get; set; }
    }
}
