using System;

namespace ENetCsharp
{
    public interface IHostServer
    {
        event Action<AbstractClient> Connected;
        event Action<AbstractClient> Disconnected;
        event Action<AbstractClient, IProtocol> ProtocolReceived;

        void StartServer();
        void Destroy();
        void SendProtocolToPeer(uint clientId, IProtocol protocol, ENetCsharp.PacketFlags packetFlags = ENetCsharp.PacketFlags.None);
        void SendProtocolToPeer(AbstractClient client, IProtocol protocol, ENetCsharp.PacketFlags packetFlags = ENetCsharp.PacketFlags.None);
        void SendProtocolToAll(IProtocol protocol, ENetCsharp.PacketFlags packetFlags = ENetCsharp.PacketFlags.None);
        void RegisterProtocolType(Type protocolType);
        void DisconnectClient(AbstractClient client, uint disconnectID);
        void Update();
    }
}
