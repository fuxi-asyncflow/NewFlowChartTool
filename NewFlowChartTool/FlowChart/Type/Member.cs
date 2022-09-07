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
        }
        public Type Type { get; set; }
        public string? Template { get; set; }
        public bool SaveToFile { get; set; }
    }
}
