using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Debug
{
    #region DebugData
    public class DebugData
    {
        public int Id;
    }

    public class NodeStatusData : DebugData
    {
        public int NodeId;
        public Guid NodeUid;
        public int OldStatus;
        public int NewStatus;
        public bool result;
    }

    public class VariablesStatusData : DebugData
    {
        public string VariableName;
        public string NodeUid;
        public string OldValue;
        public string NewValue;
    }

    public class EventStatusData : DebugData
    {
        public string EventName;
        public string NodeUid;
        public List<string> Args;
    }

    #endregion


    //public class DebugNodeInfo
    //{
    //    public enum Status
    //    {
    //        Idle = 0,
    //        Running = 1,
    //        EndRun = 2,
    //    }

    //    public int SuccessCount;
    //    public int FailureCount;
    //    public Status NodeStatus;
    //}

    public class DebugAgent
    {
        public Guid GraphGuid { get; set; }
        public GraphInfo? Info { get; set; }
        public string GraphName { get; set; }
        //public Dictionary<Guid, DebugNodeInfo> Nodes { get; set; }
        public Dictionary<string, string> Variables { get; set; }

        public void Accept(List<DebugData> dataList)
        {
            dataList.ForEach(data =>
            {
                if(data is NodeStatusData nsd)
                    Accept(nsd);
                else if(data is VariablesStatusData vsd)
                    Accept(vsd);
                else if(data is EventStatusData esd)
                    Accept(esd);
            });
        }

        public void Accept(NodeStatusData nsd)
        {
            NodeStatusChange?.Invoke(nsd);
        }

        public void Accept(VariablesStatusData vsd)
        {

        }

        public void Accept(EventStatusData esd)
        {

        }

        public delegate void NodeStatusChangeDelegate(NodeStatusData info);
        public event NodeStatusChangeDelegate? NodeStatusChange;
    }
}
