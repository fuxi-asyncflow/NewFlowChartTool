using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FlowChart.AST;
using FlowChart.Misc;
using FlowChart.Parser;
using FlowChart.Type;
using FlowChart.Common;
using XLua;

namespace FlowChart.Core
{
    public interface IProjectFactory
    {
        void Create(Project project);
        void Save(Project project);
        void Save(Graph graph, List<string> outputs, List<string> generates);
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
            BuiltinTypes.Types.ForEach(tp =>
            {
                if (tp is GenericType gt)
                    gt.Project = this;
                AddType(tp);
            });

            EnumTypeDict = new Dictionary<string, EnumType>();
        }

        public static Guid GenUUID()
        {
            return Guid.NewGuid();
        }

        #region PROPERTIES

        public ProjectConfig Config;
        public string Path { get; set; }
        public Folder Root { get; set; }
        public Dictionary<string, Type.Type> TypeDict { get; set; }
        public Dictionary<string, Type.EventType> EventDict { get; set; }
        IProjectFactory Factory { get; set; }
        public bool IsAsyncLoad { get; set; }
        public Dictionary<string, Graph> GraphDict { get; set; }
        public Builder Builder { get; set; }
        #endregion

        #region Event

        public delegate void GraphEventDelegate(Graph graph);
        public event GraphEventDelegate? AddGraphEvent;
        public event GraphEventDelegate? RemoveGraphEvent;

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

        public void SaveGraph(Graph graph, List<string> lines, List<string> genLines)
        {
            Factory?.Save(graph, lines, genLines);
        }

        public void AddType(Type.Type type)
        {
            TypeDict.Add(type.Name, type);
        }

        public void AddType(string alias, Type.Type type)
        {
            TypeDict.Add(alias, type);
        }

        public void RemoveType(string name)
        {
            if (TypeDict.ContainsKey(name))
                TypeDict.Remove(name);
        }

        public bool RenameType(string oldName, string newName)
        {
            var type = GetType(newName);
            if (type != null)
                return false;
            type = GetType(oldName);
            if (type == null)
                return false;

            if (type.IsBuiltinType)
                return false;
            type.Name = newName;
            TypeDict.Remove(oldName);
            AddType(type);
            return true;
        }

        public Type.Type? GetType(string typeName)
        {
            Type.Type? type = null;
            TypeDict.TryGetValue(typeName, out type);
            if (type == null && typeName.Contains('<'))
            {
                return AddGenericTypeInstance(typeName);
            }
            return type;
        }

        public Type.Type? AddGenericTypeInstance(string typeName)
        {
            var idx = typeName.IndexOf('<');
            var genericTypeName = typeName.Substring(0, idx);
            var genericType = GetType(genericTypeName);
            if (genericType == null)
                return null;
            if (genericType is not GenericType genType)
            {
                return null;
            }
            var tmplStr = typeName.Substring(idx + 1, typeName.Length - idx - 2);
            var tmpls = tmplStr.Split(',').ToList().ConvertAll(s => GetType(s.Trim()));
            return genType.GetInstance(tmpls);
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

            if(EventDict.Count == 0)
                EventDict.Add("None", new EventType("None") { EventId = 0 });

            if (ev.EventId < 0)
                ev.EventId = EventDict.Count;

            if (ev.EventId == EventDict.Count)
                EventDict.Add(ev.Name, ev);
            else 
            {
                OutputMessage.Inst?.Output($"add event {ev.Name} failed: event id mismatch order");
                return false;
            }
            return true;
        }

        public EventType? GetEvent(string evName)
        {
            EventDict.TryGetValue(evName, out var ev);
            return ev;
        }

        public bool RenameEvent(string oldName, string newName)
        {
            var ev = GetEvent(newName);
            if (ev != null)
                return false;
            ev = GetEvent(oldName);
            if (ev == null)
                return false;
            ev.Name = newName;
            TypeDict.Remove(oldName);
            TypeDict.Add(newName, ev);
            return true;
        }

        public void AddGraph(Graph graph)
        {
            if (GraphDict.ContainsKey(graph.Path))
            {
                var originPath = graph.Path;
                graph.Path = originPath + "_dup" + graph.GetHashCode() % 10000;
                Logger.WARN($"exist same path when add graph: {originPath} rename to {graph.Path}");
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

        public Graph? GetGraph(string path)
        {
            GraphDict.TryGetValue(path, out var graph);
            return graph;
        }

        public void Remove(Graph graph)
        {
            if (!GraphDict.ContainsKey(graph.Path))
            {
                return;
            }

            GraphDict.Remove(graph.Path);
            RemoveGraphEvent?.Invoke(graph);
        }

        public void Remove(Folder folder)
        {
            folder.Children.ForEach(item =>
            {
                if(item is Graph graph)
                    Remove(graph);
                else if (item is Folder f)
                {
                    Remove(f);
                }
            });
            //TODO trigger event
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
