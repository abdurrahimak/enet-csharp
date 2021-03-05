using System;

namespace ENetCsharp
{
    public interface IHostClient
    {
        event Action Connected;
        event Action Disconnected;
        event Action<IProtocol> ProtocolReceived;

        AbstractClient GetClient();
        void Connect();
        void Destroy();
        void SendProtocol(IProtocol protocol, ENetCsharp.PacketFlags packetFlags = ENetCsharp.PacketFlags.None);
        void RegisterProtocolType(Type protocolType);
        void Disconnect(uint disconnectType);
        void Update();
    }
}
