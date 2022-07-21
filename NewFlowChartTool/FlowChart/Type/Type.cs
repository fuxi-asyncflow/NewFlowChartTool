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
            NumberType = new Type("Number");
            BoolType = new Type("Bool");
            StringType = new Type("String");
            VoidType = new Type("Void");
        }

        public static Type NumberType;
        public static Type BoolType;
        public static Type StringType;
        public static Type VoidType;

    }
    public class Type : Core.Item
    {
        public Type(string name)
        : base(name)
        {
            MemberDict = new Dictionary<string, Member>();
        }
        public bool AddMember(Member member, bool replace = true)
        {
            if (MemberDict.ContainsKey(member.Name))
            {
                if (!replace)
                    return false;
                MemberDict[member.Name] = member;
            }
            else
            {
                MemberDict.Add(member.Name, member);
            }
            return true;
        }

        public Member? FindMember(string name)
        {
            Member? member = null;
            MemberDict.TryGetValue(name, out member);
            return member;
        }


        public List<Type> BaseTypes;
        
        public Dictionary<string, Member> MemberDict;
    }
}
