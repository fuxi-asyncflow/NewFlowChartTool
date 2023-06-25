using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Common;
using FlowChart.Core;

// Enum type in this type only worked for number ID
namespace FlowChart.Type
{
    public interface IRefData
    {
        public string Content { get; }
        public string? Abbr { get; }
        public string? Description { get; }
        public object? Source { get; }
        public void Open();
    }

    public class EnumValue : IRefData
    {
        public EnumValue(EnumType type, long value, string? description = null)
        {
            Value = value;
            Description = description;
            Type = type;
        }

        public long Value { get; set; }

        #region IRefData

        public string Content => Value.ToString();
        public string? Abbr => Type.Abbr;
        public string? Description { get; set; }
        public object? Source { get; set; }
        public void Open()
        {
            
        }
        #endregion


        public EnumType Type { get; set; }
    }

    public class EnumType
    {
        public EnumType(Project p, string enumName)
        {
            Name = enumName;
            _project = p;
        }

        public void AddEnumValue(string value, string? description)
        {
            if (long.TryParse(value, out long id))
            {
                if (_project.refIdDict.ContainsKey(id))
                {
                    Logger.ERR($"duplicated enum id: {value}");
                }
                else
                {
                    var v = new EnumValue(this, id, description);
                    _project.refIdDict.Add(id, v);
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
        private readonly Project _project;
    }
}
