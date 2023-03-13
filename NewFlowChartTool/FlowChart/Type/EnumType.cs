using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Type
{
   
    public class EnumValue
    {
        public string Value { get; set; }
        public string Description { get; set; }

        public static Dictionary<int, string> EnumValueDict = new Dictionary<int, string>();
    }

    public class EnumType
    {
        public EnumType()
        {
            Values = new List<EnumValue>();
        }

        public void AddEnumValue(string value, string description)
        {
            Values.Add(new EnumValue()
            {
                Value = value,
                Description = description,
            });
            int v;
            if (int.TryParse(value, out v) && !EnumValue.EnumValueDict.ContainsKey(v))
            {
                EnumValue.EnumValueDict.Add(v, description);
            }
        }


        // 枚举类型名
        public string Name { get; set; }
        // 枚举的实际类型名
        public string Type { get; set; }
        public List<EnumValue> Values { get; private set; }
    }
}
