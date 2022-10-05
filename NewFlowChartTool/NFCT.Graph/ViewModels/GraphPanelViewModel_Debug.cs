using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Debug;

namespace NFCT.Graph.ViewModels
{
    public partial class GraphPaneViewModel
    {
        private bool _isDebugMode;
        public bool IsDebugMode { get => _isDebugMode; set => SetProperty(ref _isDebugMode, value); }

        public void EnterDebugMode()
        {
            if (IsDebugMode)
                return;
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
            foreach (var baseNodeViewModel in Nodes)
            {
                baseNodeViewModel.ChangeDebugMode();
            }
        }

        private DebugAgent? _currentDebugAgent { get; set; }
        private List<DebugAgent>? _agents;
        private Dictionary<Guid, BaseNodeViewModel> _debugNodesCacheDict = new Dictionary<Guid, BaseNodeViewModel>();

        public void UpdateAgents(List<DebugAgent>? agents)
        {
            _agents ??= agents;
            if (_currentDebugAgent == null && _agents != null && _agents.Count > 0)
            {
                _currentDebugAgent = _agents.First();
                _currentDebugAgent.NodeStatusChange += OnNodeStatusChange;
            }
        }

        BaseNodeViewModel? _getDebugNodeViewModel(Guid uid)
        {
            if (_debugNodesCacheDict.TryGetValue(uid, out var nodeVm))
                return nodeVm;
            var node = Graph.GetNode(uid.ToString("N"));
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
            if (nsd.NewStatus == 2)
            {
                if(nsd.result)
                    nodeVm.ChangeDebugCount(nodeVm.SuccessCount + 1, nodeVm.FailureCount);
                else
                    nodeVm.ChangeDebugCount(nodeVm.SuccessCount, nodeVm.FailureCount + 1);
            }
        }
    }
}
