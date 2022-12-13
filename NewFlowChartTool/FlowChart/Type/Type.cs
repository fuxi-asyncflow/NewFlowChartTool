using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Common;
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
            
            ArrayType = new GenericTypeOne("Array") { IsBuiltinType = true };
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
        public static GenericType ArrayType;
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

        public Member? FindMember(string name, bool findInBaseType = true)
        {
            if(MemberDict.TryGetValue(name, out var member))
            {
                return member;
            }

            if (findInBaseType)
            {
                foreach (var baseType in BaseTypes)
                {
                    member = baseType.FindMember(name);
                    if (member != null)
                        return member;
                }
            }

            return member;
        }

        public bool RenameMember(string oldName, string newName)
        {
            var member = FindMember(newName, false);
            if (member != null)
                return false;
            member = FindMember(oldName, false);
            if (member == null)
                return false;
            member.Name = newName;
            MemberDict.Remove(oldName);
            MemberDict.Add(member.Name, member);
            return true;
        }

        public bool RemoveMember(string name)
        {
            var member = FindMember(name, false);
            if (member == null)
                return false;
            MemberDict.Remove(name);
            return true;
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
        public virtual int TemplateTypeCount { get; set; }
        public Project Project { get; set; }

        public GenericType(string name) : base(name)
        {
            
            
        }

        public virtual InstanceType? GetInstance(List<Type?> tmpls)
        {
            throw new NotImplementedException();
        }

        public override bool CanAccept(Type inType)
        {
            if (inType == this)
                return true;
            if (inType is not GenericType genericType)
                return false;
            // both genericType
            //if (templateTypes.Count != genericType.templateTypes.Count)
            //{
            //    return false;
            //}

            //for (int i = 0; i < templateTypes.Count; i++)
            //{
            //    if (!templateTypes[i].CanAccept(genericType.templateTypes[i]))
            //    {
            //        return false;
            //    }
            //}

            return true;
        }

        //public GenericType GetInstanceType(string name)
        //{

        //}
        
    }

    public class GenericTypeOne : GenericType
    {
        public GenericTypeOne(string name) : base(name)
        {
            InstanceTypes = new Dictionary<Type, InstanceType>();
        }

        public override int TemplateTypeCount => 1;
        public Dictionary<Type, InstanceType> InstanceTypes;

        public override InstanceType? GetInstance(List<Type?> tmpls)
        {
            if (tmpls.Count != 1)
            {
                Logger.ERR($"GenericType `{Name}` only receive 1 template type");
                return GetInstance(new List<Type?>() {BuiltinTypes.UndefinedType});
            }

            InstanceType ret;
            var tmpl = tmpls[0];
            if (tmpl == null)
                return null;
            if (InstanceTypes.TryGetValue(tmpl, out ret))
                return ret;
            ret = new InstanceType(Name) { GenType = this };
            ret.templateTypes.AddRange(tmpls);
            ret.Name = $"{Name}<{tmpl.Name}>";
            InstanceTypes.Add(tmpl, ret);
            ret.IsBuiltinType = true;
            Project.AddType(ret);
            return ret;
        }
    }

    public class InstanceType : Type
    {
        public GenericType GenType;
        public List<Type> templateTypes;


        public InstanceType(string name) : base(name)
        {
            templateTypes = new List<Type>();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override bool CanAccept(Type inType)
        {
            if (inType == this)
                return true;
            if (inType is not InstanceType instType)
                return false;
            if (instType.GenType != GenType)
                return false;
            if (templateTypes.Count != instType.templateTypes.Count)
            {
                return false;
            }

            for (int i = 0; i < templateTypes.Count; i++)
            {
                if (!templateTypes[i].CanAccept(instType.templateTypes[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }



}
