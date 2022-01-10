using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebSocketSharp.Server;

namespace Md5Pwner.Services
{
    public class PwnedWsServer
    {
        public WebSocketServer Server { get; init; }
        public List<PwnedWsSession> Sessions { get; init; }

        public PwnedWsServer(IServiceProvider services, IConfiguration configuration) 
        {
            Sessions = new List<PwnedWsSession>();

            Server = new WebSocketServer($"ws://{configuration["WsServerHost"]}:{configuration["WsServerPort"]}");
            Server.AddWebSocketService("/ws", () => services.GetRequiredService<PwnedWsSession>());
        }
    }
}
