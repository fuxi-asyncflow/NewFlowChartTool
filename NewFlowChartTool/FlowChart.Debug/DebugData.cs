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
            VariableStatusChangeEvent?.Invoke(vsd);
        }

        public void Accept(EventStatusData esd)
        {

        }

        public virtual void Stop() {}

        public delegate void NodeStatusChangeDelegate(NodeStatusData info);
        public event NodeStatusChangeDelegate? NodeStatusChange;
        public event Action<VariablesStatusData>? VariableStatusChangeEvent;
    }

    public class ReplayAgent : DebugAgent
    {
        public ReplayAgent(List<GraphDebugData> data)
        {
            Data = data;
            if (Data.Count > 0)
            {
                StartFrame = Data.First().Frame;
                EndFrame = Data.Last().Frame;
                StartTime = Data.First().Time;
                EndTime = Data.First().Time;
            }

            CurrentFrameDataIndex = 0;
            _currentDataIndex = 0;
        }

        public void Next()
        {
            if (CurrentFrameDataIndex >= Data.Count)
                return;
            var currentData = Data[CurrentFrameDataIndex].DebugDataList;
            if (_currentDataIndex < currentData.Count)
            {
                Accept(currentData[_currentDataIndex]);
                _currentDataIndex++;
            }

            if (_currentDataIndex >= currentData.Count)
            {
                CurrentFrameDataIndex++;
                _currentDataIndex = 0;
            }
        }

        public void NextFrame()
        {
            if (_currentDataIndex != 0)
            {
                FinishCurrentFrame();
                return;
            }

            if (CurrentFrameDataIndex < Data.Count)
            {
                Accept(Data[CurrentFrameDataIndex].DebugDataList);
                CurrentFrameDataIndex++;
            }
        }

        public void Play()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(AsyncPlay, _cancellationTokenSource.Token);
        }

        public void PlayTo(int frame)
        {
            while (CurrentFrameDataIndex < Data.Count && Data[CurrentFrameDataIndex].Frame < frame)
            {
                Accept(Data[CurrentFrameDataIndex].DebugDataList);
                CurrentFrameDataIndex++;
            }
        }

        async void AsyncPlay()
        {
            FinishCurrentFrame();
            await Task.Delay(1000);

            while (CurrentFrameDataIndex < Data.Count && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                Accept(Data[CurrentFrameDataIndex].DebugDataList);
                CurrentFrameDataIndex++;
                await Task.Delay(1000);
            }
        }

        void FinishCurrentFrame()
        {
            if (CurrentFrameDataIndex >= Data.Count)
                return;
            var data = Data[CurrentFrameDataIndex].DebugDataList;
            while (_currentDataIndex < data.Count)
            {
                Accept(data[_currentDataIndex]);
                _currentDataIndex++;
            }

            CurrentFrameDataIndex++;
            _currentDataIndex = 0;
        }

        public void Pause()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        void Reset()
        {
            CurrentFrameDataIndex = 0;
            _currentDataIndex = 0;
        }

        public override void Stop()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
            Reset();
        }

        public int GetPrevFrame()
        {
            if (CurrentFrameDataIndex == 0)
                return 0;
            CurrentFrameDataIndex--;
            return (int)Data[CurrentFrameDataIndex].Frame;

        }

        private List<GraphDebugData> Data;
        public long StartFrame;
        public long EndFrame;
        public long StartTime;
        public long EndTime;

        private int _currentFrameDataIndex;
        private int CurrentFrameDataIndex
        {
            get => _currentFrameDataIndex;
            set
            {
                _currentFrameDataIndex = value;
                if(_currentFrameDataIndex>=0 && _currentFrameDataIndex < Data.Count)
                    CurrentFrameChangeEvent?.Invoke((int)Data[_currentFrameDataIndex].Frame);
            }
        }

        public event Action<int>? CurrentFrameChangeEvent;

        private int _currentDataIndex;
        private CancellationTokenSource? _cancellationTokenSource;

        public event Action? EndEvent;
    }
}
