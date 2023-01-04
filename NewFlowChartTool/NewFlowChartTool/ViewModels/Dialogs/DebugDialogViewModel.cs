using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FlowChart.Core;
using FlowChart.Debug;
using FlowChart.Misc;
using FlowChart.Common;
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
        public DebugDialogViewModel(IDialogService dialogService, IOutputMessage outputService)
        {
            if (Inst != null)
            {
                Logger.ERR("DebugDialogViewModel should be singleton");
            }

            Inst = this;
            Test = "Hello world";
            GraphList = new ObservableCollection<GraphInfoViewModel>();
            _dialogService = dialogService;
            _outputService = outputService;
            CloseCommand = new DelegateCommand(() => { RequestClose.Invoke(new DialogResult(ButtonResult.OK)); });
            GetGraphListCommand = new DelegateCommand(GetGraphList);
            OpenReplayFileCommand = new DelegateCommand(ChooseReplayFile);
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

            _netManager.NewDebugGraphEvent += (graphInfo) =>
            {
                EventHelper.Pub<StartDebugGraphEvent, GraphInfo?>(graphInfo);
            };
        }

        private IDialogService _dialogService;
        private IOutputMessage _outputService;
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
        public DelegateCommand OpenReplayFileCommand { get; set; }
        
        private INetManager _netManager;
        private Dictionary<string, List<DebugAgent>> _agents;
        public bool IsOpened { get; set; }
        private string _host;
        public string Host { get => _host; set => SetProperty(ref _host, value); }
        private int _startPort;
        public int StartPort { get => _startPort; set => SetProperty(ref _startPort, value); }
        private int _endPort;
        public int EndPort { get => _endPort; set => SetProperty(ref _endPort, value); }
        public bool IsReplay { get; set; }

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
            EventHelper.Pub<StartDebugGraphEvent, GraphInfo?>(graphInfo);
            _netManager.Send(graphInfo.Host, graphInfo.Port, new StartDebugChartMessage(graphInfo));
        }

        public void SetBreakPoint(Node node, bool isBreakPoint)
        {
            _netManager.BroadCast(Host, StartPort, EndPort, new SetBreakPointMessage()
            {
                ChartName = node.OwnerGraph.Path, NodeUid = node.Uid.ToString(), Command = isBreakPoint
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
            IsReplay = false;
        }

        public void QuickDebug(Graph graph)
        {
            Task.Factory.StartNew(delegate { _quickDebug(graph); });
        }

        private async void _quickDebug(Graph graph)
        {
            // first try to find running graph
            _netManager.BroadCast(Host, StartPort, EndPort, new GetChartListMessage() { ChartName = graph.Path, ObjectName = "" });
            await Task.Delay(1000); // wait 1000ms for graph list

            if (GraphList.Count > 0)
            {
                StartDebugGraph(GraphList.First().GraphInfo);
                return;
            }

            // if no running graph, then wait for running graph
            _outputService.Output("quick debug recv no running graph, waiting graph start running");
            _netManager.BroadCast(Host, StartPort, EndPort, new QuickDebugMessage() { ChartName = graph.Path });
            EventHelper.Pub<StartDebugGraphEvent, GraphInfo?>(null);
        }

        public void Hotfix(List<string> lines, List<string> codes)
        {
            lines.Insert(0, "---");
            lines.Add("...");
            var chartData = string.Join('\n', lines);
            var chartDataBytes = System.Text.Encoding.UTF8.GetBytes(chartData);
            chartData = System.Convert.ToBase64String(chartDataBytes);

            var codesData = string.Join('\n', codes);
            var codeDataBytes = System.Text.Encoding.UTF8.GetBytes(codesData);
            codesData = System.Convert.ToBase64String(codeDataBytes);
            _netManager.BroadCast(Host, StartPort, EndPort, new HotfixMessage() {ChartsData = chartData, ChartsFunc = codesData });
        }

        #region REPLAY
        void ChooseReplayFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".dat";
            dialog.Filter = "Replay Files (.dat)|*.dat";
            dialog.InitialDirectory = FileHelper.GetFolder("debug");

            var result = dialog.ShowDialog();
            if (result != true)
                return;
            var fileName = dialog.FileName;

            // load file
            IsReplay = true;
            var replayFile = ReplayFile.Inst;
            GraphList.Clear();
            replayFile.Load(fileName);
            OnRecvGraphListEvent(replayFile.GraphInfoDict.Values.ToList());
        }

        public void StartReplay(GraphInfo graphInfo)
        {
            var replayFile = ReplayFile.Inst;
            EventHelper.Pub<StartDebugGraphEvent, GraphInfo?>(graphInfo);

            var agent = replayFile.GetAgent(graphInfo);
            EventHelper.Pub<NewDebugAgentEvent, DebugAgent>(agent);
        }

        #endregion

    }
}
