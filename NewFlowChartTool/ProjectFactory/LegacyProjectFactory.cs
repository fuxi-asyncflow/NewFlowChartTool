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
using FlowChart.Lua;
using FlowChartCommon;
using XLua;

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

        public void Save(Graph graph, List<string> outputs)
        {

        }

        public bool OpenLegacyProject(string xmlPath)
        {
            var fi = new FileInfo(xmlPath);
            ProjectDir = fi.Directory;
            Current.Path = ProjectDir + "\\";
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

            AddLuaInfomation(rootNode);

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

        public void AddLuaInfomation(XmlElement rootNode)
        {
            XmlNode? luaNode = null;
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                if (node.Name == "Lua")
                {
                    luaNode = node;
                    break;
                }
            }

            if (luaNode == null)
                return;

            List<string> packagePathList = new List<string>();
            List<string> requireFileList = new List<string>();
            List<string> luaCodes = new List<string>();
            foreach (XmlNode node in luaNode.ChildNodes)
            {
                // 注意路径要以\结尾
                // 相对路径以exe所在路径为准
                if (node.Name == "PackagePath")
                {
                    string package = node.Attributes["name"].Value;
                    package = package.Replace("$(ProjectDir)", ProjectDir.FullName+"\\");
                    packagePathList.Add(package);
                }
                else if (node.Name == "RequireFile")
                {
                    string requireFile = node.Attributes["name"].Value;
                    requireFileList.Add(requireFile);
                }
                else if (node.Name == "LuaCode")
                {
                    string luaCode = node.Attributes["code"].Value;
                    luaCodes.Add(luaCode);
                }
            }
            packagePathList.ForEach(Lua.AddRequirePath);
            requireFileList.ForEach(filePath =>
            {
                Lua.DoString(string.Format("require('{0}')", filePath));
            });
            luaCodes.ForEach(code => Lua.DoString(code));

            LuaFunction importEnumFunc = Lua.Inst.GetGlobal<LuaFunction>("OnOpenProjectBegin");
            if (importEnumFunc != null)
            {
                importEnumFunc.Call(new object[] { Current });
            }

        }

        void LoadType(FlowChart.Type.Type type, JsonElement json)
        {
            JsonElement j;
            if (json.TryGetProperty("inherits", out j))
            {
                foreach (var inherit in j.EnumerateArray())
                {
                    var baseType = Current.GetType(inherit.GetString());
                    if(baseType != null)
                        type.BaseTypes.Add(baseType);
                }
            }

            if (json.TryGetProperty("properties", out j))
            {
                LoadTypeMember<Property>(type, j, s => new Property(s));
            }

            if (json.TryGetProperty("actions", out j))
            {
                LoadTypeMember<Method>(type, j, s => new Method(s));
            }

            if (json.TryGetProperty("methods", out j))
            {
                LoadTypeMember<Method>(type, j, s => new Method(s));
            }

        }

        void LoadTypeMember<T>(FlowChart.Type.Type type, JsonElement json, Func<string, T> create) where T : Member
        {
            JsonElement j, jtmp;
            foreach (var jmember in json.EnumerateArray())
            {
                var member = create(jmember.GetProperty("name").GetString());
                Console.WriteLine($"add member {member.Name}");
                if (jmember.TryGetProperty("type", out j))
                {
                    var tp = Current.GetType(j.GetString());
                    member.Type = tp;
                }

                var paramList = new List<Parameter>();
                if (jmember.TryGetProperty("parameters", out j))
                {
                    foreach (var jpara in j.EnumerateArray())
                    {
                        var param = new Parameter(jpara.GetProperty("name").GetString());
                        if (jpara.TryGetProperty("type", out jtmp))
                        {
                            var paramType = Current.GetType(jtmp.GetString());
                            param.Type = paramType;
                        }
                        else
                        {
                            param.Type = BuiltinTypes.AnyType;
                        }
                        paramList.Add(param);

                    }
                }

                if (member is Method method)
                {
                    method.Parameters.AddRange(paramList);
                }

                type.AddMember(member);
            }
        }

        void LoadClass(string filePath)
        {
            var jsonFilePath = string.Format("{0}{2}{1}.json", filePath, "class", System.IO.Path.DirectorySeparatorChar);
            var jsonStr = File.ReadAllText(jsonFilePath);
            //var root = JArray.Parse(jsonStr);
            var root = JsonDocument.Parse(jsonStr, new JsonDocumentOptions()
            {
                CommentHandling = JsonCommentHandling.Skip, 
                AllowTrailingCommas = true
            }).RootElement;

            foreach (var jType in root.EnumerateArray())
            {
                var typeName = jType.GetProperty("name").GetString();
                if (Current.GetType(typeName) == null)
                {
                    var type = new FlowChart.Type.Type(typeName);
                    Current.AddType(type);
                }
            }

            foreach (var jType in root.EnumerateArray())
            {
                var typeName = jType.GetProperty("name").GetString();
                var type = Current.GetType(typeName);
                LoadType(type, jType);
            }

            // 继承表，记录子类的所有父类，即其除了自己可赋值的类型
            //var inheritDict = new Dictionary<Type, List<Type>>();
            //var fatherChildDict = new Dictionary<Type, List<Type>>();
            //void AddFather(Type child, Type father)
            //{
            //    if (!inheritDict.ContainsKey(child))
            //    {
            //        inheritDict.Add(child, new List<Type>() { father });
            //    }
            //    else
            //    {
            //        inheritDict[child].Add(father);
            //    }

            //    if (father.Inherits.Count <= 0) return;
            //    // 递归设置所有父类
            //    var fatherFatherList = father.Inherits.ConvertAll(GetType);
            //    fatherFatherList.ForEach(fft =>
            //    {
            //        AddFather(child, fft);
            //    });
            //}

            //void AddChild(Type father, Type child)
            //{
            //    if (!fatherChildDict.ContainsKey(father))
            //    {
            //        fatherChildDict.Add(father, new List<Type>() { child });
            //    }
            //    else
            //    {
            //        if (!fatherChildDict[father].Contains(child))
            //        {
            //            fatherChildDict[father].Add(child);
            //        }
            //    }
            //    // 递归，其所有父也添加该子
            //    if (inheritDict.ContainsKey(father))
            //    {
            //        inheritDict[father].ForEach(fatherFather =>
            //        {
            //            AddChild(fatherFather, child);
            //        });
            //    }
            //}

            //// 针对类型的继承关系，设置类型的可赋值项，基类可被派生类赋值，即派生类可赋值给所有的基类
            //foreach (var childType in TypeDict.Values.Where(type => type.Inherits.Count > 0))
            //{
            //    var fatherList = childType.Inherits.ConvertAll(GetType);
            //    fatherList.ForEach(fatherType =>
            //    {
            //        AddFather(childType, fatherType);
            //    });
            //}

            //// 从子-父Dict转父-子Dict
            //foreach (var childType in TypeDict.Values.Where(type => type.Inherits.Count > 0))
            //{
            //    var fatherList = childType.Inherits.ConvertAll(GetType);
            //    fatherList.ForEach(fatherType =>
            //    {
            //        AddChild(fatherType, childType);
            //    });
            //}

            //foreach (var (father, children) in fatherChildDict)
            //{
            //    father.CustomAssignmentPredict = child => children.Contains(child);
            //}

            LuaFunction importEnumFunc = Lua.Inst.GetGlobal<LuaFunction>("OnClassFileReadEnd");
            if (importEnumFunc != null)
            {
                importEnumFunc.Call(new object[] { Current });
            }

            
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
                var connType = Int32.Parse(sub[1].ToString());
                //var lineType = Connector.ToLineType(sub[1].Value<int>());
                if (idMap.ContainsKey(endId))
                {
                    chart.Connect(node.Uid, idMap[endId], (Connector.ConnectorType)connType);
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

                    chart.Connect(startNode.Uid, idMap[id], Connector.ConnectorType.ALWAYS);
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
