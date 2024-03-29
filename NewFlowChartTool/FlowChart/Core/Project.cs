﻿using System;
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
using FlowChart.Plugin;

namespace FlowChart.Core
{
    public interface IProjectFactory
    {
        void Create(Project project);
        void Save(Project project);
        List<string> NodesPatch(Graph graph, List<Node> nodes);    // used for generate patch
        List<string> GraphPatch(Graph graph);
        void LoadGraph(Project project, List<string> lines);
        string HotFix(List<Graph> graphs);
        string SerializeGraph(Graph graph);
        Graph? DeserializeGraph(string content);
        IProjectFactory Clone();
    }

    public class EmptyProjectFactory : IProjectFactory
    {
        public virtual void Create(Project project) { }
        public virtual void Save(Project project) { }
        public virtual List<string> NodesPatch(Graph graph, List<Node> nodes) { return new List<string>(); }
        public virtual List<string> GraphPatch(Graph graph) { return new List<string>(); }
        public virtual void LoadGraph(Project project, List<string> lines) { }
        public virtual string HotFix(List<Graph> graphs) { return string.Empty;}
        public virtual string SerializeGraph(Graph graph) { return string.Empty; }
        public virtual Graph? DeserializeGraph(string content) { return null; }
        public virtual IProjectFactory Clone() { return new EmptyProjectFactory(); }
    }

    public class Project
    {
        static Project()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public Project(IProjectFactory? factory = null)
        {
            Encoding = Encoding.UTF8;
            if(factory != null)
                Factory = factory;
            Root = new Folder("") {Project = this};
            TypeDict = new Dictionary<string, Type.Type>();
            GraphDict = new Dictionary<string, TreeItem>();
            EventDict = new Dictionary<string, EventType>();
            BuiltinTypes.Types.ForEach(tp =>
            {
                if (tp is GenericType gt)
                    gt.Project = this;
                AddType(tp);
            });
            BuiltinTypes.ArrayType.GetInstance(new List<Type.Type?>(){BuiltinTypes.AnyType});
            BuiltinTypes.ArrayType.GetInstance(new List<Type.Type?>() { BuiltinTypes.NumberType });
            BuiltinTypes.ArrayType.GetInstance(new List<Type.Type?>() { BuiltinTypes.BoolType });
            BuiltinTypes.ArrayType.GetInstance(new List<Type.Type?>() { BuiltinTypes.StringType });

            EnumTypeDict = new Dictionary<string, EnumType>();
            refIdDict = new Dictionary<long, IRefData>();
            RefStringDict = new Dictionary<string, IRefData>();

            CreateProjectEvent?.Invoke(this);
        }

        public static Guid GenUUID()
        {
            return Guid.NewGuid();
        }

        #region PROPERTIES

        public ProjectConfig? Config;
        public string Path { get; set; }
        public Folder Root { get; set; }
        public Dictionary<string, Type.Type> TypeDict { get; set; }
        public Dictionary<string, Type.EventType> EventDict { get; set; }
        public IProjectFactory Factory { get; private set; }
        public bool IsAsyncLoad { get; set; }
        public Dictionary<string, TreeItem> GraphDict { get; set; }
        public Builder Builder { get; set; }
        public string? Lang { get; set; }
        public Encoding Encoding { get; set; }
        #endregion

        #region Event
        public event Action<Graph>? AddGraphEvent;
        public event Action<TreeItem>? RemoveGraphEvent;

        public static Action<Project>? CreateProjectEvent;
        public event Action<Project>? LoadProjectConfigEvent;
        public event Action<Project>? LoadTypesEndEvent;

        public void RaiseProjectConfigEvent()
        {
            LoadProjectConfigEvent?.Invoke(this);
        }

        public void RaiseLoadTypesEndEvent()
        {
            LoadTypesEndEvent?.Invoke(this);
        }

        #endregion

        public bool Load()
        {
            var pluginManager = PluginManager.Inst;
            if (Config == null)
            {
                var configPath = Path;
                if (Directory.Exists(Path))
                {
                    configPath = System.IO.Path.Combine(Path, "project.json");
                }

                if (!File.Exists(configPath))
                {
                    Logger.ERR($"cannot find project.json at `{configPath}`, project folder is valid");
                    return false;
                }

                Config = ProjectConfig.LoadConfig(configPath);
            }

            // create builder
            SetBuilder(Config.ParserName, Config.CodeGenerator);

            // set output encoding
            if (!string.IsNullOrEmpty(Config.Encoding))
            {
                try
                {
                    if (int.TryParse(Config.Encoding, out int codePage))
                        Encoding = Encoding.GetEncoding(codePage);
                    else
                        Encoding = Encoding.GetEncoding(Config.Encoding);
                }
                catch (Exception e)
                {
                    Logger.ERR($"cannot find encoding {Config.Encoding}: {e.Message}");
                }
            }

            // create Factory
            var loaderName = Config.Loader ?? "default";
            var factory = PluginManager.Inst.GetProjectFactory(loaderName);
            if (factory == null)
            {
                Logger.ERR($"cannot find loader `{loaderName}` to open project, may be plugin is missing");
                return false;
            }

            Factory = factory;
            Factory?.Create(this);
            return true;
        }

