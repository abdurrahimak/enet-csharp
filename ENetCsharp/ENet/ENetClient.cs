namespace ENetCsharp
{
    internal class ENetClient : AbstractClient
    {
        private Peer _peer;

        public ENetClient(Peer peer) : this(peer.ID)
        {
            _peer = peer;
        }

        public ENetClient(uint networkId) : base(networkId)
        {
        }

        internal override void SendData(byte[] data)
        {
            var packet = default(ENetCsharp.Packet);
            packet.Create(data);
            _peer.Send(0, ref packet);
        }

        internal override void Disconnect(uint disconnectType = 0)
        {
            _peer.Disconnect(disconnectType);
        }
    }

}
