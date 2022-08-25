using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Type
{
    public class Parameter : Member
    {
        public Parameter(string name) : base(name)
        {
        }

        public bool IsOptional { get; set; }
        public string Default { get; set; }
    }
}
