using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FlowChart;
using FlowChart.AST;
using FlowChart.Core;
using FlowChart.Type;
using FlowChart.Common;
using FlowChart.Parser;

namespace ProjectFactory.DefaultProjectFactory
{
    public class DefaultProjectFactory : IProjectFactory
    {
        public const string GraphFolderName = "graphs";
        public const string TypeFolderName = "types";
        public const string TmpFolderName = "tmp";
        public const string GraphFileExt = ".yaml";
        public static string GenerateFileExt = ".lua";
        public const string TypeFileExt = ".yaml";
        public const string EventFileName = "_event.yaml";

        private Saver saver;
        private Loader loader;

        public IProjectFactory Clone()
        {
            return new DefaultProjectFactory();
        }

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
            saver.SetLang(project.Lang);
        }

        public void Save(Project project)
        {
            saver.SaveProject(project);
        }

        public void Save(Graph graph, List<string> outputs, List<string> genLines)
        {
            saver.SaveGraph(graph, outputs, genLines);
        }

        public static Project CreateNewProject(string path)
        {
            var p = new Project(new DefaultProjectFactory()) {Path = path};
            var factory = new MemoryProjectFactory();
            factory.Create(p);
            return p;
        }

        public void LoadGraph(Project project, List<string> lines)
        {
            loader.Project = project;
            loader.CustomLoadGraphFile(lines);
        }
    }

    public class Saver
    {
        public Project Project;
        public DirectoryInfo ProjectFolder;

        public Func<Graph, string>? GraphToFileRule;

        public ISaveForLang _saveForLang;
        //private bool SaveCode = true;

        public Saver()
        {
            _saveForLang = new SaveForDefault();
        }

        public static string SaveGraphByRoot(Graph graph)
        {
            return graph.Path.Split('.').First();
        }

        public void SetLang(string? lang)
        {
            if (string.IsNullOrEmpty(lang) || lang == "lua")
            {
                _saveForLang = new SaveForLua();
                DefaultProjectFactory.GenerateFileExt = ".lua";
            }
            else if (lang == "python")
            {
                _saveForLang = new SaveForPython();
                DefaultProjectFactory.GenerateFileExt = ".py";
            }
        }

        public void SaveProject(Project project)
        {
            Project = project;
            GraphToFileRule = SaveGraphByRoot;
            PrepareFolder();
            SaveGraphs();
            SaveTypes();
            SaveConfig(project.Path + "/project.json");
        }

        public void SaveConfig(string path)
        {
            var jsonSetting = new JsonSerializerOptions() {WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault};
            FileHelper.Save(path, JsonSerializer.Serialize<ProjectConfig>(Project.Config, jsonSetting));
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
            var graphFiles = new Dictionary<string, List<TreeItem>>();
            var generateFiles = new Dictionary<string, Tuple<string, string, string>>();

            foreach (var kv in Project.GraphDict)
            {
                var graphPath = kv.Key;
                var graph = kv.Value;
                Debug.Assert(graph.Path == graphPath);
                //if (graph.SaveFilePath == null)
                
                // get root config
                var firstDotIndex = graphPath.IndexOf('.');
                var rootName = firstDotIndex == -1 ? graphPath : graphPath.Substring(0, firstDotIndex);
                var rootConfig = Project.Config.GetGraphRoot(rootName);
                if (rootConfig == null)
                {
                    var msg = $"invalid root for graph {graphPath}";
                    Logger.ERR(msg);
                    throw new Exception(msg);
                }

                var saveFile = rootConfig.SaveRule.GetGraphSaveFile(graphPath);
                var saveFilePath = Path.Combine(rootConfig.Path, saveFile) ;
                var generateFilePath = Path.Combine(rootConfig.OutputPath, saveFile);
                if (!generateFiles.ContainsKey(saveFilePath))
                {
                    generateFiles.Add(saveFilePath, new Tuple<string, string, string>(rootConfig.Name, saveFile, generateFilePath));
                }
                

                if (!graphFiles.TryGetValue(saveFilePath, out var graphs))
                {
                    graphs = new List<TreeItem>();
                    graphFiles.Add(saveFilePath, graphs);
                }
                
                graphs.Add(graph);
            }

            var codeFiles = new List<string>();
            foreach (var kv in graphFiles)
            {
                var filePath = kv.Key + DefaultProjectFactory.GraphFileExt;
                filePath = System.IO.Path.Combine(Project.Path, filePath);
                var lines = new List<string>();
                var genLines = new List<string>();
                foreach (var item in kv.Value)
                {
                    lines.Add("--- ");
                    if (item is Graph graph)
                        SaveGraph(graph, lines, genLines);
                    else
                        SaveFolder(item as Folder, lines);
                }
                lines.Add("...");
                FileHelper.Save(filePath, lines);
                if (Project.Config.StandaloneGenerateFile)
                {
                    var fileInfo = generateFiles[kv.Key];
                    codeFiles.Add(_saveForLang.SaveGenerateFile(fileInfo.Item1, fileInfo.Item2, lines, genLines));
                    FileHelper.Save(
                        Path.Combine(Project.Path, fileInfo.Item3 + DefaultProjectFactory.GenerateFileExt),
                        genLines);
                }
            }

            if (codeFiles.Count > 0)
            {
                var fileName = _saveForLang.SaveAllFlowChartsFile(codeFiles);
                FileHelper.Save(Path.Combine(Project.Path, Project.Config.Output, fileName), codeFiles);
            }
        }

        void SaveFolder(Folder folder, List<string> lines)
        {
            lines.Add($"kind: folder");
            lines.Add($"path: {folder.Path}");
            if (!string.IsNullOrEmpty(folder.Description))
                lines.Add($"description: {folder.Description}");
            lines.Add($"type: {folder.Type.Name}");
            if (folder.HasExtraInfo)
            {
                lines.Add($"extra: ");
                foreach (var kv in folder._extra)
                {
                    lines.Add($"  - {kv.Key}: \"{kv.Value}\"");
                }
            }
        }

        public void SaveGraph(Graph graph, List<string> lines, List<string> genLines)
        {
            graph.ReorderConnectors();
            lines.Add($"path: {graph.Path}");
            lines.Add($"uid: {graph.Uid}");
            lines.Add($"type: {graph.Type.Name}");
            if(!string.IsNullOrEmpty(graph.Description))
                lines.Add($"description: {graph.Description}");

            if (graph.IsSubGraph)
            {
                lines.Add("is_subgraph: true");
                if(graph.IsGlobalSubGraph)
                    lines.Add("is_globalsub: true");
                if (graph.ReturnType != null)
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
                    if(!string.IsNullOrEmpty(variable.DefaultValue))
                        lines.Add($"  default: {variable.DefaultValue}");
                }
            }

            lines.Add("nodes: ");
            var generate_standalone = graph.Project.Config.StandaloneGenerateFile;
            foreach (var node in graph.Nodes)
            {
                lines.Add("- ");
                lines.Add($"  uid: {node.Uid}");
                if (node is TextNode textNode)
                {
                    var text = textNode.Text.Replace("\\", "\\\\").Replace("\"", "\\\"");
                    lines.Add($"  text: \"{text}\"");
                    if (!string.IsNullOrEmpty(textNode.Description))
                    {
                        var description = textNode.Description.Replace("\\", "\\\\").Replace("\"", "\\\"");
                        lines.Add($"  description: \"{description}\"");
                    }
                        
                    if(node.OwnerGroup != null)
                        lines.Add($"  group: {node.OwnerGroup.Uid}");
                    
                    if (node.Content != null)
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
                                if (content.ReturnVarName != null)
                                    lines.Add($"    return_var_name: {content.ReturnVarName}");
                                if (generate_standalone)
                                {
                                    _saveForLang.SaveNodeFunc(textNode, genLines);
                                }
                                else
                                {
                                    lines.Add($"    content: |");
                                    foreach (var c in content.Contents)
                                    {
                                        if (c is string s)
                                            lines.Add($"      {c.ToString()}");
                                    }
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

                    if (node.HasExtraInfo)
                    {
                        lines.Add($"  extra: ");
                        foreach (var kv in node._extra)
                        {
                            lines.Add($"    - {kv.Key}: \"{kv.Value}\"");
                        }
                    }

                }
            }

            if (graph.Groups.Count > 0)
            {
                lines.Add("groups: ");
                foreach (var group in graph.Groups)
                {
                    lines.Add("- ");
                    lines.Add($"  uid: {group.Uid}");
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
            var files = new HashSet<string>();
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
                FileHelper.Save(filePath, lines);
                files.Add(new FileInfo(filePath).FullName);
            }

            var evFilePath = System.IO.Path.Combine(typeFolder.FullName, DefaultProjectFactory.EventFileName);
            var evlines = new List<string>();
            SaveEvents(evlines);
            FileHelper.Save(evFilePath, evlines);
            files.Add(new FileInfo(evFilePath).FullName);

            // remove type not exist any more
            var tmpFolder = ProjectFolder.CreateSubdirectory(DefaultProjectFactory.TmpFolderName);
            foreach (var file in typeFolder.EnumerateFiles())
            {
                if (!files.Contains(file.FullName))
                {
                    var newFileName = Path.Combine(tmpFolder.FullName, Path.GetFileName(file.FullName));
                    file.MoveTo(newFileName, true);
                }
            }
        }

        public void SaveType(FlowChart.Type.Type type, List<string> lines)
        {
            lines.Add("--- ");
            lines.Add($"name: {type.Name}");
            lines.Add($"abbr: {type.Abbr}");

            if (type.BaseTypes.Count > 0)
            {
                var baseTypesStr = string.Join(", ", type.BaseTypes.ConvertAll(baseType => baseType.Name));
                lines.Add($"base: [{baseTypesStr}]");
            }

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
                if (method.IsVariadic)
                    lines.Add($"  variadic: true");

                if (method.Parameters.Count > 0)
                {
                    lines.Add("  parameters: ");
                    foreach (var para in method.Parameters)
                    {
                        var tmp = $"name: {para.Name}, type: {para.Type.Name}";
                        if (!string.IsNullOrEmpty(para.Description))
                            tmp += $", description: {para.Description}";
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
                if(ev.EventId == 0)
                    continue;
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
            _saveForLang.SaveEventDefine(Project, lines);
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
