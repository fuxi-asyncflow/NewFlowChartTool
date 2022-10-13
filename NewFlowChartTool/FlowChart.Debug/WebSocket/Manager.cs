﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Net;
using FlowChartCommon;
using Logger = FlowChartCommon.Logger;

namespace FlowChart.Debug.WebSocket
{
    public class Manager : INetManager
    {
        public Manager()
        {
            _protocal = new JsonProtocol();
            _clients = new Dictionary<string, Client>();
            _agents = new Dictionary<Guid, DebugAgent>();
        }

        public static string MakeAddr(string host, int port)
        {
            return $"ws://{host}:{port}";
        }

        private Dictionary<string, Client> _clients;
        private IDebugProtocal<string> _protocal;
        private Dictionary<Guid, DebugAgent> _agents;
        public INetClient? GetClient(string host, int port)
        {
            Client? client = null;
            if (_clients.TryGetValue(MakeAddr(host, port), out client))
                return client;
            return null;
        }

        public async Task<INetClient?> Connect(string host, int port)
        {
            var client = new Client(this, host, port);
            var result = await client.Connect();
            if (!result)
                return null;
            
            _clients.Add(MakeAddr(host, port), client);
            return client;
        }

        public void BroadCast(string host, int startPort, int endPort, IDebugMessage msg)
        {
            var data = _protocal.Serialize(msg);
            for (int port = startPort; port <= endPort; port++)
            {
                var client = GetClient(host, port);
                if (client != null)
                {
                    client.Send(data);
                }
                else
                {
                    ConnectAndSend(host, port, data);
                }
            }
        }

        public async void ConnectAndSend(string host, int port, string data)
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
            var data = _protocal.Serialize(msg);
            var client = GetClient(host, port);
            if (client != null)
                client.Send(data);
            else
            {
                var c = await Connect(host, port);
                if(c != null)
                    c.Send(data);
            }
        }

        public void RemoveClient(INetClient client)
        {
            string? key = null;
            foreach (var kv in _clients)
            {
                if(kv.Value == client)
                    key = kv.Key;
            }

            if (key != null)
            {
                _clients.Remove(key);
            }
        }

        public void HandleMessage(INetClient client, string msg)
        {
            var data = _protocal.Deserialize(msg);
            if (data is List<GraphInfo> graphInfos)
            {
                foreach (var graphInfo in graphInfos)
                {
                    graphInfo.Host = client.Host;
                    graphInfo.Port = client.Port;
                    Logger.DBG($"[ws] graph: {graphInfo.GraphName}");
                }

                RecvGraphListEvent?.Invoke(client.Host, client.Port, graphInfos);
            }

            if (data is GraphDebugData graphDebugData)
            {
                if (!_agents.TryGetValue(graphDebugData.ChartUid, out var agent))
                {
                    agent = new DebugAgent()
                    {
                        GraphGuid = graphDebugData.ChartUid, 
                        GraphName = graphDebugData.ChartName
                    };
                    _agents.Add(graphDebugData.ChartUid, agent);
                    NewDebugAgentEvent?.Invoke(agent);
                }
                agent.Accept(graphDebugData.DebugDataList);
            }
        }

        public void Stop()
        {
            foreach (var kv in _clients)
            {
                kv.Value.Stop();
            }
            _agents.Clear();
        }

        #region events
        public event INetManager.RecvGraphListDelegate? RecvGraphListEvent;
        public event INetManager.NewDebugAgentDelegate? NewDebugAgentEvent;
        #endregion


    }
}