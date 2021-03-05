using System;
using ENetCsharp;
using System.Threading;
using System.Diagnostics;

namespace ClientTest
{
    class Client
    {
        static IHostClient _client;
        private static Stopwatch stopwatch = new Stopwatch();
        static int sleepMilisecond = (int)(1000f / 30f);

        static void Main(string[] args)
        {
            Console.WriteLine("Client application started.");
            InputListener.Start();

            InputListener.Register("conn", StartClient);
            InputListener.Register("dis", Disconnect);
            InputListener.Register("destroy", DestroyClient);
            InputListener.Register("send", (commands) =>
            {
                HandleProtocol.SendTest(_client, commands);
            });

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

        private static void Disconnect(string[] obj)
        {
            _client.Disconnect(0);
        }

        private static void DestroyClient(string[] obj)
        {
            _client.Connected -= Client_Connected;
            _client.Disconnected -= Client_Disconnected;
            _client.ProtocolReceived -= Client_ProcotolReceived;
            _client.Destroy();
        }

        private static void StartClient(string[] obj)
        {
            ushort port = ushort.Parse(obj[1]);
            ClientOptions clientOptions = new ClientOptions("127.0.0.1", port);
            _client = HostCreationFactory.CreateENetClientHost(clientOptions);
            _client.Connected += Client_Connected;
            _client.Disconnected += Client_Disconnected;
            _client.ProtocolReceived += Client_ProcotolReceived;
            _client.RegisterProtocolType(typeof(TestProtocol));

            _client.Connect();
        }

        static void GameLoop()
        {
            _client?.Update();
        }

        private static void Client_ProcotolReceived(IProtocol protocol)
        {
            Console.WriteLine("Protocol received: " + protocol.GetType() + ", " + protocol.ToString());
            //Console.WriteLine($"F: {DateTime.Now.Second} {DateTime.Now.Millisecond}");
        }

        private static void Client_Disconnected()
        {
            Console.WriteLine("Disconnected");
        }

        private static void Client_Connected()
        {
            Console.WriteLine("Connected");
        }
    }

    public static class HandleProtocol
    {
        internal static void SendTest(IHostClient client, string[] commands)
        {
            if (commands.Length < 2)
            {
                Console.WriteLine("Wrong..");
                return;
            }
            Console.WriteLine("Protocol send request: " + commands[1]);

            if (commands[1] == "test")
            {
                for (int i = 0; i < 30; i++)
                {
                    TestProtocol testProtocol = new TestProtocol();
                    testProtocol.Text = "Client Test";
                    testProtocol.Number = 150;
                    testProtocol.FloatNumber = 152.500f;
                    client.SendProtocol(testProtocol);
                }
            }
        }
    }
}
