using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Misc;
using FlowChartCommon;

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
            if (splits.Length < 2)
            {
                var msg = $"graphPath {graphPath} should contains .";
                Logger.ERR(msg);
                throw new Exception(msg);
            }

            if (splits.Length == 2)
            {
                return splits[0];
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
            
        }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("output")]
        public string OutputPath { get; set; }
        [JsonPropertyName("type")]
        public string DefaultType;

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
    }

    public class ProjectConfig
    {
        public ProjectConfig()
        {
            GraphRoots = new List<GraphRootConfig>();
            _graphRootsDict = new Dictionary<string, GraphRootConfig>();
        }

        [JsonPropertyName("name")]
        public string Name;
        [JsonPropertyName("graph_roots")]
        public List<GraphRootConfig> GraphRoots { get; set; }

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
    }
}