        public void SetBuilder(string? parserName, string generatorName)
        {
            var pluginManager = PluginManager.Inst;
            parserName ??= "default";
            var parser = pluginManager.GetParser(parserName);

            var codeGen = pluginManager.GetCodeGenerator(generatorName);
            if (parser != null && codeGen != null)
            {
                Builder = new Builder(parser, codeGen);
                Lang = codeGen.Lang;
                Logger.LOG($"use parser `{parserName}`, code generator `{generatorName}` ");
            }
            else
                Logger.ERR($"create parser failed: parser `{parserName}`, code generator `{generatorName}` ");
        }

        public void Save()
        {
            Factory?.Save(this);
        }

        public void AddType(Type.Type type)
        {
            TypeDict.Add(type.Name, type);
            type.Project = this;
        }

        public void AddType(string alias, Type.Type type)
        {
            if(!TypeDict.ContainsKey(alias))
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

        public virtual Type.Type? GetType(string? typeName)
        {
            if (typeName == null)
                return null;
            Type.Type? type = null;
            TypeDict.TryGetValue(typeName, out type);
            if (type != null)
                return type;

            
            typeName = typeName.Replace(" ", "");
            TypeDict.TryGetValue(typeName, out type);
            if (type != null)
                return type;
            if (typeName.Contains('<'))
            {
                return AddGenericTypeInstance(typeName);
            }
            else if (typeName.Contains(','))
            {
                return AddTupleType(typeName);
            }

            return null;
        }

        public List<string> GetCustomTypeNames()
        {
            var types = new List<string>();
            foreach (var kv in TypeDict)
            {
                var type = kv.Value;
                if (!type.IsBuiltinType && type is not GenericType)
                {
                    types.Add(kv.Key);
                }
            }
            return types;
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
            var tmpls = tmplStr.Split(',').ToList().ConvertAll(s => GetType(s.Trim()) ?? BuiltinTypes.AnyType);
            return genType.GetInstance(tmpls);
        }

        public Type.Type? AddTupleType(string typeName)
        {
            return AddGenericTypeInstance($"Tuple<{typeName}>");
        }

        public Type.Type GetGlobalType()
        {
            return BuiltinTypes.GlobalType;
        }

        public bool AddEvent(Type.EventType ev)
        {
            var evName = ev.Name;
            if (Config.CaseInsensitive)
                evName = evName.ToLower();
            if (EventDict.ContainsKey(evName))
            {
                Console.WriteLine("event duplicated");
                return false;
            }

            if(EventDict.Count == 0)
                EventDict.Add("None", new EventType("None") { EventId = 0 });

            if (ev.EventId < 0)
                ev.EventId = EventDict.Count;

            if (ev.EventId == EventDict.Count)
                EventDict.Add(evName, ev);
            else 
            {
                OutputMessage.Inst?.Output($"add event {ev.Name} failed: event id mismatch order");
                return false;
            }
            return true;
        }

        public EventType? GetEvent(string evName)
        {
            if (Config.CaseInsensitive)
                evName = evName.ToLower();
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
            EventDict.Remove(oldName);
            EventDict.Add(newName, ev);
            return true;
        }

        public bool RemoveEvent(string evName)
        {
            var ev = GetEvent(evName);
            if (ev == null)
                return false;
            var evId = ev.EventId;
            if (evId < 3)
                return false;
            EventDict.Remove(evName);
            foreach (var e in EventDict.Values)
            {
                if (e.EventId > evId)
                    e.EventId--;
            }
            return true;
        }

        public void AddGraph(Graph graph)
        {
            if (GraphDict.ContainsKey(graph.Path))
            {
                var originPath = graph.Path;
                // graph.Path = originPath + "_dup" + graph.GetHashCode() % 10000;
                Logger.WARN($"exist same path when add graph: {originPath} rename to {graph.Path}");
                return;
            }
            var path = graph.Path;
            var paths = path.Split(".");
            graph.Name = paths.Last();

            var parent = GetFolder(path.Substring(0, path.LastIndexOf('.')));
            parent.AddChild(graph);
            
            // Console.WriteLine($"graph path: {graph.Path}");
            
            GraphDict.Add(graph.Path, graph);
            graph.Project = this;

            AddGraphEvent?.Invoke(graph);
        }

        public void RenameItem(TreeItem item, string oldPath, string newPath)
        {
            var foundItem = GetItem(oldPath);
            if (foundItem != null)
            {
                Debug.Assert(foundItem == item);
                GraphDict.Remove(oldPath);
            }

            GraphDict.Add(newPath, item);
        }

        public Graph? GetGraph(string path)
        {
            GraphDict.TryGetValue(path, out var item);
            return item as Graph;
        }

        public TreeItem? GetItem(string path)
        {
            GraphDict.TryGetValue(path, out var item);
            return item;
        }

        public Folder GetFolder(string path)
        {
            var item = GetItem(path);
            if (item is Folder f)
                return f;

            Folder folder;
            var dotPos = path.LastIndexOf('.');
            if (dotPos == -1)
            {
                var config = Config.GraphRoots.Find(cfg => cfg.Name == path);
                var type = config == null ? BuiltinTypes.GlobalType : GetType(config.DefaultType);
                folder = new Folder(path) { Path = path, Parent = Root, Project = this, Type = type ?? BuiltinTypes.GlobalType};
                Root.AddChild(folder);
                GraphDict.Add(folder.Path, folder);
            }
            else
            {
                var parent = GetFolder(path.Substring(0, dotPos));
                var name = path.Substring(dotPos + 1);
                folder = new Folder(name) { Path = path, Parent = parent, Project = this};
                parent.AddChild(folder);
                GraphDict.Add(folder.Path, folder);
            }
            return folder;

        }

        public void Remove(TreeItem item)
        {
            if(item is Graph graph)
                _removeGraph(graph);
            else if(item is Folder folder)
                _removeFolder(folder);
        }

        void _removeGraph(Graph graph)
        {
            if (!GraphDict.ContainsKey(graph.Path))
            {
                return;
            }

            if (graph.IsSubGraph)
            {
                graph.SetSubGraph(Graph.SubGraphTypeEnum.NONE);
            }
            GraphDict.Remove(graph.Path);
            RemoveGraphEvent?.Invoke(graph);

            //TODO need test
            // remove file if needed
            var rootFolder = graph.GetRoot();
            if (rootFolder != null)
            {
                var rootConfig = Config?.GetGraphRoot(rootFolder.Name);
                if (rootConfig != null && rootConfig.SaveRule is FilePerGraphSaveRule)
                {
                    var saveFilePath = System.IO.Path.Combine(Path, rootConfig.Path, rootConfig.SaveRule.GetGraphSaveFile(graph.Path), ".yaml");
                    OutputMessage.Inst?.Output($"remove file {saveFilePath}");
                    FileHelper.RemoveFile(saveFilePath);
                }
            }
        }

        void _removeFolder(Folder folder)
        {
            folder.Children.ForEach(item =>
            {
                if(item is Graph graph)
                    _removeGraph(graph);
                else if (item is Folder f)
                {
                    _removeFolder(f);
                }
            });
            GraphDict.Remove(folder.Path);
            RemoveGraphEvent?.Invoke(folder);

            // remove file if needed
            folder.RemoveFolderFile(folder.Name);
        }

        public void RenameRoot(string oldName, string newName)
        {
            var rootCfg = Config.GetGraphRoot(oldName);
            if (rootCfg == null)
                return;
            rootCfg.Name = newName;
            
            var root = Root.Children.Find(r => r.Name == oldName);
            if (root == null)
                return;

            root.Rename(newName);

            var saveFilePath = System.IO.Path.Combine(Path, rootCfg.Path, oldName + ".yaml");
            OutputMessage.Inst?.Output($"remove file {saveFilePath}");
            FileHelper.RemoveFile(saveFilePath);
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

        public event Action<Item>? OnGraphBuildEvent;

        public void RaiseGraphBuildEvent(Item item)
        {
            OnGraphBuildEvent?.Invoke(item);
        }

        #endregion

        private Dictionary<string, EnumType> EnumTypeDict { get; set; }

        public EnumType AddEnumType(string name)
        {
            if (EnumTypeDict.ContainsKey(name))
                return EnumTypeDict[name];
            var enumType = new EnumType(this, name);
            EnumTypeDict.Add(name, enumType);
            return enumType;
        }

        public EnumType? GetEnumType(string name)
        {
            if (EnumTypeDict.ContainsKey(name))
                return EnumTypeDict[name];
            return null;
        }

        public Dictionary<long, IRefData> refIdDict;
        public Dictionary<string, IRefData> RefStringDict;

        public IRefData? GetRefData(string s)
        {
            IRefData? refData = null;
            if (long.TryParse(s, out var id))
            {
                if (refIdDict.TryGetValue(id, out refData))
                    return refData;
            }

            RefStringDict.TryGetValue(s, out refData);
            return refData;
        }
    }
}
