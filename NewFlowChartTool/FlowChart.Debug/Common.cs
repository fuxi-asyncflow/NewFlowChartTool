using FlowChart.Debug.WebSocket;

namespace FlowChart.Debug
{
    public interface IDebugMessage
    {
        public string Name { get; }
        public Dictionary<string, string> GetParams();
    }

    public interface IDebugProtocal<T>
    {
        public T Serialize(IDebugMessage msg);
        public object? Deserialize(T msg);
    }

    public interface INetClient
    {
        public string Host { get;}
        public int Port { get; }
        public void Send(string message);
        public void Send(byte[] data);
    }
    public interface INetManager
    {
        public INetClient? GetClient(string host, int port);
        public void RemoveClient(INetClient client);
        public Task<INetClient?> Connect(string host, int port);
        public void BroadCast(string host, int startPort, int endPort, IDebugMessage msg);
        public void Send(string host, int port, IDebugMessage msg);
        public void HandleMessage(INetClient client, string msg);

        #region events

        public delegate void RecvGraphListDelegate(string host, int port, List<GraphInfo> graphs);

        public event RecvGraphListDelegate? RecvGraphListEvent;


        #endregion


    }
}