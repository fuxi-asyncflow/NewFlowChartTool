using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FlowChart;
using FlowChart.Core;
using FlowChart.Type;
using FlowChart.Common;
using NLog.LayoutRenderers.Wrappers;
using YamlDotNet.Core;
using YamlDotNet.Core.Tokens;
using YamlDotNet.RepresentationModel;
using Type = FlowChart.Type.Type;

namespace ProjectFactory.DefaultProjectFactory
{
    public static class YamlExtension
    {
        public static string? Get(this YamlMappingNode self, YamlScalarNode key)
        {
            if (self.Children.TryGetValue(key, out YamlNode? node))
            {
                if(node is YamlScalarNode scalarNode)
                    return scalarNode.Value;
            }
            return null;
        }

        public static int? GetInt(this YamlMappingNode self, YamlScalarNode key)
        {
            if (self.Children.TryGetValue(key, out YamlNode? node))
            {
                if (node is YamlScalarNode scalarNode && scalarNode.Value != null)
                    return Int32.Parse(scalarNode.Value);
            }
            return null;
        }

        public static YamlSequenceNode? GetArray(this YamlMappingNode self, YamlScalarNode key)
        {
            if (self.Children.TryGetValue(key, out YamlNode? node))
            {
                return node as YamlSequenceNode;
            }
            return null;
        }
    }
    public class Loader
    {
        public Project Project;
        public DirectoryInfo ProjectFolder;
        public Dictionary<string, Group> GroupDict;

        public static YamlScalarNode YAML_NAME = new YamlScalarNode("name");
        public static YamlScalarNode YAML_DESCRIPTION = new YamlScalarNode("description");
        public static YamlScalarNode YAML_ID = new YamlScalarNode("id");
        public static YamlScalarNode YAML_PATH = new YamlScalarNode("path");
        public static YamlScalarNode YAML_TYPE = new YamlScalarNode("type");
        public static YamlScalarNode YAML_TEMPLATE = new YamlScalarNode("template");
        public static YamlScalarNode YAML_VARIABLES = new YamlScalarNode("variables");
        public static YamlScalarNode YAML_NODES = new YamlScalarNode("nodes");
        public static YamlScalarNode YAML_UID = new YamlScalarNode("uid");
        public static YamlScalarNode YAML_TEXT = new YamlScalarNode("text");

        public static YamlScalarNode YAML_CONNECTORS = new YamlScalarNode("connectors");
        public static YamlScalarNode YAML_START = new YamlScalarNode("start");
        public static YamlScalarNode YAML_END = new YamlScalarNode("end");

        public static YamlScalarNode YAML_ABBR = new YamlScalarNode("abbr");
        public static YamlScalarNode YAML_BASE = new YamlScalarNode("base");
        public static YamlScalarNode YAML_METHODS = new YamlScalarNode("methods");
        public static YamlScalarNode YAML_PROPERTIES = new YamlScalarNode("properties");
        public static YamlScalarNode YAML_PARAMETERS = new YamlScalarNode("parameters");
        public static YamlScalarNode YAML_ASYNC = new YamlScalarNode("async");
        public static YamlScalarNode YAML_VARIADIC = new YamlScalarNode("variadic");
        public static YamlScalarNode YAML_DEFAULT = new YamlScalarNode("default");
        public static YamlScalarNode YAML_CUSTOMGEN = new YamlScalarNode("custom_gen");

        public static YamlScalarNode YAML_ISPARAMETER = new YamlScalarNode("is_param");
        public static YamlScalarNode YAML_ISSUBCHART = new YamlScalarNode("is_subgraph");
        public static YamlScalarNode YAML_ISGLOBALSUB = new YamlScalarNode("is_globalsub");
        public static YamlScalarNode YAML_RETURNTYPE = new YamlScalarNode("return_type");

        public static YamlScalarNode YAML_EXTRA = new YamlScalarNode("extra");

        public Loader()
        {
            GroupDict = new Dictionary<string, Group>();
        }

