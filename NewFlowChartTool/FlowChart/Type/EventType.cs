using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Type
{
    public class EventType : Type
    {
        public int EventId { get; set; }
        public List<Parameter> Parameters { get; set; }

        public void AddParameter(Parameter p)
        {
            Parameters.Add(p);
            var prop = new Property(p.Name);
            prop.Type = p.Type;
            AddMember(prop);
        }

        //TODO optimization
        public int GetParamIndex(string paraName)
        {
            var count = Parameters.Count;
            for (int i = 0; i < count; i++)
            {
                if(Parameters[i].Name == paraName)
                    return i;
            }
            return -1;
        }

        public EventType(string name) : base(name)
        {
            Parameters = new List<Parameter>();
        }
    }
}
