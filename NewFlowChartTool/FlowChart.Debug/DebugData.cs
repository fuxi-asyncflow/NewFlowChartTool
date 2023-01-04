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

        public override string ToString()
        {
            return $"{NodeId}({NodeUid}): {OldStatus} -> {NewStatus}, {result}";
        }
    }

    public class VariablesStatusData : DebugData
    {
        public string VariableName;
        public string NodeUid;
        public string OldValue;
        public string NewValue;

        public override string ToString()
        {
            return $"{VariableName}: {OldValue} -> {NewValue}, {NodeUid}";
        }
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
            dataList.ForEach(Accept);
        }

        public void Accept(DebugData data)
        {
            if (data is NodeStatusData nsd)
                Accept(nsd);
            else if (data is VariablesStatusData vsd)
                Accept(vsd);
            else if (data is EventStatusData esd)
                Accept(esd);
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

    public class ReplayAgent : DebugAgent
    {
        public ReplayAgent(List<GraphDebugData> data)
        {
            Data = data;

            StartFrame = Data.First().Frame;
            EndFrame = Data.Last().Frame;
            StartTime = Data.First().Time;
            EndTime = Data.First().Time;

            _currentFrameDataIndex = -1;
            _currentDataIndex = -1;
        }

        public void Next()
        {
            if (_currentDataIndex < 0)
            {
                _currentFrameDataIndex++;
                if (_currentFrameDataIndex >= Data.Count)
                {
                    _currentFrameDataIndex = -1;
                    _currentDataIndex = -1;
                    return;
                }
                _currentDataIndex = 0;
            }
            else
            {
                var data = Data[_currentFrameDataIndex].DebugDataList;
                if (_currentDataIndex < data.Count)
                {
                    Accept(data[_currentDataIndex]);
                }
                else
                {
                    _currentDataIndex = -1;
                }
            }
        }

        public void NextFrame()
        {
            if (_currentDataIndex >= 0)
            {
                var data = Data[_currentFrameDataIndex].DebugDataList;
                while (_currentDataIndex < data.Count)
                {
                    Accept(data[_currentDataIndex]);
                }

                _currentDataIndex = -1;
                _currentFrameDataIndex++;
                return;
            }

            if (_currentFrameDataIndex == -1)
                _currentFrameDataIndex = 0;

            if (_currentFrameDataIndex < Data.Count)
            {
                Accept(Data[_currentFrameDataIndex].DebugDataList);
                _currentFrameDataIndex++;
            }
            else
            {
                _currentDataIndex = -1;
            }
        }

        public async void Play()
        {
            _currentFrameDataIndex = 0;
            while (_currentDataIndex < Data.Count)
            {
                Accept(Data[_currentFrameDataIndex].DebugDataList);
                await Task.Delay(1000);
            }
        }

        public void Pause()
        {

        }

        private List<GraphDebugData> Data;
        public long StartFrame;
        public long EndFrame;
        public long StartTime;
        public long EndTime;

        private int _currentFrameDataIndex;
        private int _currentDataIndex;
    }
}
