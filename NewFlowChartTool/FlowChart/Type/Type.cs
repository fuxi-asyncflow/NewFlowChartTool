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

            TupleType = new TupleType("Tuple") { IsBuiltinType = true };
            Types.Add(TupleType);

            UnionType = new UnionType("Union") { IsBuiltinType = true };
            Types.Add(UnionType);

        }

        public static Type NumberType;
        public static Type BoolType;
        public static Type StringType;
        public static Type VoidType;
        public static Type AnyType;
        public static Type UndefinedType;
        public static GenericType ArrayType;
        public static Type GlobalType;
        public static GenericType TupleType;
        public static GenericType UnionType;

        public static List<Type> Types;
    }

    public class Type : Core.Item
    {
        public Type(string name)
        : base(name)
        {
            MemberDict = new SortedDictionary<string, Member>();
            CompatibleTypes = new HashSet<Type>();
            BaseTypes = new List<Type>();
            IsBuiltinType = false;
            if (name.Length >= 2)
            {
                Abbr = $"{Char.ToUpper(name[0])}{Char.ToLower(name[1])}";
            }
            else if (name.Length == 1)
            {
                Abbr = $"{Char.ToUpper(name[0])}";
            }
            else
                Abbr = string.Empty;
        }

        public override string ToString()
        {
            return Name;
        }

        public string Abbr { get; set; }

        public List<Type> BaseTypes;

        public SortedDictionary<string, Member> MemberDict;
        

        public HashSet<Type> CompatibleTypes;
        public bool IsBuiltinType { get; set; }

        public Project? Project { get; set; }

        #region extra info

        public bool HasExtraInfo => _extra != null && _extra.Count > 0;

        public SortedDictionary<string, string>? _extra; // extra information, may be used for plugins

        public string? GetExtraProp(string name)
        {
            string? v = null;
            _extra?.TryGetValue(name, out v);
            return v;
        }

        public void SetExtraProp(string name, string value)
        {
            if (_extra == null)
                _extra = new SortedDictionary<string, string>();
            _extra[name] = value;
        }

        #endregion

        public bool AddMember(Member member, bool replace = true)
        {
            var memberName = member.Name;
            if(Project != null && Project.Config.CaseInsensitive)
                memberName = memberName.ToLower();
            if (MemberDict.ContainsKey(memberName))
            {
                if (!replace)
                    return false;
                MemberDict[memberName] = member;
            }
            else
            {
                MemberDict.Add(memberName, member);
            }
            return true;
        }

        public Member? FindMember(string name, bool findInBaseType = true)
        {
            if (Project != null && Project.Config.CaseInsensitive)
                name = name.ToLower();

            if (MemberDict.TryGetValue(name, out var member))
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

        public bool AddBaseType(Type type)
        {
            if (BaseTypes.Contains(type))
                return false;
            BaseTypes.Add(type);

            var ancestorTypes = new Stack<Type>();
            var handledTypes = new HashSet<Type>();
            ancestorTypes.Push(type);
            while (ancestorTypes.Count > 0)
            {
                var topType = ancestorTypes.Pop();
                if(handledTypes.Contains(topType))
                    continue;
                handledTypes.Add(topType);
                topType.CompatibleTypes.Add(this);
                foreach (var compType in CompatibleTypes)
                {
                    topType.CompatibleTypes.Add(compType);
                }
                topType.BaseTypes.ForEach(ancestorTypes.Push);
            }

            return true;
        }

        public bool RemoveBaseType()
        {
            //TODO  remove base type and update compatibleTypes
            return true;
        }

        public bool HasBaseType => BaseTypes.Count > 0;

        public string GetBaseTypesString()
        {
            return string.Join(", ", BaseTypes.ConvertAll(tp => tp.Name));
        }

        public delegate bool AcceptDelegate (Type type);

        public AcceptDelegate? AcceptFunc;

        public virtual bool CanAccept(Type inType)
        {
            if (inType == this || inType == BuiltinTypes.AnyType)
                return true;
            if (CompatibleTypes.Contains(inType))
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
                return GetInstance(new List<Type?>() { BuiltinTypes.UndefinedType });
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

        public override bool CanAccept(Type inType)
        {
            if (inType == this)
                return true;
            if (inType is InstanceType instType)
            {
                if (instType.GenType == this)
                    return true;
            }
            return false;
        }
    }

    public class TupleType : GenericType
    {
        public TupleType(string name) : base(name)
        {
            InstanceTypes = new Dictionary<string, InstanceType>();
        }

        public Dictionary<string, InstanceType> InstanceTypes;

        public override InstanceType? GetInstance(List<Type?> tmpls)
        {
            InstanceType ret;
            var typeString = string.Join(',', tmpls.ConvertAll(tp => tp.Name));
            
            if (InstanceTypes.TryGetValue(typeString, out ret))
                return ret;
            ret = new InstanceType(Name) { GenType = this };
            ret.templateTypes.AddRange(tmpls);
            ret.Name = $"{Name}<{typeString}>";
            InstanceTypes.Add(typeString, ret);
            ret.IsBuiltinType = true;
            Project.AddType(ret);
            return ret;
        }
    }

    public class UnionType : GenericType
    {
        public UnionType(string name) : base(name)
        {
            InstanceTypes = new Dictionary<string, InstanceType>();
        }

        public Dictionary<string, InstanceType> InstanceTypes;

        public override InstanceType? GetInstance(List<Type?> tmpls)
        {
            InstanceType ret;
            var typeString = string.Join(',', tmpls.ConvertAll(tp => tp.Name));

            if (InstanceTypes.TryGetValue(typeString, out ret))
                return ret;
            ret = new InstanceType(Name) { GenType = this };
            ret.AcceptFunc = type => AcceptRule(ret, type);
            ret.templateTypes.AddRange(tmpls);
            ret.Name = $"{Name}<{typeString}>";
            InstanceTypes.Add(typeString, ret);
            ret.IsBuiltinType = true;
            Project.AddType(ret);
            return ret;
        }

        public static bool AcceptRule(InstanceType self, Type inType)
        {
            foreach (var type in self.templateTypes)
            {
                if (type.CanAccept(inType))
                    return true;
            }

            if (inType is InstanceType instType && instType.GenType is UnionType inUnionType)
            {
                foreach (var tp in instType.templateTypes)
                {
                    if (!self.CanAccept(tp))
                        return false;
                }
                return true;
            }

            return false;
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
            if(AcceptFunc != null)
                return AcceptFunc(inType);
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
