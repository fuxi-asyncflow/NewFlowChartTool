using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Common;
using XLua;

#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

namespace FlowChart.Lua
{
    public class Lua : IDisposable
    {
        static Lua()
        {
            RequirePath = new List<string>();
            try
            {
                Inst = new Lua();
            }
            catch (Exception e)
            {
                Logger.ERR("Lua init failed " + e.Message);
            }
        }

        public Lua() 
        {
            L = new LuaEnv();
            // 设置print
            LuaAPI.lua_pushstdcallcfunction(L.L, Print);
            if (0 != LuaAPI.xlua_setglobal(L.L, "print"))
            {
                throw new Exception("call xlua_setglobal fail!");
            }

            PrintCallback = s => Logger.LOG("[LUA] " + s);

            // 设置 lpairs
            L.DoString(lpairs);

            // 设置自定义的loader供require使用
            L.AddLoader(LuaFileLoader);
            
        }

        public T GetGlobal<T>(string name)
        {
            return L.Global.Get<T>(name);
        }

        public static int GetLuaMemoryUseage()
        {
            DoString("memoryusage = collectgarbage('count')");
            var result = Inst.L.Global.Get<int>("memoryusage");
            return result;
        }

        public static void SaveLuaMemorySnapshot(string filePath)
        {
            DoString("local memory = require(\"xlua.memory\") snapstring = memory.snapshot()");
            var result = Inst.L.Global.Get<string>("snapstring");
            System.IO.File.WriteAllText(filePath, result);
        }

        public static object[] DoString(string chunk, string chunkName = "chunk", LuaTable env = null)
        {
            try
            {
                return Inst.L.DoString(chunk, chunkName, env);
            }
            catch (LuaException e)
            {
                throw new Exception("[lua error in dostring]" + "\n" + e.Message);
                return null;
            }
        }

        public static Lua Inst;
        public LuaEnv L { get; set; }

        public delegate void PrintDelegate(string str);

        public static PrintDelegate PrintCallback;

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        internal static int Print(RealStatePtr L)
        {
            try
            {
                int n = LuaAPI.lua_gettop(L);
                string s = String.Empty;

                if (0 != LuaAPI.xlua_getglobal(L, "tostring"))
                {
                    return LuaAPI.luaL_error(L, "can not get tostring in print:");
                }

                for (int i = 1; i <= n; i++)
                {
                    LuaAPI.lua_pushvalue(L, -1);  /* function to be called */
                    LuaAPI.lua_pushvalue(L, i);   /* value to print */
                    if (0 != LuaAPI.lua_pcall(L, 1, 1, 0))
                    {
                        return LuaAPI.lua_error(L);
                    }
                    s += LuaAPI.lua_tostring(L, -1);

                    if (i != n) s += "\t";

                    LuaAPI.lua_pop(L, 1);  /* pop result */
                }
                if (PrintCallback != null)
                {
                    PrintCallback.Invoke(s);
                }
                
                return 0;
            }
            catch (System.Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in print:" + e);
            }
        }

        // 设置require时的文件加载路径
        public static void AddRequirePath(string path)
        {
            RequirePath.Add(FileHelper.GetFullPath(FileHelper.GetExeFolder(), path));
        }

        public static List<string> RequirePath { get; set; }
        private static byte[] LuaFileLoader(ref string filePath)
        {
            // lua文件应为utf-8格式 
            if (filePath.EndsWith(".lua"))
                filePath = filePath.Substring(0, filePath.Length-4);

            filePath = filePath.Replace('.', '/') + ".lua";

            // 如果是相对路径
            if (FileHelper.IsRelativePath(filePath))
            {
                foreach (var requirePath in RequirePath)
                {
                    var path = requirePath;
                    if (!requirePath.EndsWith("\\") && !requirePath.EndsWith("/"))
                        path += "\\";
                    var fullPath = FileHelper.GetFullPath(path, filePath);
                    if (System.IO.File.Exists(fullPath))
                    {
                        return System.IO.File.ReadAllBytes(fullPath);
                    }
                }
            }
            else // 绝对路径
            {
                if (System.IO.File.Exists(filePath))
                {
                    return System.IO.File.ReadAllBytes(filePath);
                }
            }
            return null;
        }

        const string lpairs = @"
function list_iter_func(iter, i)    
    i = i + 1
    local v = iter:MoveNext()
    if v == true then
        return i, iter.Current
    end    
end

function lpairs(list)
    local iterator = list:GetEnumerator()    
    return list_iter_func, iterator, 0
end

function dict_iter_func(iter)        
    local v = iter:MoveNext()
    if v == true then
        return iter.Current.Key, iter.Current.Value
    end    
end

function dpairs(dict)
    local iterator = dict:GetEnumerator()    
    return dict_iter_func, iterator
end
";
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (L != null) L.Dispose();
            }
        }  
    }

    public static class MyGenConfig
    {
        [LuaCallCSharp]
        public static  List<System.Type> LuaCallCSharp = new List<System.Type>()
        {
            //typeof(List<Group>),
            //typeof(List<Group>.Enumerator),
        };
    }
}





