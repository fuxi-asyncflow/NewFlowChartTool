using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Misc;
using FlowChart.Common;

namespace FlowChart
{
    public interface IGraphSaveRule
    {
        string GetGraphSaveFile(string graphPath);
    }

    public class FilePerGraphSaveRule : IGraphSaveRule
    {
        public string GetGraphSaveFile(string graphPath)
        {
            return graphPath.Replace('.', '/');
        }
    }

    public class FilePerRootSaveRule : IGraphSaveRule
    {
        public string GetGraphSaveFile(string graphPath)
        {
            var pos = graphPath.IndexOf('.');
            if (pos == -1)
            {
                var msg = $"graphPath {graphPath} should contains .";
                Logger.ERR(msg);
                throw new Exception(msg);
            }
            return graphPath.Substring(0, pos);
        }
    }

    public class FilePerRootChildSaveRule : IGraphSaveRule
    {
        public string GetGraphSaveFile(string graphPath)
        {
            var splits = graphPath.Split('.');

            if (splits.Length <= 2)
            {
                return $"{splits[0]}/{splits[0]}_root";
            }
            return $"{splits[0]}/{splits[1]}";
        }
    }

    public class GraphRootConfig
    {
        static GraphRootConfig()
        {
            GraphSaveRules = new Dictionary<string, IGraphSaveRule>();
            GraphSaveRules.Add("per_graph", new FilePerGraphSaveRule());
            GraphSaveRules.Add("per_root", new FilePerRootSaveRule());
            GraphSaveRules.Add("per_root_child", new FilePerRootChildSaveRule());
        }
        public GraphRootConfig()
        {
            Name = "root";
            Path = "graphs";
            OutputPath = "generate";
            DefaultType = "Global";
            SaveRuleString = "per_root_child";

        }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("output")]
        public string OutputPath { get; set; }
        [JsonPropertyName("type")]
        public string DefaultType { get; set; }

        private string _saveRuleString;
        [JsonPropertyName("save_rule")]
        public string SaveRuleString
        {
            get => _saveRuleString; 
            set  {
                _saveRuleString = value;
                if(GraphSaveRules.ContainsKey(_saveRuleString))
                    SaveRule = GraphSaveRules[(_saveRuleString)];
                else
                {
                    var msg = $"invalid save rule {_saveRuleString}";
                    Logger.ERR(msg);
                    throw new Exception(msg);
                }
            }
        }

        public IGraphSaveRule SaveRule;
        public static Dictionary<string, IGraphSaveRule> GraphSaveRules;

        public GraphRootConfig Clone()
        {
            return new GraphRootConfig()
            {
                Name = Name, Path = Path, OutputPath = OutputPath, DefaultType = DefaultType,
                SaveRuleString = SaveRuleString
            };
        }
    }

    public class LuaConfig
    {
        public LuaConfig()
        {
            PackagePaths = new List<string>();
            RequireFiles = new List<string>();
        }
        [JsonPropertyName("path")]
        public List<string> PackagePaths { get; set; }

        [JsonPropertyName("files")]
        public List<string> RequireFiles { get; set; }
    }

    public class DebugServerConfig
    {
        public DebugServerConfig()
        {
            Name = "default";
            Host = "127.0.0.1";
            StartPort = 9000;
            EndPort = 9003;
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("host")]
        public string Host { get; set; }
        [JsonPropertyName("start")]
        public int StartPort { get; set; }
        [JsonPropertyName("end")]
        public int EndPort { get; set; }
    }

    public class ProjectConfig
    {
        public ProjectConfig()
        {
            GraphRoots = new List<GraphRootConfig>();
            _graphRootsDict = new Dictionary<string, GraphRootConfig>();
            Output = ".";
            CodeGenerator = "lua";
            StandaloneGenerateFile = true;
        }

        [JsonPropertyName("name")]
        public string Name;

        [JsonPropertyName("loader")] 
        public string? Loader { get; set; }
        [JsonPropertyName("graph_roots")]
        public List<GraphRootConfig> GraphRoots { get; set; }
        [JsonPropertyName("output")]
        public string Output { get; set; }
        [JsonPropertyName("standalone_generate_file")]
        public bool StandaloneGenerateFile { get; set; }

        [JsonPropertyName("parser")]
        public string? ParserName { get; set; }

        [JsonPropertyName("code_generator")]
        public string CodeGenerator { get; set; }
        [JsonPropertyName("case_insensitive")]
        public bool CaseInsensitive { get; set; }
        [JsonPropertyName("encoding")]
        public string? Encoding { get; set; }

        [JsonPropertyName("lua")]
        public LuaConfig? LuaConfig { get; set; }
        [JsonPropertyName("debug_servers")]
        public List<DebugServerConfig>? DebugServers { get; set; }
        [JsonPropertyName("custom_header")]
        public List<string>? CustomHeader { get; set; }
        [JsonPropertyName("extra")]
        public Dictionary<string, string>? Extra { get; set; }

        public string CodeLang;

        public Dictionary<string, GraphRootConfig> _graphRootsDict;

        public GraphRootConfig? GetGraphRoot(string name)
        {
            if (_graphRootsDict.TryGetValue(name, out var rootConfig))
                return rootConfig;
            foreach (var cfg in GraphRoots)
            {
                if (cfg.Name == name)
                {
                    _graphRootsDict.Add(name, cfg);
                    return cfg;
                }
            }

            return null;

        }

        public ProjectConfig? Clone()
        {
            var str = JsonSerializer.Serialize<ProjectConfig>(this);
            return JsonSerializer.Deserialize<ProjectConfig>(str);
        }

        public static ProjectConfig CreateDefaultConfig()
        {
            return new ProjectConfig();
        }

        public static ProjectConfig? LoadConfig(string path)
        {
            if (!System.IO.File.Exists(path))
                return CreateDefaultConfig();
            var content = System.IO.File.ReadAllText(path);

            using (var fs = new FileStream(path, FileMode.Open))
            {
                try
                {
                    var cfg = JsonSerializer.Deserialize<ProjectConfig>(fs);
                    return cfg;
                }
                catch (Exception e)
                {
                    OutputMessage.Inst?.Output($"open project config file {path} failed: {e.Message}");
                    return null;
                }
            }
        }

        public string? GetExtraConfig(string key)
        {
            if (Extra == null)
                return null;
            if(Extra.ContainsKey(key))
                return Extra[key];
            return null;
        }
    }
}
