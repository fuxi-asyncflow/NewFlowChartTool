using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Debug
{
    internal class ReplayFile
    {
        static ReplayFile()
        {
            Inst = new ReplayFile();
        }

        public static ReplayFile Inst;

        public ReplayFile()
        {
            Data = new List<GraphDebugData>();
            GraphInfoDict = new Dictionary<string, GraphInfo>();
        }

        public List<GraphDebugData> Data { get; set; }
        public Dictionary<string, GraphInfo> GraphInfoDict { get; set; }

        public void AddDebugData(GraphDebugData data)
        {
            Data.Add(data);
        }

        public void AddGraphInfo(GraphInfo? gi)
        {
            if (gi == null)
                return;
            if(!GraphInfoDict.ContainsKey(gi.GraphUid))
                GraphInfoDict.Add(gi.GraphUid, gi);
        }

        public void Stop()
        {

        }
    }
}
