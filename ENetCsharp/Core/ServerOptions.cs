﻿namespace ENetCsharp
{
    public struct ServerOptions
    {
        public ushort Port;
        public int MaxClient;
        public int UpdateRate;

        public ServerOptions(ushort port, int maxClient=10, int updateRate=30)
        {
            Port = port;
            MaxClient = maxClient;
            UpdateRate = updateRate;
        }
    }
}
