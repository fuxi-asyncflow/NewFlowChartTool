using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using XLua;

namespace FlowChart.Lua
{
    [LuaCallCSharp]
    public class Project
    {
        public Project(Core.Project p)
        {
            _project = p;
        }

        public Type? GetType(string name)
        {
            var t = _project.GetType(name);
            if (t == null)
                return null;
            return new Type(t);
        }

        public bool RegisterCallback(string name, LuaFunction func)
        {
            if (name == "OnLoadTypes")
            {
                _project.LoadTypesEndEvent += project =>
                {
                    func.Call(new Project(_project));
                };
                return true;
            }

            return false;
        }

        private Core.Project _project;

        
    }
}
