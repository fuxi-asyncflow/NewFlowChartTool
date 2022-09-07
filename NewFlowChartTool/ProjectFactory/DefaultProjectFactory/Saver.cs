using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FlowChart.AST;
using FlowChart.Core;
using FlowChart.Type;

namespace ProjectFactory.DefaultProjectFactory
{
    public class DefaultProjectFactory : IProjectFactory
    {
        public const string GraphFolderName = "graphs";
        public const string TypeFolderName = "types";
        public const string GraphFileExt = ".yaml";
        public const string TypeFileExt = ".yaml";
        public const string EventFileName = "_event.yaml";

        private Saver saver;
        private Loader loader;

        public DefaultProjectFactory()
        {
            saver = new Saver();
            loader = new Loader();
        }

        public void Create(Project project)
        {
            //throw new NotImplementedException();
            // test = new TestProjectFactory();
            // test.Create(project);
            loader.Load(project);
        }

        public void Save(Project project)
        {
            saver.SaveProject(project);
        }

        public static Project CreateNewProject(string path)
        {
            var p = new Project(new DefaultProjectFactory()) {Path = path};
            var factory = new MemoryProjectFactory();
            factory.Create(p);
            return p;
        }
    }

    public class Saver
    {
        public Project Project;
        public DirectoryInfo ProjectFolder;

        public Func<Graph, string>? GraphToFileRule;
        private bool SaveCode = true;

        public static string SaveGraphByRoot(Graph graph)
        {
            return graph.Path.Split('.').First();
        }

        public void SaveProject(Project project)
        {
            Project = project;
            GraphToFileRule = SaveGraphByRoot;
            PrepareFolder();
            SaveGraphs();
            SaveTypes();
        }

        public void PrepareFolder()
        {
            var attr = File.GetAttributes(Project.Path);
            //if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            //    MessageBox.Show("Its a directory");
            //else
            //    MessageBox.Show("Its a file");

            ProjectFolder = new DirectoryInfo(Project.Path);
            if (!ProjectFolder.Exists)
            {
                System.IO.Directory.CreateDirectory(ProjectFolder.FullName);
            }
        }

        public void SaveGraphs()
        {
            var graphFolder = ProjectFolder.CreateSubdirectory(DefaultProjectFactory.GraphFolderName);
            Dictionary<string, List<Graph>> graphFiles = new Dictionary<string, List<Graph>>();
            foreach (var kv in Project.GraphDict)
            {
                var graphPath = kv.Key;
                var graph = kv.Value;
                Debug.Assert(graph.Path == graphPath);
                var file = SaveGraphByRoot(graph);

                List<Graph> graphs;
                if (!graphFiles.TryGetValue(file, out graphs))
                {
                    graphs = new List<Graph>();
                    graphFiles.Add(file, graphs);
                }
                graphs.Add(graph);
            }

            foreach (var kv in graphFiles)
            {
                var filePath = kv.Key.Replace('.', System.IO.Path.PathSeparator) + DefaultProjectFactory.GraphFileExt;
                filePath = System.IO.Path.Combine(graphFolder.FullName, filePath);
                var lines = new List<string>();
                foreach (var graph in kv.Value)
                {
                    lines.Add("--- ");
                    SaveGraph(graph, lines);
                }
                lines.Add("...");
                System.IO.File.WriteAllLines(filePath, lines);
            }
        }

        public void SaveGraph(Graph graph, List<string> lines)
        {
            lines.Add($"path: {graph.Path}");
            lines.Add($"type: {graph.Type.Name}");
            if(!string.IsNullOrEmpty(graph.Description))
                lines.Add($"description: {graph.Description}");

            if (graph.IsSubGraph)
            {
                lines.Add("is_subgraph: true");
                if(graph.ReturnType != null)
                    lines.Add($"return_type: {graph.ReturnType.Name}");
            }

            if (graph.Variables.Count > 0)
            {
                lines.Add("variables: ");
                foreach (var variable in graph.Variables)
                {
                    lines.Add("- ");
                    lines.Add($"  name: {variable.Name}");
                    if (!string.IsNullOrEmpty(variable.Description))
                        lines.Add($"  description: {variable.Description}");
                    lines.Add($"  type: {variable.Type.Name}");
                    if(variable.IsParameter)
                        lines.Add($"  is_param: true");
                }
            }

            lines.Add("nodes: ");
            foreach (var node in graph.Nodes)
            {
                lines.Add("- ");
                lines.Add($"  uid: {node.Uid}");
                if (node is TextNode textNode)
                {
                    lines.Add($"  text: {textNode.Text}");
                    if (SaveCode && node.Content != null)
                    {
                        lines.Add($"  code: ");
                        var content = node.Content;
                        lines.Add($"    type: {content.Type}");

                        if (content.Contents.Count != 0)
                        {
                            if (content.Type == GenerateContent.ContentType.ERROR)
                            {
                                lines.Add($"    content: |");
                                foreach (var c in content.Contents)
                                {
                                    if (c is string s)
                                        lines.Add($"      {c.ToString()}");
                                }
                            }
                            else if (content.Type == GenerateContent.ContentType.FUNC
                                     || content.Type == GenerateContent.ContentType.EVENT)
                            {
                                lines.Add($"    content: |");
                                foreach (var c in content.Contents)
                                {
                                    if (c is string s)
                                        lines.Add($"      {c.ToString()}");
                                }
                            }
                            else if (content.Type == GenerateContent.ContentType.CONTROL)
                            {
                                lines.Add($"    content: ");
                                foreach (var c in content.Contents)
                                {
                                    if (c is string s)
                                        lines.Add($"    - {c.ToString()}");
                                }
                            }
                        }
                    }
                }
            }

            if (graph.Connectors.Count > 0)
            {
                lines.Add("connectors: ");
                foreach (var connector in graph.Connectors)
                {
                    lines.Add("- ");
                    lines.Add($"  start: {connector.Start.Uid}");
                    lines.Add($"  end: {connector.End.Uid}");
                    lines.Add($"  type: {(int)connector.ConnType}");
                }
            }
        }