        public void Load(Project project)
        {
            Project = project;
            ProjectFolder = new DirectoryInfo(Project.Path);


            Project.RaiseProjectConfigEvent();

            try
            {
                GroupDict = new Dictionary<string, Group>();
                var sw = Stopwatch.StartNew();
                LoadTypes();
                Project.RaiseLoadTypesEndEvent();
                AddTypeCastMethod(Project);
                Logger.LOG($"load types time: {sw.ElapsedMilliseconds}");
                sw.Restart();
                LoadGraphs();
                Logger.LOG($"load graphs time: {sw.ElapsedMilliseconds}");
            }
            catch (Exception e)
            {
                Logger.ERR($"open project failed: {e.Message}");
                throw;
            }
            
        }

        public void LoadTypes()
        {
            var typeFolder = ProjectFolder.CreateSubdirectory(DefaultProjectFactory.TypeFolderName);
            Dictionary<Type, YamlMappingNode> yamls = new Dictionary<Type, YamlMappingNode>();
            string? evFileName = null;
            foreach (var file in typeFolder.EnumerateFiles())
            {
                if(!file.FullName.EndsWith(DefaultProjectFactory.TypeFileExt))
                    continue;
                if (file.FullName.EndsWith(DefaultProjectFactory.EventFileName))
                {
                    evFileName = file.FullName;
                    continue;
                }
                Logger.LOG($"load type from {file.FullName} ...");
                var yamlText = File.ReadAllText(file.FullName);
                var input = new StringReader(yamlText);
                var yaml = new YamlStream();
                try
                {
                    yaml.Load(input);
                }
                catch (Exception e)
                {
                    Logger.ERR($"parse yaml file {file.FullName} failed : {e.Message}");
#if DEBUG
                    throw;
#endif
                }

                var doc = yaml.Documents.FirstOrDefault();
                if(doc == null)
                    continue;
                var root = doc.RootNode as YamlMappingNode;

                YamlNode? node;
                var nameNode = root.Children[YAML_NAME] as YamlScalarNode;
                if (nameNode != null)
                {
                    var type = Project.GetType(nameNode.Value);
                    if (type == null)
                    {
                        type = new Type(nameNode.Value);
                        Project.AddType(type);
                    }
                    yamls.Add(type, root);
                }
            }

            foreach (var kv in yamls)
            {
                LoadType(kv.Key, kv.Value);
            }

            if (!string.IsNullOrEmpty(evFileName))
            {
                Logger.LOG($"load event from {evFileName} ...");
                LoadEvent(File.ReadAllText(evFileName));
            }
        }

        public void LoadType(Type type, YamlMappingNode root)
        {
            YamlNode? node;

            if (root.Children.TryGetValue(YAML_ABBR, out node))
            {
                type.Abbr = (node as YamlScalarNode).Value;
            }

            if (root.Children.TryGetValue(YAML_BASE, out node))
            {
                if (node is YamlScalarNode oneBaseNode)
                {
                    type.AddBaseType(Project.GetType(oneBaseNode.Value));
                }
                else if (node is YamlSequenceNode baseNodes)
                {
                    foreach (var baseNode in baseNodes)
                    {
                        type.AddBaseType(Project.GetType((baseNode as YamlScalarNode).Value));
                    }
                }
            }

            if (root.Children.TryGetValue(YAML_PROPERTIES, out node))
            {
                var propsNode = node as YamlSequenceNode;
                foreach (var propNode in propsNode)
                {
                    var member = LoadProperty(propNode as YamlMappingNode);
                    if (member != null)
                    {
                        type.AddMember(member);
                    }
                }
            }

            if (root.Children.TryGetValue(YAML_METHODS, out node))
            {
                var methodsNode = node as YamlSequenceNode;
                foreach (var methodNode in methodsNode)
                {
                    var member = LoadMethod(methodNode as YamlMappingNode);
                    if (member != null)
                    {
                        type.AddMember(member);
                    }
                }
            }

            if (root.Children.TryGetValue(YAML_EXTRA, out node))
            {
                var extraPropsNode = node as YamlMappingNode;
                foreach (var propNode in extraPropsNode)
                {
                    var key = propNode.Key.ToString();
                    var value = propNode.Value.ToString();
                    type.SetExtraProp(key, value);
                }
            }
        }

