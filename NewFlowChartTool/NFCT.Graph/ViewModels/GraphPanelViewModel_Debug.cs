using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Debug;
using FlowChartCommon;
using NFCT.Common;
using NFCT.Common.Events;
using NFCT.Common.Services;
using Prism.Ioc;

namespace NFCT.Graph.ViewModels
{
    public enum DebugNodeStatus
    {
        IDLE = 0,
        RUNNING = 1,
        SUCCESS = 2,
        FAILURE = 3,
        ERROR = 4
    }
    public partial class GraphPaneViewModel
    {
        private bool _isDebugMode;
        public bool IsDebugMode { get => _isDebugMode; set => SetProperty(ref _isDebugMode, value); }

        public void EnterDebugMode(GraphInfo? graphInfo = null)
        {
            if (IsDebugMode)
            {
                // quick debug, graph will enterdebugmode with null for the first time,
                // when graph start running, enterdebugmode with graphinfo again 
                if (_graphInfo == null && graphInfo != null) 
                    _graphInfo = graphInfo;
                return;
            }

            _graphInfo = graphInfo;
            Logger.LOG($"[debug] {FullPath} enter debug mode");
            IsDebugMode = true;
            foreach (var baseNodeViewModel in Nodes)
            {
                baseNodeViewModel.ChangeDebugMode();
            }
        }

        public void ExitDebugMode()
        {
            if (!IsDebugMode)
                return;
            IsDebugMode = false;
            _graphInfo = null;
            Logger.LOG($"[debug] {FullPath} exit debug mode");
            _currentDebugAgent = null;
            _agents = null;
            foreach (var baseNodeViewModel in Nodes)
            {
                baseNodeViewModel.ChangeDebugMode();
                baseNodeViewModel.ExitDebugMode();
            }
        }

        private DebugAgent? _currentDebugAgent { get; set; }
        private List<DebugAgent>? _agents;
        private Dictionary<Guid, BaseNodeViewModel> _debugNodesCacheDict = new Dictionary<Guid, BaseNodeViewModel>();
        private GraphInfo? _graphInfo;

        public void UpdateAgents(List<DebugAgent>? agents)
        {
            _agents ??= new List<DebugAgent>();
            if(agents != null)
                _agents.AddRange(agents);
            if (_currentDebugAgent == null && _agents != null && _agents.Count > 0)
            {
                _currentDebugAgent = _agents.First();
                Logger.LOG($"[debug] {FullPath} bind to a debug agent");
                _currentDebugAgent.NodeStatusChange += OnNodeStatusChange;
            }
        }

        BaseNodeViewModel? _getDebugNodeViewModel(Guid uid)
        {
            if (_debugNodesCacheDict.TryGetValue(uid, out var nodeVm))
                return nodeVm;
            var node = Graph.GetNode(uid);
            if (node == null)
                return null;
            nodeVm = GetNodeVm(node);
            _debugNodesCacheDict.Add(uid, nodeVm);
            return nodeVm;
        }

        void OnNodeStatusChange(NodeStatusData nsd)
        {
            var nodeVm = _getDebugNodeViewModel(nsd.NodeUid);
            if (nodeVm == null)
                return;
            if (nsd.NewStatus == 2) // 2 is endrun
            {
                nodeVm.ChangeDebugStatus(nsd.result ? DebugNodeStatus.SUCCESS : DebugNodeStatus.FAILURE);
            }
            else if (nsd.NewStatus == 1)
            {
                nodeVm.ChangeDebugStatus(DebugNodeStatus.RUNNING);
            }
            else if(nsd.NewStatus == 0)
                nodeVm.ChangeDebugStatus(DebugNodeStatus.IDLE);
        }

        public void ContinueBreakPoint()
        {
            if (_graphInfo == null)
            {
                Logger.WARN("[debug] cannot continue breakpoint because graph is not connecting to a running graph");
                return;
            }
            ContainerLocator.Current.Resolve<IDebugService>().ContinueBreakPoint(_graphInfo);
        }
    }
}
