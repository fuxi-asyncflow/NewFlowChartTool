using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Common;

// Enum type in this type only worked for number ID
namespace FlowChart.Type
{
    public class EnumValue
    {
        static EnumValue()
        {
            EnumValueDict = new Dictionary<long, EnumValue>();
        }

        public EnumValue(EnumType type, long value, string? description = null)
        {
            Value = value;
            Description = description;
            Type = type;
        }

        public long Value { get; set; }
        public string? Description { get; set; }
        public EnumType Type { get; set; }

        public static Dictionary<long, EnumValue> EnumValueDict;

        public static string? GetAbbr(string v)
        {
            if (long.TryParse(v, out long id))
            {
                if (EnumValueDict.TryGetValue(id, out var enumValue))
                {
                    return enumValue.Type.Abbr;
                }
            }

            return null;
        }
    }

    public class EnumType
    {
        public EnumType(string enumName)
        {
            Name = enumName;
        }

        public void AddEnumValue(string value, string? description)
        {
            if (long.TryParse(value, out long id))
            {
                if (EnumValue.EnumValueDict.ContainsKey(id))
                {
                    Logger.ERR($"duplicated enum id: {value}");
                }
                else
                {
                    var v = new EnumValue(this, id, description);
                    EnumValue.EnumValueDict.Add(id, v);
                }
            }
            else
            {
                Logger.ERR($"enum value should be integer: {value}");
            }
        }

        public string Name { get; set; }
        //public string Type { get; set; }
        public string? Abbr { get; set; }
    }
}
