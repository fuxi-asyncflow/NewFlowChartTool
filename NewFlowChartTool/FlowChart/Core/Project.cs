using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FlowChart.AST;
using FlowChart.Parser;
using FlowChart.Type;
using XLua;

namespace FlowChart.Core
{
    public interface IProjectFactory
    {
        void Create(Project project);
        void Save(Project project);
    }

    [LuaCallCSharp]
    public class Project
    {
        public Project(IProjectFactory factory)
        {
            Factory = factory;
            Root = new Folder("") {Project = this};
            TypeDict = new Dictionary<string, Type.Type>();
            GraphDict = new Dictionary<string, Graph>();
            EventDict = new Dictionary<string, EventType>();
            BuiltinTypes.Types.ForEach(AddType);

            EnumTypeDict = new Dictionary<string, EnumType>();
        }

        public static Guid GenUUID()
        {
            return Guid.NewGuid();
        }

        #region PROPERTIES
        public string Path { get; set; }
        public Folder Root { get; set; }
        public Dictionary<string, Type.Type> TypeDict { get; set; }
        public Dictionary<string, Type.EventType> EventDict { get; set; }
        IProjectFactory Factory { get; set; }
        public Dictionary<string, Graph> GraphDict { get; set; }
        public Builder Builder { get; set; }
        #endregion

        #region Event

        public delegate void GraphEventDelegate(Graph graph);
        public event GraphEventDelegate? AddGraphEvent;

        #endregion

        public bool Load()
        {
            Factory?.Create(this);
            return true;
        }

        public void Save()
        {
            Factory?.Save(this);
        }

        public void AddType(Type.Type type)
        {
            TypeDict.Add(type.Name, type);
        }

        public Type.Type? GetType(string typeName)
        {
            Type.Type? type = null;
            TypeDict.TryGetValue(typeName, out type);
            return type;
        }

        public Type.Type GetGlobalType()
        {
            return BuiltinTypes.GlobalType;
        }

        public bool AddEvent(Type.EventType ev)
        {
            if (EventDict.ContainsKey(ev.Name))
            {
                Console.WriteLine("event duplicated");
                return false;
            }
            EventDict.Add(ev.Name, ev);
            return true;
        }

        public EventType? GetEvent(string evName)
        {
            EventDict.TryGetValue(evName, out var ev);
            return ev;
        }

        public void AddGraph(Graph graph)
        {
            if (GraphDict.ContainsKey(graph.Path))
            {
                var originPath = graph.Path;
                graph.Path = originPath + "_dup";
                Console.WriteLine($"exist same path when add graph: {originPath} rename to {graph.Path}");
            }
            var paths = graph.Path.Split(".");
            graph.Name = paths.Last();
            var folder = Root;
            
            foreach(var path in paths.SkipLast(1))
            {
                folder = folder.GetOrCreateSubFolder(path);
                if(folder == null)
                {
                    break;
                }
                if (folder.Type == null)
                {
                    folder.Type = graph.Type;
                }
            }
            if (folder == null)
                return;
            folder.AddChild(graph);
            
            // Console.WriteLine($"graph path: {graph.Path}");
            
            GraphDict.Add(graph.Path, graph);
            graph.Project = this;

            AddGraphEvent?.Invoke(graph);
        }

        #region PARSER and BUILDER

        public void Build()
        {
            Builder.Build(this);
        }

        public void BuildGraph(Graph graph, ParserConfig cfg)
        {
            Builder.BuildGraph(graph, cfg);
        }

        #endregion

        private Dictionary<string, EnumType> EnumTypeDict { get; set; }

        public EnumType AddEnumType(string name, string type)
        {
            var enumType = new EnumType()
            {
                Name = name,
                Type = type,
            };
            EnumTypeDict.Add(name, enumType);
            return enumType;
        }
    }
}
