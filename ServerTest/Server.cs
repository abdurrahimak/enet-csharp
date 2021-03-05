using ENetCsharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ServerTest
{
    class Server
    {
        static IHostServer _server;
        private static Stopwatch stopwatch = new Stopwatch();
        static int sleepMilisecond = (int)(1000f / 45f);
        private static List<AbstractClient> _clients = new List<AbstractClient>();

        static void Main(string[] args)
        {
            Console.WriteLine("Server application started.");
            InputListener.Start();

            InputListener.Register("conn", StartServer);
            InputListener.Register("dis", DestroyServer);
            InputListener.Register("disc", DisconnectClient);
            InputListener.Register("send", (commands) => { HandleProtocol.SendTest(_server, commands); });
            StartServer("conn", "6005");
            while (!InputListener.Quited)
            {
                stopwatch.Start();
                InputListener.Update();
                GameLoop();

                int sleepTime = sleepMilisecond - (int)stopwatch.ElapsedMilliseconds;
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
                Time.Update(stopwatch.ElapsedMilliseconds);
                stopwatch.Reset();
            }
        }

        private static void DisconnectClient(string[] commands)
        {
            if (commands.Length < 2)
            {
                Console.WriteLine("Wrong..");
                return;
            }
            uint clientId = uint.Parse(commands[1]);
            var client = _clients.Find(c => c.NetworkId == clientId);
            if (client != null)
            {
                Console.WriteLine($"Disconnect request sended to {clientId}");
                _server.DisconnectClient(client, 0);
            }
        }

        private static void DestroyServer(string[] obj)
        {
            _server.Connected -= Client_Connected;
            _server.Disconnected -= Client_Disconnected;
            _server.ProtocolReceived -= Client_ProcotolReceived;
            _server.Destroy();
        }

        private static void StartServer(params string[] obj)
        {
            ushort port = ushort.Parse(obj[1]);
            ServerOptions serverOptions = new ServerOptions(port, updateRate:10);
            _server = HostCreationFactory.CreateENetServerHost(serverOptions);
            _server.Connected += Client_Connected;
            _server.Disconnected += Client_Disconnected;
            _server.ProtocolReceived += Client_ProcotolReceived;
            _server.RegisterProtocolType(typeof(TestProtocol));
            _server.StartServer();
        }

        static void GameLoop()
        {
            _server?.Update();
        }

        private static void Client_ProcotolReceived(AbstractClient client, IProtocol protocol)
        {
            Console.WriteLine($"Protocol received from {client.NetworkId}: {protocol.GetType()}, {protocol.ToString()}");
        }

        private static void Client_Disconnected(AbstractClient client)
        {
            Console.WriteLine($"Disconnected {client.NetworkId}");
            _clients.Remove(client);
        }

        private static void Client_Connected(AbstractClient client)
        {
            Console.WriteLine($"Connected {client.NetworkId}");
            _clients.Add(client);
            HandleProtocol.SendTest(_server, _clients, new string[] { "send", "test" });
        }
    }

    public static class HandleProtocol
    {
        internal static void SendTest(IHostServer server, string[] commands)
        {
            if (commands.Length < 2)
            {
                Console.WriteLine("Wrong..");
                return;
            }
            Console.WriteLine("Protocol send request: " + commands[1]);

            if (commands[1] == "test")
            {
                for (int i = 0; i < 150; i++)
                {
                    TestProtocol testProtocol = new TestProtocol();
                    testProtocol.Text = "Server Test";
                    testProtocol.Number = i;
                    testProtocol.FloatNumber = 15.20f;
                    testProtocol.PlayerID = 313131;
                    server.SendProtocolToAll(testProtocol);
                }
            }
        }
        internal static void SendTest(IHostServer server, List<AbstractClient> clients, string[] commands)
        {
            if (commands.Length < 2)
            {
                Console.WriteLine("Wrong..");
                return;
            }
            Console.WriteLine("Protocol send request: " + commands[1]);

            if (commands[1] == "test")
            {
                foreach (var item in clients)
                {
                    for (int i = 0; i < 150; i++)
                    {
                        TestProtocol testProtocol = new TestProtocol();
                        testProtocol.Text = "Server Test";
                        testProtocol.Number = i;
                        testProtocol.FloatNumber = 15.20f;
                        testProtocol.PlayerID = item.NetworkId;
                        server.SendProtocolToPeer(item, testProtocol);
                    }
                }
            }
        }
    }
}