        public void SaveTypes()
        {
            var typeFolder = ProjectFolder.CreateSubdirectory(DefaultProjectFactory.TypeFolderName);
            foreach (var kv in Project.TypeDict)
            {
                var tp = kv.Value;
                if (tp.IsBuiltinType && tp != BuiltinTypes.GlobalType)
                {
                    continue;
                }

                var filePath = kv.Key + DefaultProjectFactory.TypeFileExt;
                filePath = System.IO.Path.Combine(typeFolder.FullName, filePath);
                var lines = new List<string>();
                SaveType(tp, lines);
                System.IO.File.WriteAllLines(filePath, lines);
            }

            var evFilePath = System.IO.Path.Combine(typeFolder.FullName, DefaultProjectFactory.EventFileName);
            var evlines = new List<string>();
            SaveEvents(evlines);
            System.IO.File.WriteAllLines(evFilePath, evlines);
        }

        public void SaveType(FlowChart.Type.Type type, List<string> lines)
        {
            lines.Add("--- ");
            lines.Add($"name: {type.Name}");
            var props = new SortedList<string, Property>();
            var methods = new SortedList<string, Method>();
            foreach (var kv in type.MemberDict)
            {
                if (!kv.Value.SaveToFile)
                    continue;
                if (kv.Value is Property prop)
                    props.Add(prop.Name, prop);
                else if(kv.Value is Method method)
                    methods.Add(method.Name, method);
            }

            if(props.Count > 0)
                lines.Add($"properties: ");
            //TODO sort members
            foreach (var kv in props)
            {
                var member = kv.Value;
                lines.Add("- ");
                lines.Add($"  name: {member.Name}");
                if (!string.IsNullOrEmpty(member.Description))
                    lines.Add($"  description: {member.Description}");
                lines.Add($"  type: {member.Type.Name}");
                if (!string.IsNullOrEmpty(member.Template))
                    lines.Add($"  template: {member.Template}");
            }

            if(methods.Count > 0)
                lines.Add($"methods: ");
            foreach (var kv in methods)
            {
                var method = kv.Value;
                lines.Add("- ");
                lines.Add($"  name: {method.Name}");
                if (!string.IsNullOrEmpty(method.Description))
                    lines.Add($"  description: {method.Description}");
                lines.Add($"  type: {method.Type.Name}");
                if (!string.IsNullOrEmpty(method.Template))
                    lines.Add($"  template: {method.Template}");
                if (method.IsCustomGen)
                    lines.Add($"  custom_gen: true");

                if (method.Parameters.Count > 0)
                {
                    lines.Add("  parameters: ");
                    foreach (var para in method.Parameters)
                    {
                        var tmp = $"name: {para.Name}, type: {para.Type.Name}";
                        if (!string.IsNullOrEmpty(para.Description))
                            tmp += $", description: {para.Description}";
                        if (para.IsVariadic)
                            tmp += ", variadic: true";
                        lines.Add($"  - {{{tmp}}}");
                    }
                }
                
            }


            lines.Add("...");
        }

        public void SaveEvents(List<string> lines)
        {
            lines.Add("--- ");
            var events = Project.EventDict.Values.ToList();
            events.Sort((a, b) =>  a.EventId.CompareTo(b.EventId));
            foreach (var ev in events)
            {
                lines.Add("- ");
                lines.Add($"  id: {ev.EventId}");
                lines.Add($"  name: {ev.Name}");
                if (!string.IsNullOrEmpty(ev.Description))
                {
                    lines.Add($"  description: {ev.Description}");
                }

                if (ev.Parameters.Count > 0)
                {
                    lines.Add("  parameters: ");
                    foreach (var para in ev.Parameters)
                    {
                        if (string.IsNullOrEmpty(para.Description))
                            lines.Add($"  - {{name: {para.Name}, type: {para.Type.Name}}}");
                        else
                            lines.Add($"  - {{name: {para.Name}, description: {para.Description}, type: {para.Type.Name}}}");
                    }
                }
            }
            lines.Add("...");
        }

        //TODO if string has #, it should inside double quotes
        public static string CheckString(string str)
        {
            if (str.Contains('#'))
                return $"\"{str}\"";
            return str;
        }
    }
}
