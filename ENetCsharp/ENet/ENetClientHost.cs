using System;
using System.Threading;
using System.Threading.Tasks;

namespace ENetCsharp
{
    internal class ENetClientHost : IHostClient
    {
        private IProtocolManager _protocolManager;
        private Host _client;
        private Peer _peer;
        private ClientOptions _clientOptions;
        private bool _connected = false;
        private PeerState _peerState;
        private CancellationTokenSource _cancellationTokenSource;
        private Event netEvent;

        private const int _channelID = 0;

        public event Action Connected;
        public event Action Disconnected;
        public event Action<IProtocol> ProtocolReceived;

        public ENetClientHost(ClientOptions clientOptions)
        {
            _clientOptions = clientOptions;
            _protocolManager = new ProtocolManager();
            _peerState = PeerState.Uninitialized;
        }

        public void StartClient()
        {
            try
            {
                if (_connected)
                {
                    throw new Exception("Already connected..");
                }

                ENetCsharp.Library.Initialize();
                _client = new Host();
                Address address = new Address();

                address.SetHost(_clientOptions.IP);
                address.Port = _clientOptions.Port;
                _client.Create();
                Console.WriteLine($"Connecting to {_clientOptions.IP}:{_clientOptions.Port}");
                _peer = _client.Connect(address);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Destroy()
        {
            _client?.Dispose();
            _client = null;
            Library.Deinitialize();
        }

        public void Disconnect(uint disconnectType = 0)
        {
            if (_connected)
                _peer.Disconnect(disconnectType);
        }

        public void RegisterProtocolType(Type protocolType)
        {
            _protocolManager.RegisterProtocolType(protocolType);
        }

        public void SendProtocol(IProtocol protocol, PacketFlags packetFlags = PacketFlags.None)
        {
            if (_connected)
            {
                using (PacketOrganizer pW = new PacketOrganizer(_protocolManager.GetProtocolID(protocol)))
                {
                    protocol.Write(pW);
                    byte[] data = pW.ToArray();
                    var p = default(Packet);
                    p.Create(data);
                    _peer.Send(_channelID, ref p);
                }
            }
        }

        private void HandleEvent(ref Event netEvent)
        {
            switch (netEvent.Type)
            {
                case EventType.None:
                    break;

                case EventType.Connect:
                    Console.WriteLine("Client connected to server - ID: " + _peer.ID);
                    OnClientConnected();
                    break;

                case EventType.Disconnect:
                    Console.WriteLine("Client disconnected from server");
                    OnClientDisconnected();
                    break;

                case EventType.Timeout:
                    Console.WriteLine("Client connection timeout");
                    OnClientDisconnected();
                    break;

                case EventType.Receive:
                    //Console.WriteLine("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                    OnHandlePacket(ref netEvent);
                    netEvent.Packet.Dispose();
                    break;
            }
        }

        private void OnClientConnected()
        {
            _peer.Timeout(32, 1000, 4000);
            _connected = true;
            Connected?.Invoke();
        }

        private void OnClientDisconnected()
        {
            _connected = false;
            Disconnected?.Invoke();
        }

        private void OnPeerStateChanged(PeerState peerState)
        {
            if (peerState == PeerState.Connecting)
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                }
                _cancellationTokenSource = new CancellationTokenSource();
                var task = Task.Factory.StartNew(() => { return CheckConnected(5000); }, _cancellationTokenSource.Token);
                task.ContinueWith((result) =>
                {
                    if (result.Result == PeerState.Connected)
                    {
                        OnClientConnected();
                    }
                    else
                    {
                        OnClientDisconnected();
                    }
                });
            }
            else if (peerState == PeerState.Disconnecting)
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                }
                _cancellationTokenSource = new CancellationTokenSource();
                var task = Task.Factory.StartNew(() => { return CheckConnected(3000); }, _cancellationTokenSource.Token);
                task.ContinueWith((result) =>
                {
                    if (result.Result == PeerState.Connected)
                    {
                        OnClientConnected();
                    }
                    else
                    {
                        OnClientDisconnected();
                    }
                });
            }
            _peerState = peerState;
        }

        private PeerState CheckConnected(int timeout)
        {
            if (_client.Service(timeout, out netEvent) > 0 && netEvent.NativeData.type == EventType.Connect)
            {
                return PeerState.Connected;
            }
            else
            {
                return PeerState.Disconnected;
            }
        }

        private void OnHandlePacket(ref Event netEvent)
        {
            var readBuffer = new byte[netEvent.Packet.Length];
            netEvent.Packet.CopyTo(readBuffer);
            IProtocol protocol;
            using (PacketOrganizer packet = new PacketOrganizer(readBuffer))
            {
                Type type = _protocolManager.GetProtocolType(packet.ReadInt());
                protocol = (IProtocol)Activator.CreateInstance(type);
                protocol.Read(packet);
            }
            ProtocolReceived?.Invoke(protocol);
        }

        public void Update()
        {
            if (_client == null)
                return;
            if (_peerState != _peer.State)
                OnPeerStateChanged(_peer.State);

            if (_peerState != PeerState.Connected)
                return;

            while (true)
            {
                var res = _client.Service(0, out netEvent);
                if (res > 0)
                {
                    //received packet
                    HandleEvent(ref netEvent);
                }
                else
                {
                    if (res < 0)
                    {
                        Console.WriteLine("Encountered error while polling.");
                    }
                    break;
                }
            }

            _client.Flush();
        }
    }

}
