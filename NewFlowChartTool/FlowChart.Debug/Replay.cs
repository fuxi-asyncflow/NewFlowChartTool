using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Common;

namespace FlowChart.Debug
{
    public class GraphDebugInfo
    {
        public GraphDebugInfo()
        {
            _nodeIdDict = new Dictionary<Guid, int>();
            _nodeGuidDict = new Dictionary<int, Guid>();
            _variableIdDict = new Dictionary<string, int>();
            _variableNameDict = new Dictionary<int, string>();
        }

        public void AddNode(Guid uid, int id)
        {
            if (!_nodeIdDict.ContainsKey(uid))
            {
                Logger.DBG($"[debug] add node uid {uid} {id}");
                _nodeIdDict.Add(uid, id);
                _nodeGuidDict.Add(id, uid);
            }
        }

        public int GetNodeId(Guid uid)
        {
            int nodeId = -1;
            _nodeIdDict.TryGetValue(uid, out nodeId);
            return nodeId;
        }

        public Guid GetNodeGuid(int id)
        {
            if(_nodeGuidDict.ContainsKey(id))
                return _nodeGuidDict[id];
            return Guid.Empty;
        }

        public int GetVariableId(string varName)
        {
            if (_variableIdDict.ContainsKey(varName))
            {
                return _variableIdDict[varName];
            }

            int id = _variableIdDict.Count;
            _variableIdDict[varName] = id;
            _variableNameDict[id] = varName;
            return id;
        }

        public string GetVariableName(int varId)
        {
            if(_variableNameDict.ContainsKey(varId))
                return _variableNameDict[varId];
            return string.Empty;
        }

        private Dictionary<Guid, int> _nodeIdDict;
        private Dictionary<int, Guid> _nodeGuidDict;
        private Dictionary<string, int> _variableIdDict;
        private Dictionary<int, string> _variableNameDict;

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(_nodeIdDict.Count);
            foreach (var kv in _nodeIdDict)
            {
                bw.Write(kv.Key.ToByteArray());
                bw.Write((short)kv.Value);
            }

            bw.Write(_variableIdDict.Count);
            foreach (var kv in _variableIdDict)
            {
                bw.Write(kv.Key);
                bw.Write((short)kv.Value);
            }
        }

        public void Deserialize(BinaryReader br)
        {
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var nodeUid = new Guid(br.ReadBytes(16));
                var nodeId = br.ReadInt16();
                AddNode(nodeUid, nodeId);
            }

