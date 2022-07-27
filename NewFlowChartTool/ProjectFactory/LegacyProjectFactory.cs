using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FlowChart.Core;
using FlowChart.Type;
using Type = System.Type;
using System.Text.Json;

namespace ProjectFactory
{
    public class LegacyProjectFactory : IProjectFactory
    {
        private Project Current { get; set; }
        private DirectoryInfo ProjectDir { get; set; }
        private DirectoryInfo CurrentJsonDir { get; set; }

        private string GetFullPath(string relPath)
        {
            return ProjectDir.ToString() + "\\" + relPath;
        }

        public void Create(Project project)
        {
            Current = project;
            OpenLegacyProject(@"F:\nshm\dev\tools\designdata_tools\AsyncFlow\nshm.xml");
        }

        public bool OpenLegacyProject(string xmlPath)
        {
            var fi = new FileInfo(xmlPath);
            ProjectDir = fi.Directory;
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlPath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"load xml file error {e.Message}");
                //Output("load xml file error " + e.Message);
            }

            var rootNode = xmlDoc.DocumentElement;

            var inputPath = rootNode["Define"].GetAttribute("path");
            inputPath = GetFullPath(inputPath);
            LoadClass(inputPath);
            //LoadEvent(FileUtils.GetAbsolutePath(LegacyProjectPath, $"{inputPath}/event.json"));

            foreach (XmlNode node in rootNode.ChildNodes)
            {
                if (node.Name == "Flowchart")
                {
                    foreach (XmlNode fileNode in node.ChildNodes)
                    {
                        if (fileNode.Name == "Source")
                        {
                            XmlAttribute attr = fileNode.Attributes["path"];
                            string path = attr.Value;
                            path = GetFullPath(path);
                            string name = fileNode.Attributes["name"].Value;
                            LoadChartsFromJsonFile(name, path);
                        }
                    }
                }
            }
            CheckType();
            
