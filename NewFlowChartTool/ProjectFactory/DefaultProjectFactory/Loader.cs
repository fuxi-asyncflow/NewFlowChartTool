using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FlowChart.Core;
using FlowChart.Type;
using FlowChartCommon;
using NLog.LayoutRenderers.Wrappers;
using YamlDotNet.Core;
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

        public static YamlScalarNode YAML_METHODS = new YamlScalarNode("methods");
        public static YamlScalarNode YAML_PROPERTIES = new YamlScalarNode("properties");
        public static YamlScalarNode YAML_PARAMETERS = new YamlScalarNode("parameters");

        public static YamlScalarNode YAML_ISPARAMETER = new YamlScalarNode("is_param");
        public static YamlScalarNode YAML_ISSUBCHART = new YamlScalarNode("is_subgraph");
        public static YamlScalarNode YAML_RETURNTYPE = new YamlScalarNode("return_type");

        public void Load(Project project)
        {
            Project = project;
            ProjectFolder = new DirectoryInfo(Project.Path);
            LoadTypes();
            LoadGraphs();
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
                LoadEvent(File.ReadAllText(evFileName));
            }
        }

        public void LoadType(Type type, YamlMappingNode root)
        {
            YamlNode? node;

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
            prop.Type = Project.GetType(typeNode.Value);

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
                method.Type = Project.GetType(typeNode.Value);
            }

            if (root.Children.ContainsKey(YAML_DESCRIPTION) && root.Children[YAML_DESCRIPTION] is YamlScalarNode descriptionNode)
                method.Description = descriptionNode.Value;

            if (root.Children.ContainsKey(YAML_TEMPLATE) && root.Children[YAML_TEMPLATE] is YamlScalarNode templateNode)
                method.Template = templateNode.Value;

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
                    para.Type = Project.GetType(typeNode.Value);
                    method.Parameters.Add(para);

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
                            para.Type = Project.GetType(paraTypeName);
                            evType.AddParameter(para);
                        }
                    }
                }

                Project.AddEvent(evType);
            }
        }

        public void LoadGraphs()
        {
            var graphFolder = ProjectFolder.CreateSubdirectory(DefaultProjectFactory.GraphFolderName);
            foreach (var file in graphFolder.EnumerateFiles())
            {
                var yamlText = File.ReadAllText(file.FullName);
                LoadGraphFile(yamlText);
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
                YamlNode? node;
                if (root.Children.TryGetValue(YAML_PATH, out node))
                {
                    var path = ((YamlScalarNode)node).Value;
                    var graph = new Graph("newgraph");
                    if (LoadGraph(graph, root))
                    {
                        Project.AddGraph(graph);
                    }
                }
                else
                {
                    Logger.ERR("open graph file error: graph has no `path`");
                }
            }
        }

        public bool LoadGraph(Graph graph, YamlMappingNode graphNode)
        {
            YamlNode? node;

            if (graphNode.Children.TryGetValue(YAML_PATH, out node))
            {
                graph.Path = ((YamlScalarNode)node).Value;
                graph.Name = graph.Path.Split('.').Last();
            }

            if (graphNode.Children.TryGetValue(YAML_TYPE, out node))
            {
                var typeName = ((YamlScalarNode)node).Value;
                graph.Type = Project.GetType(typeName);
            }

            if (graphNode.Children.TryGetValue(YAML_ISSUBCHART, out node))
            {
                var isSubchart = ((YamlScalarNode)node).Value;
                graph.IsSubGraph = isSubchart != null && isSubchart == "true";
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

            if (graphNode.Children.TryGetValue(YAML_CONNECTORS, out node))
            {
                foreach (var tmpNode in ((YamlSequenceNode)node))
                {
                    var connectNode = tmpNode as YamlMappingNode;
                    var start = ((YamlScalarNode)(connectNode.Children[YAML_START])).Value;
                    var end = ((YamlScalarNode)(connectNode.Children[YAML_END])).Value;
                    var connType = ((YamlScalarNode)(connectNode.Children[YAML_TYPE])).Value;
                    graph.Connect(start, end, (Connector.ConnectorType)(Int32.Parse(connType)));
                }
            }

            // if graph is subgraph ,add it as a method
            if (graph.IsSubGraph)
            {
                var method = new Method(graph.Name) { Type = graph.ReturnType };
                foreach (var v in graph.Variables)
                {
                    if(v.IsParameter)
                        method.Parameters.Add(new Parameter(v.Name)
                        {
                            Type = v.Type
                        });
                }
                graph.Type.AddMember(method);
            }

            return true;
        }

        public Node? LoadGraphNode(YamlMappingNode root)
        {
            YamlNode? tmpNode;
            string? uid;
            if (root.Children.TryGetValue(YAML_UID, out tmpNode))
                uid = ((YamlScalarNode)tmpNode).Value;
            else
            {
                uid = "1";
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
