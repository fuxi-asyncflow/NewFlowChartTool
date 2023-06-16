using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Type;
using FlowChart.Core;


namespace FlowChart.Lua
{
    public class Type
    {
        public Type(FlowChart.Type.Type t)
        {
            _type = t;
        }

        public IDictionary<string, Member> GetMembers()
        {
            var dicts = new Dictionary<string, Member>();
            foreach (var kv in _type.MemberDict)
            {
                var member = kv.Value;
                if(member is FlowChart.Type.Method method)
                    dicts.Add(kv.Key, new Method(method));
                else
                    dicts.Add(kv.Key, new Member(member));
            }
            return dicts;
        }

        public Member? GetMember(string name)
        {
            var member = _type.FindMember(name);
            return member == null ? null : new Member(member);
        }

        public Member? GetMember(string name, bool findInBase)
        {
            var member = _type.FindMember(name, findInBase);
            return member == null ? null : new Member(member);
        }

        public Method? AddMethod(string name)
        {
            if (_type.FindMember(name, false) != null)
                return null;
            var method = new FlowChart.Type.Method(name);
            _type.AddMember(method);
            return new Method(method);
        }

        public FlowChart.Type.Type Tp => _type;
        private FlowChart.Type.Type _type;

    }

    public class Member
    {
        public Member(FlowChart.Type.Member m)
        {
            _member = m;
        }

        public string? Description
        {
            get => _member.Description; set => _member.Description = value; 
        }

        public string? Template
        {
            get => _member.Template; set => _member.Template = value;
        }

        public string? Source
        {
            get => _member.Template; set => _member.Template = value;
        }

        public Type? Type { get => new Type(_member.Type); set => _member.Type = value.Tp; }

        public bool IsProperty() { return _member is Property; }
        public bool IsMethod() { return _member is FlowChart.Type.Method; }

        private FlowChart.Type.Member _member;
    }

    public class Method : Member
    {
        public Method(FlowChart.Type.Method m):base(m)
        {
            _method = m;
        }

        public List<Parameter> GetParameters()
        {
            return _method.Parameters.ConvertAll(para => new Parameter(para));
        }

        public Parameter AddParameter(string name)
        {
            var p = new FlowChart.Type.Parameter(name);
            _method.Parameters.Add(p);
            var para = new Parameter(p);
            return para;
        }

        public void SetVariadic(bool v)
        {
            _method.IsVariadic = true;
        }

        private FlowChart.Type.Method _method;
    }

    public class Parameter
    {
        public Parameter(FlowChart.Type.Parameter p)
        {
            _para = p;
        }

        public string Name
        {
            get => _para.Name; set => _para.Name = value;
        }

        public string? Description
        {
            get => _para.Description; set => _para.Description = value;
        }

        public string? Default
        {
            get => _para.Default; set => _para.Default = value;
        }

        public Type? Type { get => new Type(_para.Type); set => _para.Type = value.Tp; }

        private FlowChart.Type.Parameter _para;
    }
}
