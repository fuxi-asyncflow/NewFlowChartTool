using FlowChart.Debug.WebSocket;

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
        public object? Deserialize(T msg);
    }

    public interface INetClient
    {
        public string Host { get;}
        public int Port { get; }
        public void Send(string message);
        public void Send(byte[] data);
        public void Stop();
    }
    public interface INetManager
    {
        public INetClient? GetClient(string host, int port);
        public void RemoveClient(INetClient client);
        public Task<INetClient?> Connect(string host, int port);
        public void BroadCast(string host, int startPort, int endPort, IDebugMessage msg);
        public void Send(string host, int port, IDebugMessage msg);
        public void HandleMessage(INetClient client, string msg);
        public void Stop();

        #region events
        public delegate void RecvGraphListDelegate(string host, int port, List<GraphInfo> graphs);
        public event RecvGraphListDelegate? RecvGraphListEvent;

        public delegate void NewDebugAgentDelegate(DebugAgent agent);
        public event NewDebugAgentDelegate? NewDebugAgentEvent;

        public delegate void NewDebugGraphDelegate(GraphInfo graphInfo);
        public event NewDebugGraphDelegate? NewDebugGraphEvent;

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

    public class GraphInfo
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
    }

    public class GraphDebugData
    {
        public string ChartName { get; set; }
        public Guid ChartUid { get; set; }
        public Int64 Frame { get; set; }
        public Int64 Time { get; set; }
        public List<DebugData> DebugDataList { get; set; }
    }


}