        void AddTypeCastMethod(Project? p)
        {
            var typeList = new List<Type>();
            var globalType = BuiltinTypes.GlobalType;
            var excludeTypes = new HashSet<Type>() { BuiltinTypes.GlobalType, BuiltinTypes.UndefinedType, BuiltinTypes.VoidType };
            foreach (var kv in p.TypeDict)
            {
                var name = kv.Key;
                if(excludeTypes.Contains(kv.Value))
                    continue;
                if (name.Contains('<'))
                    continue;
                if (globalType.FindMember(name) != null)
                {
                    Logger.LOG($"not add cast function for type {name} because method with same name exist in Global");
                    continue;
                }

                var method = new Method(name) {Type = kv.Value, Template = "$params", Description = $"cast to {name} type"};
                method.SaveToFile = false;
                method.Parameters.Add(new Parameter("value") {Type = BuiltinTypes.AnyType});
                globalType.AddMember(method);
            }
        }

        public Property? LoadProperty(YamlMappingNode? root)
        {
            if (root == null)
                return null;
            

            YamlNode? node;
            var nameNode = root.Children[YAML_NAME] as YamlScalarNode;
            if (nameNode == null)
                return null;

            var prop = new Property(nameNode.Value);

            var typeNode = root.Children[YAML_TYPE] as YamlScalarNode;
            prop.Type = Project.GetType(typeNode.Value) ?? BuiltinTypes.AnyType;

            if (root.Children.ContainsKey(YAML_DESCRIPTION) && root.Children[YAML_DESCRIPTION] is YamlScalarNode descriptionNode)
                prop.Description = descriptionNode.Value;

            if (root.Children.ContainsKey(YAML_TEMPLATE) && root.Children[YAML_TEMPLATE] is YamlScalarNode templateNode)
                prop.Template = templateNode.Value;

            return prop;
        }

        public Method? LoadMethod(YamlMappingNode? root)
        {
            if (root == null)
                return null;


            YamlNode? node;
            var nameNode = root.Children[YAML_NAME] as YamlScalarNode;
            if (nameNode == null)
                return null;

            var method = new Method(nameNode.Value);

            if ( root.Children.TryGetValue(YAML_TYPE, out node))
            {
                var typeNode = node as YamlScalarNode;
                method.Type = Project.GetType(typeNode.Value) ?? BuiltinTypes.AnyType;
            }

            if (root.Children.ContainsKey(YAML_DESCRIPTION) && root.Children[YAML_DESCRIPTION] is YamlScalarNode descriptionNode)
                method.Description = descriptionNode.Value;

            if (root.Children.ContainsKey(YAML_ASYNC) && root.Children[YAML_ASYNC] is YamlScalarNode asyncNode)
                method.IsAsync = asyncNode.Value == "true";

            if (root.Children.ContainsKey(YAML_TEMPLATE) && root.Children[YAML_TEMPLATE] is YamlScalarNode templateNode)
                method.Template = templateNode.Value;

            if (root.Children.ContainsKey(YAML_CUSTOMGEN) && root.Children[YAML_CUSTOMGEN] is YamlScalarNode customGenNode)
                method.IsCustomGen = customGenNode.Value == "true";

            if (root.Children.ContainsKey(YAML_VARIADIC) && root.Children[YAML_VARIADIC] is YamlScalarNode variadicNode)
                method.IsVariadic = variadicNode.Value == "true";

            if (root.Children.TryGetValue(YAML_PARAMETERS, out node))
            {
                var parasNode = node as YamlSequenceNode;
                foreach (var tmpNode in parasNode)
                {
                    var paraNode = tmpNode as YamlMappingNode;
                    if(paraNode == null)
                        continue;
                    nameNode = paraNode.Children[YAML_NAME] as YamlScalarNode;
                    var para = new Parameter(nameNode.Value);
                    var typeNode = paraNode.Children[YAML_TYPE] as YamlScalarNode;
                    para.Type = Project.GetType(typeNode.Value) ?? BuiltinTypes.AnyType;
                    method.Parameters.Add(para);

                    if(paraNode.Children.ContainsKey(YAML_DESCRIPTION) && paraNode.Children[YAML_DESCRIPTION] is YamlScalarNode paraDescripNode)
                        para.Description = paraDescripNode.Value;
                    if (paraNode.Children.ContainsKey(YAML_DEFAULT) && paraNode.Children[YAML_DEFAULT] is YamlScalarNode paraDefaultNode)
                        para.Default = paraDefaultNode.Value;

                }
            }
            return method;
        }