            count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var varName = br.ReadString();
                var varId = br.ReadInt16();
                _variableIdDict.Add(varName, varId);
                _variableNameDict.Add(varId, varName);
            }
        }

        public void Print(List<string> outputs)
        {
            outputs.Add($"node id:");
            foreach (var kv in _nodeGuidDict)
            {
                outputs.Add($"  {kv.Key}  {kv.Value}");
            }
            outputs.Add($"var name:");
            foreach (var kv in _variableNameDict)
            {
                outputs.Add($"  {kv.Key}  {kv.Value}");
            }
        }
    }
    public class ReplayFile
    {
        enum Flag
        {
            NodeData = 1,
            VariableData = 2,
            EventData = 3,

            Frame = 16,
            GraphDebugData = 17,
        }
        static ReplayFile()
        {
            Inst = new ReplayFile();
            var exefolder = FileHelper.GetExeFolder();
            var debugFileFolder = System.IO.Path.Combine(exefolder, "debug");
            if (!Directory.Exists(debugFileFolder))
            {
                System.IO.Directory.CreateDirectory(debugFileFolder);
            }
        }

        public static ReplayFile Inst;

        public ReplayFile()
        {
            Data = new List<GraphDebugData>();
            GraphInfoDict = new Dictionary<Guid, GraphInfo>();
            GraphDebugInfoDict = new Dictionary<string, GraphDebugInfo>();
        }

        public void Reset()
        {
            _isLoadFromFile = false;
            Data.Clear();
            GraphInfoDict.Clear();
            GraphDebugInfoDict.Clear();
        }

        public List<GraphDebugData> Data { get; set; }
        public Dictionary<Guid, GraphInfo> GraphInfoDict { get; set; }
        public Dictionary<string, GraphDebugInfo> GraphDebugInfoDict { get; set; }
        private bool _isLoadFromFile;
        public bool IsLoadFromFile => _isLoadFromFile;


        public void AddDebugData(GraphDebugData data)
        {
            Data.Add(data);
            //TODO optimize
            var gdi = GraphDebugInfoDict[data.ChartName];
            data.DebugDataList.ForEach(d =>
            {
                if (d is NodeStatusData nsd)
                {
                    gdi.AddNode(nsd.NodeUid, nsd.NodeId);
                }
                else if (d is VariablesStatusData vsd)
                {
                    gdi.GetVariableId(vsd.VariableName);
                }
            });
        }

        public void AddGraphInfo(GraphInfo? gi)
        {
            if (gi == null)
            {
                Logger.WARN("graphInfo is null when AddGraphInfo");
                return;
            }

            var uid = Guid.Parse(gi.GraphUid);
            if(!GraphInfoDict.ContainsKey(uid))
                GraphInfoDict.Add(uid, gi);
            if(!GraphDebugInfoDict.ContainsKey(gi.GraphName))
                GraphDebugInfoDict.Add(gi.GraphName, new GraphDebugInfo());
        }

        public void Save(string fileName)
        {
            if (Data.Count == 0)
                return;
            using var ms = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            using var bw = new BinaryWriter(ms);
            long frame = -1;
            long pos = 0;
            // write header
            bw.Write((int)0);

            // write static info
            bw.Write(GraphDebugInfoDict.Count);
            foreach (var kv in GraphDebugInfoDict)
            {
                bw.Write(kv.Key);
                kv.Value.Serialize(bw);
            }

            bw.Write(GraphInfoDict.Count);
            foreach (var kv in GraphInfoDict)
            {
                bw.Write(kv.Key.ToByteArray());
                kv.Value.Serialize(bw);
            }

            foreach (var data in Data)
            {
                // write frame info
                if (data.Frame != frame)
                {
                    bw.Write((byte)Flag.Frame);
                    bw.Write(data.Frame);
                    bw.Write(data.Time);
                    frame = data.Frame;
                }

                WriteGraphDebugData(bw, data);
            }
        }

        public void Load(string fileName)
        {
            Stop();
            using var ms = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(ms);
            _isLoadFromFile = true;
            Load(br);
        }

        public void Load(BinaryReader br)
        {
            int header = br.ReadInt32();

            // read static info
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var chartName = br.ReadString();
                var gdi = new GraphDebugInfo();
                gdi.Deserialize(br);
                GraphDebugInfoDict.Add(chartName, gdi);
            }

            count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var graphUid = new Guid(br.ReadBytes(16));
                var gd = new GraphInfo();
                gd.Deserialize(br);
                GraphInfoDict.Add(graphUid, gd);
            }

            // read frame
            long frame = -1;
            long time = -1;

            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                var flag = br.ReadByte();
                if (flag == (byte)Flag.Frame)
                {
                    frame = br.ReadInt64();
                    time = br.ReadInt64();
                }
                else if (flag == (byte)Flag.GraphDebugData)
                {
                    var graphDebugData = LoadGraphDebugData(br);
                    graphDebugData.Frame = frame;
                    graphDebugData.Time = time;
                    Data.Add(graphDebugData);
                }
            }
        }

        int WriteGraphDebugData(BinaryWriter bw, GraphDebugData data)
        {
            bw.Write((byte)Flag.GraphDebugData);
            var startPos = bw.BaseStream.Position;
            bw.Write(0);

            // write graph uid
            bw.Write(data.ChartUid.ToByteArray());
            var graphDebugInfo = GraphDebugInfoDict[data.ChartName];

            // write data
            bw.Write(data.DebugDataList.Count);
            data.DebugDataList.ForEach(d =>
            {
                WriteData(bw, d, graphDebugInfo);
            });

            // write length
            var endPos = bw.BaseStream.Position;
            var length = (int)(endPos - startPos - 4);
            bw.BaseStream.Position = startPos;
            bw.Write(length);
            bw.BaseStream.Position = endPos;
            return length;
        }

        GraphDebugData LoadGraphDebugData(BinaryReader br)
        {
            var gdd = new GraphDebugData();
            int length = br.ReadInt32();
            var graphUid = new Guid(br.ReadBytes(16));
            var graphInfo = GraphInfoDict[graphUid];
            var graphDebugInfo = GraphDebugInfoDict[graphInfo.GraphName];

            var graphDebugData = new GraphDebugData();
            graphDebugData.ChartName = graphInfo.GraphName;
            graphDebugData.ChartUid = graphUid;

            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var data = LoadData(br, graphDebugInfo);
                if(data != null)
                    graphDebugData.DebugDataList.Add(data);
            }

            return graphDebugData;
        }

        void WriteData(BinaryWriter bw, DebugData data, GraphDebugInfo gi)
        {
            if (data is NodeStatusData nsd)
            {
                bw.Write((byte)Flag.NodeData);
                bw.Write((short)nsd.NodeId);
                bw.Write((byte)nsd.OldStatus);
                bw.Write((byte)nsd.NewStatus);
                bw.Write(nsd.result);
            }
            else if (data is VariablesStatusData vsd)
            {
                bw.Write((byte)Flag.VariableData);
                bw.Write((byte)gi.GetVariableId(vsd.VariableName));
                bw.Write(vsd.OldValue);
                bw.Write(vsd.NewValue);
                var nodeId = gi.GetNodeId(Guid.Parse(vsd.NodeUid));
                bw.Write((short)nodeId);
            }
            else if (data is EventStatusData esd)
            {
                bw.Write((byte)Flag.EventData);
            }
        }

        DebugData? LoadData(BinaryReader br, GraphDebugInfo gi)
        {
            var flag = br.ReadByte();
            if (flag == (byte)Flag.NodeData)
            {
                var data = new NodeStatusData();
                data.NodeId = br.ReadUInt16();
                data.OldStatus = br.ReadByte();
                data.NewStatus = br.ReadByte();
                data.result = br.ReadBoolean();
                data.NodeUid = gi.GetNodeGuid(data.NodeId);
                return data;
            }
            else if (flag == (byte)Flag.VariableData)
            {
                var data = new VariablesStatusData();
                data.VariableName = gi.GetVariableName(br.ReadByte());
                data.OldValue = br.ReadString();
                data.NewValue = br.ReadString();
                data.NodeUid = gi.GetNodeGuid(br.ReadInt16()).ToString();
                return data;
            }
            else if (flag == (byte)Flag.EventData)
            {

            }

            return null;
        }

        public void Stop()
        {
            if (!_isLoadFromFile)
            {
                var timeStr = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
                var fileName = $"debug_{timeStr}.dat";
                var exefolder = FileHelper.GetExeFolder();
                fileName = System.IO.Path.Combine(exefolder, "debug", fileName);
                Logger.LOG($"save debug data {fileName}");
                Save(fileName);
            }
            
            Reset();
            
            // Load(fileName);
        }

        public DebugAgent GetAgent(GraphInfo graphInfo)
        {
            var graphUid = Guid.Parse((ReadOnlySpan<char>)graphInfo.GraphUid);
            var data = Data.FindAll(d => d.ChartUid == graphUid);
            var agent = new ReplayAgent(data)
            {
                GraphGuid = graphUid,
                GraphName = graphInfo.GraphName,
                Info = graphInfo
            };
            return agent;

        }

        public void Print(List<string> output)
        {
            output.Add($"GraphDebugInfo:");
            foreach (var kv in GraphDebugInfoDict)
            {
                output.Add($"{kv.Key}");
                kv.Value.Print(output);
            }

            output.Add("GraphInfo");
            foreach (var kv in GraphInfoDict)
            {
                output.Add($"{kv.Key}");
                kv.Value.Print(output);
            }

            long frame = -1;
            foreach (var data in Data)
            {
                // write frame info
                if (data.Frame != frame)
                {
                    output.Add($"== frame: {data.Frame} {data.Time}");
                    frame = data.Frame;
                }

                data.Print(output);
            }

        }
    }
}
