namespace ENetCsharp
{
    public struct ClientOptions
    {
        public string IP;
        public ushort Port;
        public int UpdateRate;

        public ClientOptions(string ip, ushort port, int updateRate = 30)
        {
            IP = ip;
            Port = port;
            UpdateRate = updateRate;
        }
    }
}