        public void LoadEvent(string yamlText)
        {
            var input = new StringReader(yamlText);
            var yaml = new YamlStream();
            yaml.Load(input);

            var doc = yaml.Documents.FirstOrDefault();
            if(doc == null) return;

            var eventsNode = doc.RootNode as YamlSequenceNode;
            if (eventsNode == null) return;

            YamlNode? tmpNode = null;

            foreach (var evNode in eventsNode)
            {
                var node = evNode as YamlMappingNode;
                if(node == null) continue;

                var evName = node.Get(YAML_NAME);
                if(evName == null) continue;

                var evType = new EventType(evName);

                var description = node.Get(YAML_DESCRIPTION);
                if(!string.IsNullOrEmpty(description))
                    evType.Description = description;

                var eventId = node.GetInt(YAML_ID);
                if(eventId == null) continue;
                evType.EventId = (int)eventId;

                var paramsNode = node.GetArray(YAML_PARAMETERS);
                if (paramsNode != null)
                {
                    foreach (var paramNode in paramsNode)
                    {
                        if (paramNode is YamlMappingNode pNode)
                        {
                            var pName = pNode.Get(YAML_NAME);
                            if (pName == null) continue;
                            var para = new Parameter(pName);
                            para.Description = pNode.Get(YAML_DESCRIPTION);
                            var paraTypeName = pNode.Get(YAML_TYPE);
                            para.Type = Project.GetType(paraTypeName) ?? BuiltinTypes.AnyType;
                            evType.AddParameter(para);
                        }
                    }
                }

                Project.AddEvent(evType);
            }
        }

        public void LoadGraphs()
        {
            var roots = Project.Config.GraphRoots;

            bool _loadFile(string path)
            {
                var yamlPath = path + ".yaml";
                if (File.Exists(yamlPath))
                {
                    var file = new FileInfo(yamlPath);
                    CustomLoadGraphFile(File.ReadAllLines(file.FullName).ToList());
                    FileHelper.WriteLock(yamlPath);
                    return true;
                }
                return false;
            }

            bool _loadFolder(string path)
            {
                if (Directory.Exists(path))
                {
                    var graphFolder = new DirectoryInfo(path);
                    foreach (var file in graphFolder.EnumerateFiles())
                    {
                        CustomLoadGraphFile(File.ReadAllLines(file.FullName).ToList());
                        FileHelper.WriteLock(file.FullName);
                    }

                    return true;
                }
                return false;
            }

            foreach (var root in roots)
            {
                var path = System.IO.Path.Combine(ProjectFolder.FullName, root.Path, root.Name);
                if (root.SaveRule is FilePerGraphSaveRule)
                {
                    if (!_loadFolder(path))
                        _loadFile(path);
                }
                else if (root.SaveRule is FilePerRootSaveRule)
                {
                    if (!_loadFile(path))
                        _loadFolder(path);
                }
                else if (root.SaveRule is FilePerRootChildSaveRule)
                {
                    //_loadFile(path);
                    _loadFolder(path);
                }
            }
        }

