﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using FlowChart.Core;
using FlowChart.Debug;
using FlowChartCommon;
using NFCT.Common;
using NFCT.Common.Events;
using NFCT.Common.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace NewFlowChartTool.ViewModels
{
    class GraphInfoViewModel : BindableBase
    {
        public GraphInfoViewModel(GraphInfo gi)
        {
            GraphInfo = gi;
        }

        private int _rowId;
        public int RowId
        {
            get => _rowId;
            set => SetProperty(ref _rowId, value);
        }
        public GraphInfo GraphInfo { get; set; }
        public string GraphPath => GraphInfo.GraphName;
        public string ObjectName => GraphInfo.ObjectName;

        public string OwnerGraphPath => string.IsNullOrEmpty(GraphInfo.OwnerGraphName)
            ? "-"
            : $"{GraphInfo.OwnerGraphName}[{GraphInfo.OwnerNodeId}]";
    }

    class DebugDialogViewModel : BindableBase, IDialogAware, IDebugService
    {
        public DebugDialogViewModel(IDialogService dialogService)
        {
            if (Inst != null)
            {
                Logger.ERR("DebugDialogViewModel should be singleton");
            }

            Inst = this;
            Test = "Hello world";
            GraphList = new ObservableCollection<GraphInfoViewModel>();
            _dialogService = dialogService;
            CloseCommand = new DelegateCommand(() => { RequestClose.Invoke(new DialogResult(ButtonResult.OK)); });
            GetGraphListCommand = new DelegateCommand(GetGraphList);
            StartPort = 9000;
            EndPort = 9003;
            Host = "127.0.0.1";

            _netManager = new FlowChart.Debug.WebSocket.Manager();
            _agents = new Dictionary<string, List<DebugAgent>>();

            _netManager.RecvGraphListEvent += (host, ip, graphList) =>
            {
                EventHelper.Pub<RecvGraphListEvent, List<GraphInfo>>(graphList);
            };
            EventHelper.Sub<RecvGraphListEvent, List<GraphInfo>>(OnRecvGraphListEvent, ThreadOption.UIThread);

            _netManager.NewDebugAgentEvent += (agent) =>
            {
                EventHelper.Pub<NewDebugAgentEvent, DebugAgent>(agent);
            };
            EventHelper.Sub<NewDebugAgentEvent, DebugAgent>(OnNewDebugAgentEvent, ThreadOption.UIThread);
        }

        private IDialogService _dialogService;
        public static DebugDialogViewModel? Inst { get; set; }

        public const string NAME = "DebugDialog";
        public string Test { get; set; }
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            IsOpened = false;
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            IsOpened = true;
        }

        public string Title => "Debug Dialog";
        public ObservableCollection<GraphInfoViewModel> GraphList { get; set; }
        private GraphInfoViewModel? _selectedGraphInfo;
        public GraphInfoViewModel? SelectedGraphInfo
        {
            get => _selectedGraphInfo;
            set => SetProperty(ref _selectedGraphInfo, value);
        }

        public event Action<IDialogResult>? RequestClose;

        public DelegateCommand CloseCommand { get; set;}
        public DelegateCommand GetGraphListCommand { get; set; }
        
        private INetManager _netManager;
        private Dictionary<string, List<DebugAgent>> _agents;
        public bool IsOpened { get; set; }
        private string _host;
        public string Host { get => _host; set => SetProperty(ref _host, value); }
        private int _startPort;
        public int StartPort { get => _startPort; set => SetProperty(ref _startPort, value); }
        private int _endPort;
        public int EndPort { get => _endPort; set => SetProperty(ref _endPort, value); }

        public void GetGraphList()
        {
            GraphList.Clear();
          
            _netManager.BroadCast(Host, StartPort, EndPort, new GetChartListMessage(){ChartName = "", ObjectName = ""});
        }

        private void OnRecvGraphListEvent(List<GraphInfo> graphs)
        {
            graphs.ForEach(gi => GraphList.Add(new GraphInfoViewModel(gi) {RowId = GraphList.Count + 1}));
        }

        public List<DebugAgent>? GetAgents(string graphName)
        {
            if (!_agents.ContainsKey(graphName))
                return null;
            return _agents[graphName];
        }

        private void OnNewDebugAgentEvent(DebugAgent agent)
        {
            if (!_agents.ContainsKey(agent.GraphName))
            {
                _agents.Add(agent.GraphName, new List<DebugAgent>());
            }
            var list = _agents[agent.GraphName];
            Debug.Assert(list.Find(a => a.GraphGuid == agent.GraphGuid) == null);
            list.Add(agent);
        }

        public void StartDebugGraph(GraphInfo graphInfo)
        {
            _netManager.Send(graphInfo.Host, graphInfo.Port, new StartDebugChartMessage(graphInfo));
        }

        public void SetBreakPoint(Node node, bool isBreakPoint)
        {
            _netManager.BroadCast(Host, StartPort, EndPort, new SetBreakPointMessage()
            {
                ChartName = node.OwnerGraph.Path, NodeUid = node.Uid, Command = isBreakPoint
            });
        }

        public void ContinueBreakPoint(GraphInfo graphInfo)
        {
            _netManager.Send(graphInfo.Host, graphInfo.Port, new ContinueBreakPointMessage(graphInfo));
        }

        public void StopDebug()
        {
            _netManager.Stop();
            _agents.Clear();
        }
    }
}