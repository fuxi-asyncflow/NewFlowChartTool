using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Fluent;
using WebSocketSharp;
using WebSocketSharp.Net;
using Logger = FlowChartCommon.Logger;

namespace FlowChart.Debug.WebSocket
{
    public class Client : INetClient
    {
        public Client(INetManager manager, string host, int port)
        {
            _manager = manager;
            _host = host;
            _port = port;
            _addr = Manager.MakeAddr(_host, _port);
            _messages = new List<string>();

            _ws = new WebSocketSharp.WebSocket(_addr);
            _ws.OnOpen += OnOpen;
            _ws.OnMessage += OnMessage;
            _ws.OnError += OnError;
            _ws.OnClose += OnClose;
        }

        #region props
        WebSocketSharp.WebSocket _ws;
        private INetManager _manager;
        private string _host;
        private int _port;
        private string _addr;
        private List<string> _messages;
        private bool _isOpen;
        #endregion

        public void Send(string message)
        {
            if(_isOpen)
                _ws.SendAsync(message, b => { Logger.DBG($"[ws] send data: {b}, {message}"); });
            else
            {
                Logger.DBG("[ws] send data while not open");
                _messages.Add(message);
            }
        }

        public void Send(byte[] data)
        {
            throw new NotImplementedException();
        }

        TaskCompletionSource<bool> _connectFuture;

        public Task<bool> Connect()
        {
            _connectFuture = new TaskCompletionSource<bool>();
            _ws.ConnectAsync();
            return _connectFuture.Task;
        }

        #region websocket callbacks

        public void OnOpen(object? sender, EventArgs args)
        {
            Logger.LOG($"[ws] OnOpen for {_addr}");
            _isOpen = true;
            _connectFuture.TrySetResult(true);
        }

        public void OnMessage(object? sender, MessageEventArgs args)
        {
            if (args.IsBinary)
                return;
            Logger.DBG(args.Data);
        }

        public void OnError(object? sender, EventArgs args)
        {
            Logger.LOG($"[ws] OnError for {_addr}");
            _isOpen = false;
            _manager.RemoveClient(this);
            _connectFuture.TrySetResult(false);

        }

        public void OnClose(object? sender, EventArgs args)
        {
            Logger.LOG($"[ws] OnClose for {_addr}");
            _isOpen = false;
            _manager.RemoveClient(this);
            _connectFuture.TrySetResult(false);
        }

        #endregion
    }
}