        Folder? CustomCreateFolder(List<string> lines)
        {
            Folder? folder = null;
            int pos = 1;
            while (pos < lines.Count)
            {
                var line = lines[pos++];
                int colonPos = line.IndexOf(':');
                var key = line.Substring(0, colonPos).Trim();
                var value = line.Substring(colonPos + 1).Trim();
                //if (value.Length > 0 && value[0] == '"')
                //    value = value.Trim();
                if (key == "description")
                {
                    if(folder != null)
                        folder.Description = value;
                }
                else if (key == "path")
                {
                    folder = Project.GetFolder(value);
                }
                else if (key == "type")
                {
                    if(folder != null)
                        folder.Type = Project.GetType(value);
                }
                else if (key == "extra")
                {
                    do
                    {
                        var l = lines[pos++];
                        int extPos = l.IndexOf(':');
                        var extKey = l.Substring(2, extPos-2).Trim();
                        var extValue = l.Substring(extPos + 1).Trim();
                        if (extValue.StartsWith('"'))
                            extValue = extValue.Trim('\"');
                        folder.SetExtraProp(extKey, extValue);

                    } while (line.Length > 2 && line[1] == ' ' && line[1] == ' ');
                    pos--;
                }
            }
            return folder;
        }

        public Graph? CustomCreateGraph(List<string> lines)
        {
            if(lines.Count == 0) return null;
            var firstLine = lines[0];
            if (!firstLine.StartsWith("path: "))
                return null;
            var path = firstLine.Substring(6).Trim();
            var name = path.Split('.').Last();
            var graph =  new Graph(name) { Path = path};
            if(Project.IsAsyncLoad)
                graph.LazyLoadFunc = delegate { CustomLoadGraph(graph, lines); };
            else
                CustomLoadGraph(graph, lines);
            //graph.LazyLoadFunc = delegate {  };
            return graph;
        }

        public void CustomLoadGraphFile(List<string> yamlLines)
        {
            List<string> graphLines = new List<string>();
            foreach (var line in yamlLines)
            {
                //if (line.Length == 4 && line.StartsWith("---") || line.StartsWith("..."))
                if(line.Length >= 3 && (
                       (line[0] == '-' && line[1] == '-' && line[2] == '-') 
                       || (line[0] == '.' && line[1] == '.' && line[2] == '.')
                       ))
                {
                    if (graphLines.Count > 1 && graphLines[0] == "kind: folder")
                    {
                        CustomCreateFolder(graphLines);
                    }
                    else
                    {
                        var graph = CustomCreateGraph(graphLines);
                        if (graph != null)
                            Project.AddGraph(graph);
                    }
                    graphLines = new List<string>();
                }
                else
                {
                    graphLines.Add(line);
                }
            }
        }

        public void CustomLoadGraph(Graph graph, List<string> lines)
        {
            GroupDict.Clear();
            var subGraph = Graph.SubGraphTypeEnum.NONE;
            int pos = 1;
            while (pos < lines.Count)
            {
                var line = lines[pos++];
                int colonPos = line.IndexOf(':');
                var key = line.Substring(0, colonPos).Trim();
                var value = line.Substring(colonPos + 1).Trim();
                //if (value.Length > 0 && value[0] == '"')
                //    value = value.Trim();
                if (key == "variables")
                {
                    CustomLoadVariables(graph, lines, ref pos);
                }
                else if (key == "nodes")
                {
                    CustomLoadNodes(graph, lines, ref pos);
                }
                else if (key == "groups")
                {
                    CustomLoadGroups(graph, lines, ref pos);
                }
                else if (key == "connectors")
                {
                    CustomLoadConnectors(graph, lines, ref pos);
                }
                else if (key == "type")
                {
                    graph.Type = Project.GetType(value);
                }
                else if (key == "description")
                {
                    graph.Description = value;
                }
                else if (key == "is_subgraph")
                {
                    subGraph = value == "true" ? Graph.SubGraphTypeEnum.LOCAL : Graph.SubGraphTypeEnum.NONE;
                    graph.ReturnType = BuiltinTypes.VoidType;
                }
                else if (key == "is_globalsub")
                {
                    subGraph = value == "true" ? Graph.SubGraphTypeEnum.GLOBAL : Graph.SubGraphTypeEnum.LOCAL;
                }
                else if (key == "return_type")
                {
                    graph.ReturnType = Project.GetType(value);
                }
                else if (key == "uid")
                {
                    graph.Uid = Guid.Parse(value);
                }
                //////////////////////////////////////////////////
                if(graph.Uid == Guid.Empty)
                    graph.Uid = Project.GenUUID();
            }

            if(subGraph != Graph.SubGraphTypeEnum.NONE)
                graph.SetSubGraph(subGraph);

            foreach (var group in GroupDict.Values)
            {
                graph.Groups.Add(group);
            }
        }

