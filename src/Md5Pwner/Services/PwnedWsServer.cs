using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCoreServer;

namespace Md5Pwner.Services
{
    public class PwnedWsServer : WsServer
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<PwnedWsServer> _logger;

        public new List<PwnedWsSession> Sessions { get; init; }

        public PwnedWsServer(IServiceProvider services, ILogger<PwnedWsServer> logger, IConfiguration configuration) 
            : base(new IPEndPoint(IPAddress.Any, int.Parse(configuration["WsServerPort"])))
        {
            _services = services;
            _logger = logger;

            Sessions = new List<PwnedWsSession>();
        }

        protected override TcpSession CreateSession()
        {
            _logger.LogDebug("Initiating requested WS session");

            var session = _services.GetRequiredService<PwnedWsSession>();
            Sessions.Add(session);
            return session;
        }
    }
}
