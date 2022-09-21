using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
