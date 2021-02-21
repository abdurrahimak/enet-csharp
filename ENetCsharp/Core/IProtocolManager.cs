using System;

namespace ENetCsharp
{
    public interface IProtocolManager
    {
        void RegisterProtocolType(Type protocolType);
        Type GetProtocolType(int id);
        IProtocol GetProtocolInstance(int id);
        IProtocol GetProtocolInstance(Type type);
        int GetProtocolID(Type type);
        int GetProtocolID(IProtocol protocol);
        void Destroy();
    }
}
