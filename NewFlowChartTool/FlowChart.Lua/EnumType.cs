using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Type;

namespace FlowChart.Lua
{
    public class EnumType
    {
        public EnumType(FlowChart.Type.EnumType type)
        {
            _type = type;
        }

        public void SetAbbr(string? abbr)
        {
            _type.Abbr = abbr;
        }

        public void AddValue(string value, string? description = null)
        {
            _type.AddEnumValue(value, description);
        }

        private FlowChart.Type.EnumType _type;
    }
}
