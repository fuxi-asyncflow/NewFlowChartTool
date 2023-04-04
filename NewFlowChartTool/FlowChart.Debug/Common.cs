namespace FlowChart.Debug
{
    public interface IDebugMessage
    {
        public string Name { get; }
        public Dictionary<string, object> GetParams();
    }

    public interface IDebugProtocal<T>
    {
        public T Serialize(IDebugMessage msg);
        public IDebugMessage? Deserialize(T msg);
    }

    public interface INetClient
    {
        public void Init(INetManager manager, string host, int port);
        public string Host { get; }
        public int Port { get; }
        public Task<bool> Connect();
        public void Send(IDebugMessage msg);
        public void Stop();
    }

    public interface INetManager
    {
        public INetClient? GetClient(string host, int port);
        public void RemoveClient(INetClient client);
        public Task<INetClient?> Connect(string host, int port);
        public void BroadCast(string host, int startPort, int endPort, IDebugMessage msg);
        public void Send(string host, int port, IDebugMessage msg);
        public void HandleMessage(INetClient client, IDebugMessage? msg);
        public void Stop();

        #region events

        public event Action<string, int, List<GraphInfo>>? RecvGraphListEvent;
        public event Action<DebugAgent>? NewDebugAgentEvent;
        public event Action<GraphInfo>? NewDebugGraphEvent;

        #endregion
    }

    public class GetChartListMessage : IDebugMessage
    {
        public string Name => "get_chart_list";
        public string ChartName { get; set; }
        public string ObjectName { get; set; }

        public Dictionary<string, object> GetParams()
        {
            var dict = new Dictionary<string, object>();
            dict.Add("chart_name", ChartName);
            dict.Add("obj_name", ObjectName);
            return dict;
        }
    }

    public class StartDebugChartMessage : IDebugMessage
    {
        public StartDebugChartMessage(GraphInfo graphInfo)
        {
            GraphInfo = graphInfo;
        }

        public string Name => "debug_chart";
        public GraphInfo GraphInfo { get; set; }

        public Dictionary<string, object> GetParams()
        {
            var dict = new Dictionary<string, object>();
            dict.Add("chart_name", GraphInfo.GraphName);
            dict.Add("agent_id", GraphInfo.AgentId);
            dict.Add("owner_node_addr", GraphInfo.OwnerNodeAddr);
            return dict;
        }
    }

    public class SetBreakPointMessage : IDebugMessage
    {
        public string Name => "break_point";
        public string ChartName { get; set; }
        public string NodeUid { get; set; }
        public bool Command { get; set; }

        public Dictionary<string, object> GetParams()
        {
            var dict = new Dictionary<string, object>();
            dict.Add("chart_name", ChartName);
            dict.Add("node_uid", NodeUid);
            dict.Add("command", Command ? "set" : "delete");
            return dict;
        }
    }

    public class ContinueBreakPointMessage : IDebugMessage
    {
        public string Name => "continue";

        public ContinueBreakPointMessage(GraphInfo graphInfo)
        {
            GraphInfo = graphInfo;
        }

        public GraphInfo GraphInfo { get; set; }

        public Dictionary<string, object> GetParams()
        {
            var dict = new Dictionary<string, object>();
            dict.Add("chart_name", GraphInfo.GraphName);
            dict.Add("agent_id", GraphInfo.AgentId);
            dict.Add("owner_node_addr", GraphInfo.OwnerNodeAddr);
            return dict;
        }
    }

    public class QuickDebugMessage : IDebugMessage
    {
        public string Name => "quick_debug";
        public string ChartName { get; set; }

        public Dictionary<string, object> GetParams()
        {
            var dict = new Dictionary<string, object>();
            dict.Add("chart_name", ChartName);
            return dict;
        }
    }

    public class HotfixMessage : IDebugMessage
    {
        public string Name => "hotfix";
        public string ChartsFunc { get; set; }
        public string ChartsData { get; set; }

        public Dictionary<string, object> GetParams()
        {
            var dict = new Dictionary<string, object>();
            dict.Add("charts_func", ChartsFunc);
            dict.Add("charts_data", ChartsData);
            return dict;
        }
    }

    public class RecvMessage : IDebugMessage
    {
        public virtual string Name => "RecvMessage";
        public Dictionary<string, object> GetParams()
        {
            return new Dictionary<string, object>();
        }
    }

    public class GraphInfo : RecvMessage
    {
        public int AgentId { get; set; }
        public ulong OwnerNodeAddr { get; set; }
        public int OwnerNodeId { get; set; }
        public string OwnerNodeUid { get; set; }
        public string ObjectName { get; set; }
        public string GraphName { get; set; }
        public string OwnerGraphName { get; set; }
        public string GraphUid { get; set; }

        #region client info

        public string Host { get; set; }
        public int Port { get; set; }

        #endregion

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(AgentId);
            bw.Write(OwnerNodeAddr);
            bw.Write(OwnerNodeId);
            bw.Write(OwnerNodeUid);
            bw.Write(ObjectName);
            bw.Write(GraphName);
            bw.Write(OwnerGraphName);
            bw.Write(GraphUid);
            bw.Write(Host);
            bw.Write(Port);
        }

        public void Deserialize(BinaryReader br)
        {
            AgentId = br.ReadInt32();
            OwnerNodeAddr = br.ReadUInt64();
            OwnerNodeId = br.ReadInt32();
            OwnerNodeUid = br.ReadString();
            ObjectName = br.ReadString();
            GraphName = br.ReadString();
            OwnerGraphName = br.ReadString();
            GraphUid = br.ReadString();
            Host = br.ReadString();
            Port = br.ReadInt32();
        }

        public void Print(List<string> outputs)
        {
            outputs.Add($"  {AgentId}");
            outputs.Add($"  {OwnerNodeAddr}");
            outputs.Add($"  {OwnerNodeId}");
            outputs.Add($"  {OwnerNodeUid}");
            outputs.Add($"  {ObjectName}");
            outputs.Add($"  {GraphName}");
            outputs.Add($"  {OwnerGraphName}");
            outputs.Add($"  {GraphUid}");
            outputs.Add($"  {Host}");
            outputs.Add($"  {Port}");
        }
    }

    public class RecvGraphInfos : RecvMessage
    {
        public RecvGraphInfos()
        {
            GraphInfos = new List<GraphInfo>();
        }

        public List<GraphInfo> GraphInfos;
    }

    public class GraphDebugData : RecvMessage
    {
        public GraphDebugData()
        {
            DebugDataList = new List<DebugData>();
        }

        public string ChartName { get; set; }
        public Guid ChartUid { get; set; }
        public Int64 Frame { get; set; }
        public Int64 Time { get; set; }
        public List<DebugData> DebugDataList { get; set; }

        public void Print(List<string> outputs)
        {
            outputs.Add($"graph: {ChartName} {ChartUid}");
            DebugDataList.ForEach(data => outputs.Add(data.ToString()));
        }
    }
}