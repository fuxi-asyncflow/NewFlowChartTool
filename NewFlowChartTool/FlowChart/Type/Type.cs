using System;
using System.Collections.Generic;
using System.Data;
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
            Types = new List<Type>();

            NumberType = new Type("Number") {IsBuiltinType = true};
            Types.Add(NumberType);

            BoolType = new Type("Bool") { IsBuiltinType = true };
            Types.Add(BoolType);

            StringType = new Type("String") { IsBuiltinType = true };
            Types.Add(StringType);

            VoidType = new Type("Void") { IsBuiltinType = true };
            Types.Add(VoidType);

            AnyType = new Type("Any") { IsBuiltinType = true };
            AnyType.AcceptFunc = type => true;
            Types.Add(AnyType);

            UndefinedType = new Type("Undefined") { IsBuiltinType = true };
            Types.Add(UndefinedType);
            
            ArrayType = new GenericType("Array") { IsBuiltinType = true };
            Types.Add(ArrayType);

            GlobalType = new Type("Global");
            Types.Add(GlobalType);

        }

        public static Type NumberType;
        public static Type BoolType;
        public static Type StringType;
        public static Type VoidType;
        public static Type AnyType;
        public static Type UndefinedType;
        public static Type ArrayType;
        public static Type GlobalType;

        public static List<Type> Types;
    }

    public class Type : Core.Item
    {
        public Type(string name)
        : base(name)
        {
            MemberDict = new SortedDictionary<string, Member>();
            CompatibleTypes = new List<Type>();
            BaseTypes = new List<Type>();
            IsBuiltinType = false;
        }

        public override string ToString()
        {
            return $"[Type] {Name}";
        }

        public List<Type> BaseTypes;

        public SortedDictionary<string, Member> MemberDict;
        

        public List<Type> CompatibleTypes;
        public bool IsBuiltinType { get; set; }

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
            if(MemberDict.TryGetValue(name, out member))
            {
                return member;
            }

            foreach (var baseType in BaseTypes)
            {
                member = baseType.FindMember(name);
                if (member != null)
                    return member;
            }

            return member;
        }

        public delegate bool AcceptDelegate (Type type);

        public AcceptDelegate? AcceptFunc;

        public virtual bool CanAccept(Type inType)
        {
            if (inType == this)
                return true;
            if (AcceptFunc != null)
                return AcceptFunc(inType);
           
            return false;
        }
    }

    public class GenericType : Type
    {
        public List<Type> templateTypes;
        public Dictionary<string, GenericType> InstanceTypes;
        public GenericType(string name) : base(name)
        {
            templateTypes = new List<Type>();
            InstanceTypes = new Dictionary<string, GenericType>();
        }

        public override bool CanAccept(Type inType)
        {
            if (inType == this)
                return true;
            if (inType is not GenericType genericType)
                return false;
            // both genericType
            if (templateTypes.Count != genericType.templateTypes.Count)
            {
                return false;
            }

            for (int i = 0; i < templateTypes.Count; i++)
            {
                if (!templateTypes[i].CanAccept(genericType.templateTypes[i]))
                {
                    return false;
                }
            }

            return true;
        }

        //public GenericType GetInstanceType(string name)
        //{

        //}
        
    }

    
}
