using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = FlowChart.Common.Logger;

namespace FlowChart.Debug
{
    public class DebugManager<TClient> : INetManager
        where TClient : INetClient, new()

    {
        public DebugManager()
        {
            _clients = new Dictionary<string, INetClient>();
            _agents = new Dictionary<Guid, DebugAgent>();
            _graphInfos = new Dictionary<Guid, GraphInfo>();
        }

        public static string MakeAddr(string host, int port)
        {
            return $"ws://{host}:{port}";
        }

        protected Dictionary<string, INetClient> _clients;
        protected Dictionary<Guid, DebugAgent> _agents;
        protected Dictionary<Guid, GraphInfo> _graphInfos;
        public INetClient? GetClient(string host, int port)
        {
            return _clients.TryGetValue(MakeAddr(host, port), out var client) ? client : null;
        }

        public async Task<INetClient?> Connect(string host, int port)
        {
            var client = new TClient();
            client.Init(this, host, port);
            var result = await client.Connect();
            if (!result)
                return null;

            _clients.Add(MakeAddr(host, port), client);
            return client;
        }

        public void BroadCast(string host, int startPort, int endPort, IDebugMessage msg)
        {
            
            for (int port = startPort; port <= endPort; port++)
            {
                var client = GetClient(host, port);
                if (client != null)
                {
                    client.Send(msg);
                }
                else
                {
                    ConnectAndSend(host, port, msg);
                }
            }
        }

        public async void ConnectAndSend(string host, int port, IDebugMessage? data)
        {
            var client = await Connect(host, port);
            client?.Send(data);
        }

        public void Send(string host, int port, IDebugMessage msg)
        {
            Task.Factory.StartNew(delegate { _sendAsync(host, port, msg); });
        }

        private async void _sendAsync(string host, int port, IDebugMessage msg)
        {
            
            var client = GetClient(host, port);
            if (client != null)
                client.Send(msg);
            else
            {
                ConnectAndSend(host, port, msg);
            }
        }

        public void RemoveClient(INetClient client)
        {
            string? key = null;
            foreach (var kv in _clients)
            {
                if (kv.Value == client)
                    key = kv.Key;
            }

            if (key != null)
            {
                _clients.Remove(key);
            }
        }

        public void HandleMessage(INetClient client, IDebugMessage data)
        {
            //Logger.DBG($"[ws] recv {msg}");
            //var data = Deserialize(msg);
            if (data is RecvGraphInfos recvGraphInfos)
            {
                foreach (var graphInfo in recvGraphInfos.GraphInfos)
                {
                    graphInfo.Host = client.Host;
                    graphInfo.Port = client.Port;
                    var graphUid = Guid.Parse(graphInfo.GraphUid);
                    if (!_graphInfos.ContainsKey(graphUid))
                    {
                        _graphInfos.Add(graphUid, graphInfo);
                    }
                    Logger.DBG($"[ws] graph: {graphInfo.GraphName}");
                }

                RecvGraphListEvent?.Invoke(client.Host, client.Port, recvGraphInfos.GraphInfos);
                return;
            }

            if (data is GraphDebugData graphDebugData)
            {
                if (!_agents.TryGetValue(graphDebugData.ChartUid, out var agent))
                {
                    GraphInfo? graphInfo;
                    if (!_graphInfos.TryGetValue(graphDebugData.ChartUid, out graphInfo))
                    {
                        // if graph begin after quick debug, _graphInfo is empty, so graphInfo will be null
                        // create an fake graphInfo here
                        graphInfo = new GraphInfo()
                        {
                            GraphName = graphDebugData.ChartName,
                            GraphUid = graphDebugData.ChartUid.ToString(),
                            Host = "127.0.0.1",
                            Port = 8888
                        };
                    }
                    agent = new DebugAgent()
                    {
                        GraphGuid = graphDebugData.ChartUid,
                        GraphName = graphDebugData.ChartName,
                        Info = graphInfo
                    };
                    _agents.Add(graphDebugData.ChartUid, agent);
                    ReplayFile.Inst.AddGraphInfo(graphInfo);
                    NewDebugAgentEvent?.Invoke(agent);
                }
                agent.Accept(graphDebugData.DebugDataList);
                ReplayFile.Inst.AddDebugData(graphDebugData);
                return;
            }

            if (data is GraphInfo gi) // quick debug start info
            {
                // set graphinfo to graphviewmodel
                NewDebugGraphEvent?.Invoke(gi);
                var graphUid = Guid.Parse(gi.GraphUid);
                if (!_graphInfos.ContainsKey(graphUid))
                {
                    _graphInfos.Add(graphUid, gi);
                }
            }
        }

        public void Stop()
        {
            ReplayFile.Inst.Stop();
            foreach (var kv in _clients)
            {
                kv.Value.Stop();
            }
            _agents.Clear();
        }

        #region events
        public event Action<string, int, List<GraphInfo>>? RecvGraphListEvent;
        public event Action<DebugAgent>? NewDebugAgentEvent;
        public event Action<GraphInfo>? NewDebugGraphEvent;
        #endregion


    }
}
