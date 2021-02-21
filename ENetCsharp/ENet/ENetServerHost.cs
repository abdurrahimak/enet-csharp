using System;
using System.Collections.Generic;

namespace ENetCsharp
{
    internal class ENetServerHost : IHostServer
    {
        private IProtocolManager _protocolManager;
        private Host _server;
        private ServerOptions _serverOptions;
        private bool _isSuccessfullyCreated;
        private Dictionary<uint, AbstractClient> _clientsById;
        Event netEvent;

        public event Action<AbstractClient> Connected;
        public event Action<AbstractClient> Disconnected;
        public event Action<AbstractClient, IProtocol> ProtocolReceived;

        public ENetServerHost(ServerOptions serverOptions)
        {
            _isSuccessfullyCreated = false;
            _clientsById = new Dictionary<uint, AbstractClient>();
            _protocolManager = new ProtocolManager();
            _serverOptions = serverOptions;
        }

        public void StartServer()
        {
            try
            {
                Library.Initialize();
                _server = new Host();
                Address address = new Address();
                address.Port = _serverOptions.Port;
                _server.Create(address, _serverOptions.MaxClient);
                _isSuccessfullyCreated = true;
                Console.WriteLine($"Circle ENet Server started on {_serverOptions.Port}");
            }
            catch (Exception ex)
            {
                _isSuccessfullyCreated = false;
                Console.WriteLine(ex.ToString());
            }
        }

        public void Destroy()
        {
            _clientsById?.Clear();
            _protocolManager?.Destroy();
            _server?.Dispose();
            Library.Deinitialize();
            _isSuccessfullyCreated = false;
        }

        public void DisconnectClient(AbstractClient client, uint disconnectID)
        {
            if (client != null)
            {
                client.Disconnect(disconnectID);
            }
        }

        public void RegisterProtocolType(Type protocolType)
        {
            _protocolManager.RegisterProtocolType(protocolType);
        }

        public void SendProtocolToPeer(uint clientId, IProtocol protocol, ENetCsharp.PacketFlags packetFlags = ENetCsharp.PacketFlags.None)
        {
            if (_clientsById.ContainsKey(clientId))
            {
                SendProtocolToPeer(_clientsById[clientId], protocol);
            }
        }

        public void SendProtocolToPeer(AbstractClient client, IProtocol protocol, ENetCsharp.PacketFlags packetFlags = ENetCsharp.PacketFlags.None)
        {
            using (PacketOrganizer packet = new PacketOrganizer(_protocolManager.GetProtocolID(protocol)))
            {
                protocol.Write(packet);
                byte[] buffer = packet.ToArray();
                client.SendData(buffer);
            }
        }

        public void SendProtocolToAll(IProtocol protocol, ENetCsharp.PacketFlags packetFlags = ENetCsharp.PacketFlags.None)
        {
            using (PacketOrganizer packet = new PacketOrganizer(_protocolManager.GetProtocolID(protocol)))
            {
                protocol.Write(packet);
                byte[] buffer = packet.ToArray();
                var p = default(ENetCsharp.Packet);
                p.Create(buffer);
                _server.Broadcast(0, ref p);
            }
        }

        private void HandleEvent(ref Event netEvent)
        {
            switch (netEvent.Type)
            {
                case EventType.None:
                    break;

                case EventType.Connect:
                    Console.WriteLine("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                    OnClientConnected(netEvent.Peer);
                    break;

                case EventType.Disconnect:
                    Console.WriteLine("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                    OnClientDisconnected(netEvent.Peer);
                    break;

                case EventType.Timeout:
                    Console.WriteLine("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                    OnClientDisconnected(netEvent.Peer);
                    break;

                case EventType.Receive:
                    //Console.WriteLine("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                    //HandlePacket(ref netEvent);
                    OnHandlePacket(ref netEvent);
                    netEvent.Packet.Dispose();
                    break;
            }
        }

        private void OnClientConnected(Peer peer)
        {
            peer.Timeout(32, 1000, 4000);
            AbstractClient client = new ENetClient(peer);
            _clientsById.Add(peer.ID, client);
            Connected?.Invoke(client);
        }

        private void OnClientDisconnected(Peer peer)
        {
            if (_clientsById.ContainsKey(peer.ID))
            {
                AbstractClient client = _clientsById[peer.ID];
                _clientsById.Remove(peer.ID);
                Disconnected?.Invoke(client);
            }
        }

        private void OnHandlePacket(ref Event netEvent)
        {
            if (!_clientsById.ContainsKey(netEvent.Peer.ID))
            {
                Console.WriteLine("Packet received but server does not have the client");
                return;
            }
            var readBuffer = new byte[netEvent.Packet.Length];
            netEvent.Packet.CopyTo(readBuffer);
            IProtocol protocol;
            using (PacketOrganizer packet = new PacketOrganizer(readBuffer))
            {
                Type type = _protocolManager.GetProtocolType(packet.ReadInt());
                protocol = (IProtocol)Activator.CreateInstance(type);
                protocol.Read(packet);
            }
            ProtocolReceived?.Invoke(_clientsById[netEvent.Peer.ID], protocol);
        }

        public void Update()
        {
            if (!_isSuccessfullyCreated)
                return;
            
            while (true)
            {
                var res = _server.Service(0, out netEvent);
                if (res > 0)
                {
                    //received packet
                    HandleEvent(ref netEvent);
                }
                else
                {
                    if(res < 0)
                    {
                        Console.WriteLine("Encountered error while polling.");
                    }
                    break;
                }
            }

            _server.Flush();
        }
    }
}
