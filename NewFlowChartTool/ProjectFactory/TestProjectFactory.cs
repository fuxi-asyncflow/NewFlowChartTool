using FlowChart.Type;
using FlowChart.Core;
using System.Text.Json;
using Type = System.Type;

namespace ProjectFactory
{
    #region for json
    public class ProjectConfig
    {
        public string generator { get; set; }
        public Dictionary<string, string> flowchart_type { get; set; }
        public string output_path { get; set; }
    }

    class ParameterInfo
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string? IsParameter { get; set; }

        public Parameter ToParameter()
        {
            return new Parameter(Name) { Type = TypeJson.GetType(Type) };
        }
    }

    class MemberJson
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string? Description { get; set; }
        public int MemberType { get; set; }
        public int From { get; set; }
        public string? Source { get; set; }
        public List<ParameterInfo> Parameters { get; set; }

        public Member ToMember()
        {
            Member member = null;
            try
            {                
                if (MemberType == 2)
                {
                    member = new Method(Name)
                    {
                        Parameters = Parameters.ConvertAll(p => p == null ? new Parameter("__error") {Type = BuiltinTypes.UndefinedType} : p.ToParameter()),
                        Type = TypeJson.GetType(Type),
                        Description = Description,
                        Template = Source
                    };
                }
                else if (MemberType == 1)
                {
                    member = new Property(Name)
                    {
                        Type = TypeJson.GetType(Type),
                        Description = Description,
                        Template = Source
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"load member {Name} error");
            }
            
            return member;
        }

    }

    class TypeJson
    {
        
        public string Uid { get; set; }
        public string Name { get; set; }
        public int Source { get; set; }
        public List<MemberJson> Members { get; set; }

        public static Project Project { get; set; }
        
        public static FlowChart.Type.Type GetType(string name)
        {
            var tp = Project.GetType(name);
            if (tp == null)
            {
                Console.WriteLine($"unkown type : `{name}`");
                return FlowChart.Type.BuiltinTypes.UndefinedType;
            }

            return tp;
        }

    }

    class EventJson
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public List<ParameterInfo>? Parameters { get; set; }
        public FlowChart.Type.EventType ToEvent()
        {
            var ev = new FlowChart.Type.EventType(Name);
            ev.EventId = Id;
            if (Parameters != null)
            {
                Parameters.ForEach(param =>
                {
                    ev.AddParameter(param.ToParameter());
                });
            }
            return ev;
        }
    }


    class NodeJson
    {
        public string Uid { get; set; }
        public string Text { get; set; }

        public Node ToNode()
        {
            if (Uid == "0")
                return new StartNode() { Uid = Uid };
            return new TextNode() { Uid = Uid, Text = Text};
        }
    }

    class ConnectorJson
    {
        public string Uid { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public int Type { get; set; }
        public int Shape { get; set; }

        //public Connector Connector()
        //{
        //    return new Connector();
        //}
    }

    class ChartJson
    {
        public string Uid { get; set;}
        public string Path { get; set; }
        public string? Description { get; set; }
        public string Type { get; set; }
        public bool? IsSubChart { get; set; }
        public string? ReturnType { get; set; }
        public List<NodeJson> Nodes { get; set;}
        public List<ConnectorJson> Connectors { get; set;}
        public List<ParameterInfo> Variables { get; set; }

        public void ToGraph(Graph g)
        {
            g.Uid = Uid;
            g.Path = Path;
            g.Description = Description;
            Nodes.ForEach(node => g.AddNode(node.ToNode()));
            //g.Nodes[0] = new StartNode() { Uid = g.Nodes[0].Uid };
            Connectors.ForEach(con => g.Connect(con.Start, con.End, (Connector.ConnectorType)(con.Type)));
            if (IsSubChart != null)
                g.IsSubGraph = IsSubChart.Value;
        }
    }

    class ChartFileJson
    {
        public List<ChartJson> Charts { get; set; }
    }
    #endregion

    public class TestProjectFactory : IProjectFactory
    {
        public TestProjectFactory()
        {
            FileLock = false;
            FileDict = new Dictionary<string, FileStream>();
        }

        public bool FileLock { get; set; }
        public Dictionary<string, FileStream> FileDict { get; set; }
        ProjectConfig projectConfig { get; set; }
        public Project Project { get; set; }

        public void Create(Project project)
        {
            Project = project;
            var sp = Path.DirectorySeparatorChar;
            string path = project.Path;
            LoadConfig($"{path}{sp}config.json");
            TypeJson.Project = Project;
            LoadTypes($"{path}{sp}types");            

            foreach (var kv in projectConfig.flowchart_type)
            {
                var tp = Project.GetType(kv.Value);
                LoadFlowchartFile($"{path}{sp}charts{sp}{kv.Key}.json", tp);
            }
            
        }

        public void Save(Project project)
        {
            var saver = new DefaultProjectFactory.Saver();
            saver.SaveProject(project);
            //throw new NotImplementedException();
        }

        public FileStream GetFileStream(string fileName)
        {
            if (FileDict.ContainsKey(fileName))
                return FileDict[fileName];
            var shareMode = FileLock ? FileShare.Read : FileShare.ReadWrite;
            var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, shareMode);
            FileDict.Add(fileName, fs);
            return fs;
        }

        bool LoadConfig(string fileName)
        {
            var fs = GetFileStream(fileName);
            if(fs == null)
            {
                Console.WriteLine("project config file not exist");
                return false;
            }
            fs.Position = 0;
            var config = JsonSerializer.Deserialize<ProjectConfig>(fs);
            if(config == null)
            {
                Console.WriteLine("load project config error");
                return false;
            }

            projectConfig = config;
            Console.WriteLine("load project config success");
            return true;
        }

        // 加载json文件中的type数据
        // 加载过程分为两步，第一步先快速加载，以保证类型都有
        void LoadTypes(string typeFolder)
        {
            var files = Directory.GetFiles(typeFolder);
            var tmp_dict = new Dictionary<FlowChart.Type.Type, TypeJson>();
            string eventFileName = null;
            foreach(var fileName in files)
            {
                if(fileName.EndsWith(".json"))
                {
                    if (fileName.EndsWith("event.json"))
                    {
                        eventFileName = fileName;
                        continue;
                    }

                    var fs = GetFileStream(fileName);
                    fs.Position = 0;
                    var tpJson = JsonSerializer.Deserialize<TypeJson>(fs);
                    
                    if(tpJson == null)
                    {
                        Console.WriteLine("load chart json failed");
                        continue;
                    }
                    if(tpJson.Name != "Global")
                    {
                        var tp = new FlowChart.Type.Type(tpJson.Name);
                        Project.AddType(tp);
                        tmp_dict.Add(tp, tpJson);
                    }
                    else
                    {
                        var tp = Project.GetGlobalType();
                        tmp_dict.Add(tp, tpJson);
                    }

                }
            }

            foreach(var kv in tmp_dict)
            {
                LoadType(kv.Key, kv.Value);
            }

            if (!string.IsNullOrEmpty(eventFileName))
            {
                var eventFs = GetFileStream(eventFileName);
                eventFs.Position = 0;
                var eventJson = JsonSerializer.Deserialize<List<EventJson>>(eventFs);
                LoadEvent(eventJson);
            }
        }

        bool LoadType(FlowChart.Type.Type tp, TypeJson js)
        {
            tp.Name = js.Name;
            js.Members.ForEach(m =>
            {
                if(m.From != 1)
                    tp.AddMember(m.ToMember());
            });
            return true;
        }

        bool LoadEvent(List<EventJson> eventsJson)
        {
            foreach (var eventJson in eventsJson)
            {
                Project.AddEvent(eventJson.ToEvent());
            }
            return true;
        }

        void LoadFlowchartFile(string fileName, FlowChart.Type.Type tp)
        {
            var fs = GetFileStream(fileName);
            fs.Position = 0;
            var chartFileJson = JsonSerializer.Deserialize<ChartFileJson>(fs, new JsonSerializerOptions { AllowTrailingCommas = true});
            Console.WriteLine($"load chart File {fileName} success, {chartFileJson.Charts.Count} chart is loaded");
            chartFileJson.Charts.ForEach(chart =>
            {
                var graph = new Graph(chart.Path.Split(".").Last()) {Type = tp};
                chart.ToGraph(graph);
                if (!string.IsNullOrEmpty(chart.ReturnType))
                {
                    graph.ReturnType = Project.GetType(chart.ReturnType);
                }

                if (chart.Variables != null)
                {
                    chart.Variables.ForEach(v =>
                    {
                        var variable = new Variable(v.Name)
                        {
                            Type = Project.GetType(v.Type),
                        };
                        if (!string.IsNullOrEmpty(v.IsParameter) && v.IsParameter == "true")
                        {
                            variable.IsParameter = true;
                        }
                        graph.Variables.Add(variable);
                    });
                }

                Project.AddGraph(graph);
            });
        }
    }
}