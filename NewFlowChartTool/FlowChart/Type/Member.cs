using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Type
{
    public class Member : Core.Item
    {
        public Member(string name) : base(name)
        {
            SaveToFile = true;
            _type = BuiltinTypes.VoidType;
        }

        protected Type _type;
        public virtual Type Type { get => _type; set => _type = value; }
        public string? Template { get; set; }
        public bool SaveToFile { get; set; }
    }
}
