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
        static Variable()
        {
            Default = new Variable("new_variable")
            {
                Type = BuiltinTypes.UndefinedType,
                IsVariadic = false
            };
            
        }
        public Variable(string name) : base(name)
        {
            Type = BuiltinTypes.UndefinedType;
        }

        public bool Initialized => Type != BuiltinTypes.UndefinedType;

        FlowChart.Type.Type _type;
        public FlowChart.Type.Type Type
        {
            get => _type; 
            set  
            {
                if (_type == value) return;
                _type = value;
                VariableChangeEvent?.Invoke(this);
            }
        }
        public bool IsParameter { get; set; }
        public string? DefaultValue { get; set; }
        public bool IsVariadic { get; set; }

        public static Variable Default { get; set; }

        public Variable Clone()
        {
            var v = new Variable(Name);
            v.Description = Description;
            v.Type = Type;
            v.DefaultValue = DefaultValue;
            v.IsVariadic = IsVariadic;
            return v;
        }

        public delegate void VariableChangeDelegate(Variable v);
        public event VariableChangeDelegate? VariableChangeEvent;
    }
}
