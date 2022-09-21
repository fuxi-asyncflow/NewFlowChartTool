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
    }

    public interface INetClient
    {
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

    }
}