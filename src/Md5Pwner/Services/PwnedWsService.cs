using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Md5Pwner.Services
{
    public class PwnedWsService : BackgroundService
    {
        private readonly PwnedWsServer _wsServer;

        public PwnedWsService(PwnedWsServer wsServer)
        {
            _wsServer = wsServer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