        public void CustomLoadVariables(Graph graph, List<string> lines, ref int pos)
        {
            Variable? v = null;
            while (pos < lines.Count)
            {
                var line = lines[pos++];
                var c = line.First();
                if (c == '-')
                {
                    line = lines[pos++];
                    int colonPos = line.IndexOf(':');
                    //var key = line.Substring(0, colonPos).Trim();
                    var value = line.Substring(colonPos + 1).Trim();
                    v = new Variable(value);
                    graph.AddVariable(v);
                }
                else if (c == ' ')
                {
                    int colonPos = line.IndexOf(':');
                    var key = line.Substring(0, colonPos).Trim();
                    var value = line.Substring(colonPos + 1).Trim();
                    if (key == "type")
                    {
                        v.Type = Project.GetType(value) ?? BuiltinTypes.UndefinedType;
                    }
                    else if (key == "description")
                    {
                        v.Description = value;
                    }
                    else if (key == "is_param")
                    {
                        v.IsParameter = true;
                    }
                    else if (key == "default")
                        v.DefaultValue = value;
                }
                else
                {
                    pos--;
                    break;
                }
            }
        }

        public void CustomLoadNodes(Graph graph, List<string> lines, ref int pos)
        {
            Node? node = null;
            while (pos < lines.Count)
            {
                var line = lines[pos++];
                var c = line.First();
                if (c == '-')
                {
                    var uidLine = lines[pos++];
                    int colonPos = uidLine.IndexOf(':');
                    var key = uidLine.Substring(0, colonPos).Trim();
                    var value = uidLine.Substring(colonPos + 1).Trim();
                    if (!Guid.TryParse(value, out Guid uid))
                    {
                        uid = Project.GenUUID();
                    }
                    if (graph.Nodes.Count == 0)
                    {
                        node = new StartNode(){Uid = uid};
                    }
                    else
                    {
                        node = new TextNode(){Uid = uid};
                    }
                    node.Id = graph.Nodes.Count;
                    graph.AddNode(node);
                }
                else if (c == ' ')
                {
                    int colonPos = line.IndexOf(':');
                    var key = line.Substring(0, colonPos).Trim();
                    var value = line.Substring(colonPos + 1).Trim();
                    if (key == "text")
                    {
                        if (node is TextNode textNode)
                        {
                            if (value.Length > 0 && value[0] == '"')
                                textNode.Text = value.Substring(1, value.Length-2).Replace("\\\"", "\"").Replace("\\\\", "\\");
                            else
                                textNode.Text = value;
                        }
                    }
                    else if (key == "description")
                    {
                        if (value.Length > 0 && value[0] == '"')
                            node.Description = value.Substring(1, value.Length - 2).Replace("\\\"", "\"").Replace("\\\\", "\\");
                        else
                            node.Description = value;
                    }
                    else if (key == "group")
                    {
                        Group? group = null;
                        if (!GroupDict.TryGetValue(value, out group))
                        {
                            group = new Group("NewGroup", value);
                            GroupDict.Add(value, group);
                        }
                        group.Nodes.Add(node);
                        node.OwnerGroup = group;
                    }
                    else if (key == "code")
                    {
                        // ignore generate code
                        do
                        {
                            line = lines[pos++];
                        } while (line.Length > 2 && line[0] == ' ' && line[1] == ' ' && line[2] == ' ');
                        pos--;
                    }
                    else if (key == "extra")
                    {
                        do
                        {
                            line = lines[pos++];
                            int extPos = line.IndexOf(':');
                            var extKey = line.Substring(4, extPos - 4).Trim();
                            var extValue = line.Substring(extPos + 1).Trim();
                            if (extValue.StartsWith('"'))
                                extValue = extValue.Trim('\"');
                            node.SetExtraProp(extKey, extValue);
                            if(pos >= lines.Count)
                                break;
                            line = lines[pos];
                        } while (line.Length > 4 && line[0] == ' ' && line[1] == ' ' && line[2] == ' ');
                        pos--;

                    }
                }
                else
                {
                    pos--;
                    break;
                }
            }
            
        }

