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
        }
        public List<Parameter> Parameters { get; set; }
    }
}
