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
                await Task.Delay(500, stoppingToken);

                if (_wsServer.Sessions.Count == 0)
                {
                    continue;
                }

                var queue = _service.PendingHashes;
                var current = queue.FirstOrDefault();
                if (current == null || current.Processing)
                {
                    continue;
                }

                current.Processing = true;
                _wsServer.Sessions.FirstOrDefault()!.SendSearch(current.Hash, _service.Begin.ToString(), _service.End.ToString());
            }
        }
    }
}
