using FlowChart.Type;
using FlowChart.Core;
using System.Text.Json;

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

        public Parameter ToParameter()
        {
            return new Parameter { Name = Name, Type = TypeJson.GetType(Type) };
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
        public List<ParameterInfo> Parameters { get; set; }

        public Member ToMember()
        {
            Member member = null;
            try
            {                
                if (MemberType == 2)
                {
                    member = new Method()
                    {
                        Name = Name,
                        Parameters = Parameters.ConvertAll(p => p.ToParameter()),
                        Type = TypeJson.GetType(Type)
                    };
                }
                else if (MemberType == 1)
                {
                    member = new Property()
                    {
                        Name = Name,
                        Type = TypeJson.GetType(Type)
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

        public static Dictionary<string, FlowChart.Type.Type> types 
            = new Dictionary<string, FlowChart.Type.Type>();
        
        public static FlowChart.Type.Type GetType(string name)
        {
            if(types.ContainsKey(name))
            {
                return types[name];
            }
            return new FlowChart.Type.Type { Name = $"Undefined<{name}>"};
        }

    }

    class NodeJson
    {
        public string Uid { get; set; }
        public string Text { get; set; }

        public Node ToNode()
        {
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

        public Connector Connector()
        {
            return new Connector();
        }
    }

    class ChartJson
    {
        public string Uid { get; set;}
        public string Path { get; set; }
        public string Type { get; set; }
        public List<NodeJson> Nodes { get; set;}
        public List<ConnectorJson> Connectors { get; set;}

        public void ToGraph(Graph g)
        {
            g.Uid = Uid;
            g.Path = Path;
            Nodes.ForEach(node => g.AddNode(node.ToNode()));
            Connectors.ForEach(con => g.Connect(con.Start, con.End));
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

            LoadTypes($"{path}{sp}types");            

            foreach (var kv in projectConfig.flowchart_type)
            {
                LoadFlowchartFile($"{path}{sp}charts{sp}{kv.Key}.json");
            }
            
        }

        public void Save(Project project)
        {
            throw new NotImplementedException();
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
            foreach(var fileName in files)
            {
                if(fileName.EndsWith(".json"))
                {
                    if (fileName.EndsWith("event.json"))
                        continue;
                    var fs = GetFileStream(fileName);
                    fs.Position = 0;
                    var tpJson = JsonSerializer.Deserialize<TypeJson>(fs);                    
                    var tp = new FlowChart.Type.Type();
                    if(tpJson == null)
                    {
                        Console.WriteLine("load chart json failed");
                        continue;
                    }
                    tp.Name = tpJson.Name;
                    tmp_dict.Add(tp, tpJson);
                }
            }

            foreach(var kv in tmp_dict)
            {
                LoadType(kv.Key, kv.Value);
            }
        }

        bool LoadType(FlowChart.Type.Type tp, TypeJson js)
        {
            tp.Name = js.Name;
            tp.Members = js.Members.ConvertAll(m => m.ToMember());
            return true;
        }

        void LoadFlowchartFile(string fileName)
        {
            var fs = GetFileStream(fileName);
            fs.Position = 0;
            var chartFileJson = JsonSerializer.Deserialize<ChartFileJson>(fs, new JsonSerializerOptions { AllowTrailingCommas = true});
            Console.WriteLine($"load chart File {fileName} success, {chartFileJson.Charts.Count} chart is loaded");
            chartFileJson.Charts.ForEach(chart =>
            {
                var graph = new Graph();
                chart.ToGraph(graph);
                Project.AddGraph(graph);
            });
        }
    }
}