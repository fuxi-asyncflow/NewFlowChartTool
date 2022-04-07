using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Type
{
    public class Type : Core.Item
    {
        public List<Type> BaseTypes;
        public List<Member> Members;
    }
}