        public void CustomLoadConnectors(Graph graph, List<string> lines, ref int pos)
        {
            Connector? conn = null;
            string? start = null;
            string? end = null;
            while (pos < lines.Count)
            {
                var line = lines[pos++];
                var c = line.First();
                if (c == '-')
                {
                    start = null;
                    end = null;
                }
                else if (c == ' ')
                {
                    int colonPos = line.IndexOf(':');
                    var key = line.Substring(0, colonPos).Trim();
                    var value = line.Substring(colonPos + 1).Trim();
                    if (key == "start")
                        start = value;
                    else if (key == "end")
                        end = value;
                    else if (key == "type")
                    {
                        int type = int.Parse(value);
                        if (!Guid.TryParse(start, out Guid startUid))
                        {
                            Logger.ERR($"invalid node guid in connector : {start}");
                        }
                        else if (!Guid.TryParse(end, out Guid endUid))
                        {
                            Logger.ERR($"invalid node guid in connector : {end}");
                        }
                        else
                            graph.Connect(startUid, endUid, (Connector.ConnectorType)(type));
                    }
                }
                else
                {
                    pos--;
                    break;
                }
            }
        }

        void CustomLoadGroups(Graph graph, List<string> lines, ref int pos)
        {
            Group? group = null;
            while (pos < lines.Count)
            {
                var line = lines[pos++];
                var c = line.First();
                if (c == '-')
                {
                    
                }
                else if (c == ' ')
                {
                    int colonPos = line.IndexOf(':');
                    var key = line.Substring(0, colonPos).Trim();
                    var value = line.Substring(colonPos + 1).Trim();
                    if (key == "uid")
                    {
                        if (!GroupDict.ContainsKey(value))
                        {
                            group = new Group("NewGroup", value);
                            GroupDict.Add(value, group);
                        }
                    }
                }
                else
                {
                    pos--;
                    break;
                }
            }
        }

        public void LoadGraphFile(string yamlText)
        {
            var input = new StringReader(yamlText);
            var yaml = new YamlStream();
            yaml.Load(input);
            
            foreach (var doc in yaml.Documents)
            {
                var root = doc.RootNode as YamlMappingNode;
                var graph = LoadGraph(root, Project.IsAsyncLoad);
                if(graph != null)
                    Project.AddGraph(graph);
                else
                    Logger.ERR("open graph file error: graph has no `path`");
            }
        }

        public Graph? LoadGraph(YamlMappingNode? graphNode, bool isAsync = false)
        {
            if (graphNode == null)
                return null;
            if (!graphNode.Children.TryGetValue(YAML_PATH, out var node))
                return null;

            var path = ((YamlScalarNode)node).Value;
            if (path == null)
                return null;

            var name = path.Split('.').Last();
            var graph = new Graph(name) {Path = path};
            if (isAsync)
            {
                graph.LazyLoadFunc = delegate { LoadGraph(graph, graphNode); };
            }
            else
            {
                LoadGraph(graph, graphNode);
            }
            return graph;
        }

