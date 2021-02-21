using System;
using System.Collections.Generic;
using System.Text;

namespace ENetCsharp
{
    public static class HostCreationFactory
    {
        public static IHostServer CreateENetServerHost(ServerOptions serverOptions)
        {
            return new ENetServerHost(serverOptions);
        }

        public static IHostClient CreateENetClientHost(ClientOptions clientOptions)
        {
            return new ENetClientHost(clientOptions);
        }
    }
}
