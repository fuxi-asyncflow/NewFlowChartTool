using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Core
{
    public class Item
    {
        public Item(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
