﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FlowChart.Common;

namespace FlowChart.Debug.WebSocket
{
    
    

    public class JsonProtocol : IDebugProtocal<string>
    {
        public static string JsonConvert(object obj)
        {
            if (obj is string s)
                return $"\"{s}\"";
            else
                return obj.ToString();

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
                var methodStr = methodObj.GetString();
                JsonElement tmpObj;
                if (root.TryGetProperty("result", out tmpObj))
                {
                    if (methodStr == "get_chart_list")
                    {
                        return DeserializeGraphList(tmpObj);
                    }

                    if (methodStr == "break_point")
                    {
                        Logger.DBG($"[debug] recv {msg}");
                    }
                }
                else if (root.TryGetProperty("params", out tmpObj))
                {
                    if (methodStr == "debug_chart")
                    {
                        return DeserializeDebugData(tmpObj);
                    }

                    if (methodStr == "quick_debug")
                    {
                        var gi = new GraphInfo();
                        gi.AgentId = tmpObj.GetProperty("agent_id").GetInt32();
                        gi.OwnerNodeAddr = tmpObj.GetProperty("owner_node_addr").GetUInt64();
                        //gi.OwnerNodeId = jObj.GetProperty("owner_node_id").GetInt32();
                        //gi.OwnerNodeUid = jObj.GetProperty("owner_node_uid").GetString();
                        //gi.ObjectName = jObj.GetProperty("object_name").GetString();
                        gi.GraphName = tmpObj.GetProperty("chart_name").GetString();
                        //gi.OwnerGraphName = jObj.GetProperty("owner_chart_name").GetString();
                        return gi;
                    }
                }
                
                

                return null;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        object DeserializeGraphList(JsonElement result)
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
                gi.GraphUid = jObj.GetProperty("graph_uid").GetString();
                list.Add(gi);
            }

            return list;
        }

        object DeserializeDebugData(JsonElement result)
        {
            var ret = new GraphDebugData()
            {
                ChartName = result.GetProperty("chart_name").GetString(),
                ChartUid = Guid.Parse(result.GetProperty("chart_uid").GetString()),
                Frame = result.GetProperty("frame").GetInt64(),
                Time = result.GetProperty("time").GetInt64(),
                DebugDataList = new List<DebugData>()
            };
            
            //var chartUidObj = result.GetProperty("chart_uid");
            //var chartUid = chartUidObj.GetString();
            var dataArrayObj = result.GetProperty("running_data");
            foreach (var dataObj in dataArrayObj.EnumerateArray())
            {
                var typeStr = dataObj.GetProperty("type").GetString();
                if (typeStr == "node_status")
                {
                    var nsd = new NodeStatusData()
                    {
                        Id = dataObj.GetProperty("id").GetInt32(),
                        NodeId = dataObj.GetProperty("node_id").GetInt32(),
                        NodeUid = Guid.Parse(dataObj.GetProperty("node_uid").GetString()),
                        OldStatus = dataObj.GetProperty("old_status").GetInt32(),
                        NewStatus = dataObj.GetProperty("new_status").GetInt32(),
                    };
                    if (nsd.NewStatus == 2)
                    {
                        nsd.result = dataObj.GetProperty("result").GetBoolean();
                    }
                    ret.DebugDataList.Add(nsd);
                }
            }
            return ret;
        }
    }
}
