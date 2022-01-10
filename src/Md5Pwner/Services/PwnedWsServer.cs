using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebSocketSharp.Server;

namespace Md5Pwner.Services
{
    /// <summary>
    /// Represents a simple embedded WS server.
    /// </summary>
    public class PwnedWsServer
    {
        /// <summary>
        /// Gets the websocket server.
        /// </summary>
        public WebSocketServer Server { get; init; }

        /// <summary>
        /// Gets the current websocket clients.
        /// </summary>
        public List<PwnedWsSession> Sessions { get; init; }

        /// <summary>
        /// Creates an embedded websocket server.
        /// </summary>
        /// <param name="services">Services for dependency injection.</param>
        /// <param name="configuration">Configuration of the app.</param>
        public PwnedWsServer(IServiceProvider services, IConfiguration configuration) 
        {
            Sessions = new List<PwnedWsSession>();

            Server = new WebSocketServer($"ws://{configuration["WsServerHost"]}:{configuration["WsServerPort"]}");

            // since PwnedWsSession is transiant, a new instance of it is created everytime a client attempts to connect to the server.
            Server.AddWebSocketService("/ws", () => services.GetRequiredService<PwnedWsSession>());
        }
    }
}
