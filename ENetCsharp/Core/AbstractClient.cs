namespace ENetCsharp
{
    public abstract class AbstractClient
    {
        public uint NetworkId;

        internal AbstractClient(uint networkId)
        {
            NetworkId = networkId;
        }

        internal abstract void SendData(byte[] data);
        internal abstract void Disconnect(uint disconnectType);
    }
}
