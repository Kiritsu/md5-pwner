using System;
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

        public PwnedWsServer(IServiceProvider services, ILogger<PwnedWsServer> logger, IConfiguration configuration) 
            : base(new IPEndPoint(IPAddress.Any, int.Parse(configuration["WsServerPort"])))
        {
            _services = services;
            _logger = logger;
        }

        protected override TcpSession CreateSession()
        {
            _logger.LogDebug("Initiating requested WS session");
            return _services.GetRequiredService<PwnedWsSession>();
        }
    }
}
