using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Type;

namespace FlowChart.Core
{
    public class Variable : Core.Item
    {
        public Variable(string name) : base(name)
        {
            Type = BuiltinTypes.UndefinedType;
        }

        public bool Initialized => Type != BuiltinTypes.UndefinedType;
        public FlowChart.Type.Type Type;
        public string DefaultValue { get; set; }
        public bool IsVariadic { get; set; }
    }
}
