using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlowChart.Debug.WebSocket
{
    public class GetChartListMessage : IDebugMessage
    {
        public string Name => "get_chart_list";
        public string ChartName { get; set; }
        public string ObjectName { get; set; }
        public Dictionary<string, string> GetParams()
        {
            var dict = new Dictionary<string, string>();
            dict.Add("chart_name", ChartName);
            dict.Add("obj_name", ObjectName);
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

        #region client info
        public string Host { get; set; }
        public int Port { get; set; }

        #endregion
    }

    public class JsonProtocol : IDebugProtocal<string>
    {
        public static string JsonConvert(object obj)
        {
            if (obj is string s)
                return $"\"{s}\"";
            throw new NotImplementedException();
        }

        private static int _id = 1;

        public string Serialize(IDebugMessage msg)
        {
            var dict = msg.GetParams();
            var sb = new StringBuilder();
            sb.Append($"{{ \"jsonrpc\": \"2.0\", \"id\": {_id++}, \"method\": \"{msg.Name}\", \"params\": {{");

            List<string> tmpStrs = new List<string>();
            foreach (var kv in dict)
            {
                tmpStrs.Add($"\"{kv.Key}\": {JsonConvert(kv.Value)}");
            }
            sb.Append(string.Join(", ", tmpStrs));
            sb.Append("}}");
            return sb.ToString();
        }

        public object? Deserialize(string msg)
        {
            try
            {
                var root = JsonDocument.Parse(msg, new JsonDocumentOptions()).RootElement;

                var methodObj = root.GetProperty("method");
                var resultObj = root.GetProperty("result");
                if (methodObj.GetString() == "get_chart_list")
                {
                    return DeserializeGraphList(resultObj);
                }

                return null;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public object DeserializeGraphList(JsonElement result)
        {
            var chartInfo = result.GetProperty("chart_info");
            var list = new List<GraphInfo>();
            foreach (var jObj in chartInfo.EnumerateArray())
            {
                var gi = new GraphInfo();
                gi.AgentId = jObj.GetProperty("agent_id").GetInt32();
                gi.OwnerNodeAddr = jObj.GetProperty("owner_node_addr").GetUInt64();
                gi.OwnerNodeId = jObj.GetProperty("owner_node_id").GetInt32();
                gi.OwnerNodeUid = jObj.GetProperty("owner_node_uid").GetString();
                gi.ObjectName = jObj.GetProperty("object_name").GetString();
                gi.GraphName = jObj.GetProperty("chart_name").GetString();
                gi.OwnerGraphName = jObj.GetProperty("owner_chart_name").GetString();
                list.Add(gi);
            }

            return list;
        }
    }
}
