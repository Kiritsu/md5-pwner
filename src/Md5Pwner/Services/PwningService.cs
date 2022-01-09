using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Md5Pwner.Services
{
    public class PwningService : BackgroundService
    {
        private readonly PwnedWsService _service;
        private readonly PwnedWsServer _wsServer;

        public PwningService(PwnedWsService service, PwnedWsServer wsServer)
        {
            _service = service;
            _wsServer = wsServer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(2000, stoppingToken);

                var queue = _service.PendingHashes;
                var current = queue.FirstOrDefault(x => !x.Processing);
                if (current == null || current.Processing)
                {
                    continue;
                }

                current.Processing = true;
                _wsServer.Sessions.FirstOrDefault()?.SendSearch(current.Hash, "0", "15018569");
            }
        }
    }
}
