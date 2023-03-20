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
        static SubGraphMethod()
        {
            SubGraphTemplate = DefaultSubGraphTemplate;
        }
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

        public static Func<string, bool, string> SubGraphTemplate;

        public static string DefaultSubGraphTemplate(string path, bool hasParameter)
        {
            if (!hasParameter)
                return $"asyncflow.call_sub(\"{path}\", $caller)";
            else
                return $"asyncflow.call_sub(\"{path}\", $caller, $params)";
        }

        public void Update()
        {
            Template = SubGraphTemplate(RelativeGraph.Path, Parameters.Count > 0);
        }
    }
}