            return true;
        }

        void LoadClass(string filePath)
        {

        }

        void LoadEvent(string eventPath)
        {

        }

        void LoadChartsFromJsonFile(string name, string path)
        {
            var jsonFilePath = string.Format("{0}{2}{1}.json", path, name, System.IO.Path.DirectorySeparatorChar);
            CurrentJsonDir = new FileInfo(jsonFilePath).Directory;
            var jsonStr = File.ReadAllText(jsonFilePath);
            var root = JsonDocument.Parse(jsonStr, new JsonDocumentOptions(){CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true}).RootElement;
            

            var chartsObj = root.GetProperty("chart");
            var className = root.GetProperty("class").GetString();
            //Config.FlowchartTypeDict.Add(name, className);
            LoadFolder(chartsObj, name, "", className);

            Console.WriteLine(jsonFilePath);
        }

        private void LoadFolder(JsonElement folderObj, string folderName, string parentPath, string chartType)
        {
            string path = string.IsNullOrEmpty(parentPath) ? folderName : $"{parentPath}.{folderName}";

            foreach (var kv in folderObj.EnumerateObject())
            {
                var name = kv.Name;
                var chartObj = kv.Value;
                JsonElement obj;
                
                if (chartObj.TryGetProperty("nodes", out obj))
                {
                    LoadChart(chartObj, name, path, chartType);
                }
                else if (chartObj.TryGetProperty("folder", out obj))
                {
                    LoadFolder(obj, name, path, chartType);
                }
                else if (chartObj.TryGetProperty("file", out obj))
                {
                    var jsonFilePath = CurrentJsonDir + "\\" + obj.GetString();
                    var jsonStr = File.ReadAllText(jsonFilePath);
                    var root = JsonDocument.Parse(jsonStr, new JsonDocumentOptions()
                    {
                        CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true
                    }).RootElement;
                    LoadFolder(root.GetProperty("folder"), name, path, chartType);
                }
            }
        }

        //private Variable LoadParameter(JObject paraObject)
        //{
        //    if (paraObject == null)
        //        return null;
        //    var para = new Variable();
        //    para.FromJson(paraObject);
        //    para.IsParameter = true;
        //    return para;
        //}

        private void LoadNode(JsonElement nodeObj, string nodeUId, Graph chart, Dictionary<int, string> idMap)
        {
            var node = chart.GetNode(nodeUId) as TextNode;
            node.Text = nodeObj.GetProperty("command").GetString();
            //var s = TranslateStopNode(node.Text, idMap);
            //if (!string.IsNullOrEmpty(s))
            //{
            //    node.Text = s;
            //}

            // node.Description = nodeObj.GetProperty("description").GetString();
            JsonElement subs;
            

            //var subs = nodeObj.GetValue("subsequences") as JArray;
            if (!nodeObj.TryGetProperty("subsequences", out subs))
                return;

            foreach (var sub in subs.EnumerateArray())
            {
                //var l = sub as JArray;
                var endId = Int32.Parse(sub[0].ToString());
                //var lineType = Connector.ToLineType(sub[1].Value<int>());
                if (idMap.ContainsKey(endId))
                {
                    chart.Connect(node.Uid, idMap[endId]);
                }
                else
                {
                    //Log.LogError($"cannot find uid for id [{endId}]");
                }
            }
        }

        private void LoadChart(JsonElement chartObj, string chartName, string parentPath, string chartType)
        {
            var chart = new Graph(chartName)
            {
                Path = string.Format("{0}.{1}", parentPath, chartName),
                Project = Current,
                Type = Current.GetType(chartType)
            };

            JsonElement jtoken;
            if (chartObj.TryGetProperty("export", out jtoken))
            {
                var exportObj = jtoken.GetBoolean();
                // chart.IsSubChart = exportObj;
            }

            if (chartObj.TryGetProperty("description", out jtoken))
            {
                chart.Description = jtoken.GetString();
            }

            if (chartObj.TryGetProperty("returnType", out jtoken))
            {
                // chart.ReturnType = jtoken.GetString();
            }

            //if (chartObj.TryGetValue("parameters", out jtoken))
            //{
            //    var jArray = jtoken as JArray;
            //    foreach (var paraObj in jArray)
            //    {
            //        var para = LoadParameter(paraObj as JObject);
            //        chart.Parameters.Add(para);
            //        chart.Variables.Add(para);
            //    }
            //}

            //if (chartObj.TryGetValue("variables", out jtoken))
            //{
            //    var jArray = jtoken as JArray;
            //    foreach (var paraObj in jArray)
            //    {
            //        //var para = LoadParameter(paraObj as JObject);
            //        var para = new Variable() { Name = paraObj["name"].Value<string>(), TypeName = paraObj["type"]?.Value<string>() ?? "Any" };
            //        chart.Parameters.Add(para);
            //        chart.Variables.Add(para);
            //    }
            //}

            //////////////////////////////////////////////////////////////////////////////////////
            // nodes

            // start
            var startNode = new StartNode()
            {
                Uid = "0",
            };
            if (chart.Variables.Count > 0)
            {
                //startNode.Text = "start("
                //                    + string.Join(",", chart.Parameters.ConvertAll(p => p.Name))
                //                    + ")";
                //startNode.Text = "start";
            }
            chart.AddNode(startNode);

            // nodes
            var nodesObj = chartObj.GetProperty("nodes");

            // 第一次遍历，获取id到uid的映射
            Dictionary<int, string> idMap = new Dictionary<int, string>();
            idMap.Add(0, startNode.Uid);

            foreach (var kv in nodesObj.EnumerateObject())
            {
                int nodeId = Int32.Parse(kv.Name);
                var nodeUid = Project.GenUUID().ToString();

                var node = new TextNode()
                {
                    Uid = nodeUid
                };
                idMap.Add(nodeId, nodeUid);
                chart.AddNode(node);
            }

            foreach (var kv in nodesObj.EnumerateObject())
            {
                int nodeId = Int32.Parse(kv.Name);
                LoadNode(kv.Value, idMap[nodeId], chart, idMap);
            }

            JsonElement startArray;

            if (chartObj.TryGetProperty("start", out startArray))
            {
                foreach (var jToken in startArray.EnumerateArray())
                {
                    var id = Int32.Parse(jToken.GetString());

                    chart.Connect(startNode.Uid, idMap[id]);
                }
            }

            // add group
            //var groupsObj = chartObj.GetValue("collapseSubunit") as JArray;
            //if (groupsObj != null)
            //{
            //    foreach (JObject groupObj in groupsObj)
            //    {
            //        var group = new Flowchart.Core.Group(chart);
            //        if (groupObj.ContainsKey("description"))
            //            group.Description = groupObj["description"].Value<string>();
            //        foreach (var nodeId in groupObj["nodes"] as JArray)
            //        {
            //            int id = Int32.Parse(nodeId.Value<string>());
            //            group.AddNode(chart.NodeDict[idMap[id]]);
            //            group.Uid = UidHelper.NewGuid();
            //        }

            //        group.Layout = Group.LayoutType.Custom;
            //        chart.Groups.Add(group);
            //    }
            //}

            // chart.Uid ??= NewGuid();
            // ChartDict.Add(chart.Uid, chart);
            Current.AddGraph(chart);
        }

        void CheckType()
        {

        }

        public void Save(Project project)
        {
            Console.WriteLine("nothing to do when save memory project");
        }
    }

}
