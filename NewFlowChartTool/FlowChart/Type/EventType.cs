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

        public EventType(string name) : base(name)
        {
            Parameters = new List<Parameter>();
        }
    }
}
