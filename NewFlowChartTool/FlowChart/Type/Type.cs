using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace FlowChart.Type
{
    public static class BuiltinTypes
    {
        static BuiltinTypes()
        {
            NumberType = new Type();
            BoolType = new Type();
            StringType = new Type();
        }

        public static Type NumberType;
        public static Type BoolType;
        public static Type StringType;

    }
    public class Type : Core.Item
    {
        public List<Type> BaseTypes;
        public List<Member> Members;
    }
}
