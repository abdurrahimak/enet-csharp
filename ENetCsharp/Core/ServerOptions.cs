namespace ENetCsharp
{
    public struct ServerOptions
    {
        public ushort Port;
        public int MaxClient;
    }

    public struct ClientOptions
    {
        public string IP;
        public ushort Port;
    }
}
