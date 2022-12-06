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
        private string? _description;
        public string? Description
        {
            get => _description;
            set
            {
                if (_description == value)
                    return;
                _description = value;
                DescriptionChangeEvent?.Invoke(_description);
            }
        }
        public event Action<string?>? DescriptionChangeEvent;
    }
}