        public void LoadGraph(Graph graph, YamlMappingNode graphNode)
        {
            GroupDict.Clear();
            YamlNode? node;

            if (graphNode.Children.TryGetValue(YAML_UID, out node))
            {
                graph.Uid = Guid.Parse(((YamlScalarNode)node).Value);
            }

            if (graphNode.Children.TryGetValue(YAML_TYPE, out node))
            {
                var typeName = ((YamlScalarNode)node).Value;
                graph.Type = Project.GetType(typeName);
            }

            if (graphNode.Children.TryGetValue(YAML_ISSUBCHART, out node))
            {
                var isSubchart = ((YamlScalarNode)node).Value;
                var isSubGraph = isSubchart != null && isSubchart == "true";
                var isGlobalSub = false;

                if (graphNode.Children.TryGetValue(YAML_ISGLOBALSUB, out node))
                {
                    var tmp = ((YamlScalarNode)node).Value;
                    isGlobalSub = tmp != null && isSubchart == "true";
                }

                graph.SubGraphType = isSubGraph
                    ? (isGlobalSub ? Graph.SubGraphTypeEnum.GLOBAL : Graph.SubGraphTypeEnum.LOCAL)
                    : Graph.SubGraphTypeEnum.NONE;
            }

            if (graphNode.Children.TryGetValue(YAML_RETURNTYPE, out node))
            {
                var returnType = ((YamlScalarNode)node).Value;
                graph.ReturnType = Project.GetType(returnType);
            }
            else if(graph.IsSubGraph)
            {
                graph.ReturnType = BuiltinTypes.VoidType;
            }

            if (graphNode.Children.TryGetValue(YAML_VARIABLES, out node))
            {
                foreach (var tmpNode in ((YamlSequenceNode)node))
                {
                    graph.AddVariable(LoadGraphVariable(tmpNode as YamlMappingNode));
                }
            }

            if (graphNode.Children.TryGetValue(YAML_NODES, out node))
            {
                foreach (var tmpNode in ((YamlSequenceNode)node))
                {
                    graph.AddNode(LoadGraphNode(tmpNode as YamlMappingNode));
                }
            }

            if (graph.Nodes.Count == 0)
            {
                graph.AddNode(new StartNode());
            }

            if (graphNode.Children.TryGetValue(YAML_CONNECTORS, out node))
            {
                foreach (var tmpNode in ((YamlSequenceNode)node))
                {
                    var connectNode = tmpNode as YamlMappingNode;
                    var start = ((YamlScalarNode)(connectNode.Children[YAML_START])).Value;
                    var end = ((YamlScalarNode)(connectNode.Children[YAML_END])).Value;
                    var connType = ((YamlScalarNode)(connectNode.Children[YAML_TYPE])).Value;
                    if (!Guid.TryParse(start, out Guid startUid))
                    {
                        Logger.ERR($"invalid node guid in connector : {start}");
                    }
                    else if (!Guid.TryParse(end, out Guid endUid))
                    {
                        Logger.ERR($"invalid node guid in connector : {end}");
                    }
                    else
                        graph.Connect(startUid, endUid, (Connector.ConnectorType)(Int32.Parse(connType)));
                }
            }

            // if graph is subgraph ,add it as a method
            if (graph.IsSubGraph)
            {
                var method = graph.ToMethod();
                graph.Type?.AddMember(method);
            }
        }

        public Node? LoadGraphNode(YamlMappingNode root)
        {
            YamlNode? tmpNode;
            string? uidStr;
            Guid uid;
            if (root.Children.TryGetValue(YAML_UID, out tmpNode))
            {
                uidStr = ((YamlScalarNode)tmpNode).Value;
                if (!Guid.TryParse(uidStr, out uid))
                    uid = Project.GenUUID();
            }
            else
            {
                uid = Project.GenUUID();
            }

            Node node;

            if (root.Children.TryGetValue(YAML_TEXT, out tmpNode))
            {
                var textNode = new TextNode(){Uid = uid};
                textNode.Text = ((YamlScalarNode)tmpNode).Value;
                node = textNode;
            }
            else
            {
                node = new StartNode() { Uid = uid };
            }

            return node;
        }

        public Variable? LoadGraphVariable(YamlMappingNode root)
        {
            YamlNode? tmpNode;
            Variable? v = null;
            if (!root.Children.TryGetValue(YAML_NAME, out tmpNode))
                return null;
            
            var name = ((YamlScalarNode)tmpNode).Value;
            v = new Variable(name);

            if (root.Children.TryGetValue(YAML_TYPE, out tmpNode))
            {
                var typeName = ((YamlScalarNode)tmpNode).Value;
                v.Type = Project.GetType(typeName);
                Debug.Assert(v.Type != null);
            }

            if (root.Children.TryGetValue(YAML_ISPARAMETER, out tmpNode))
            {
                var isParameter = ((YamlScalarNode)tmpNode).Value;
                v.IsParameter = isParameter != null && isParameter == "true";
            }


            return v;
        }
    }
}
