using System;
using System.Threading;
using System.Threading.Tasks;
using Md5Pwner.Database;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Md5Pwner.Services
{
    public class PwnedWsService
    {
        private readonly PwnedWsServer _wsServer;
        private readonly PwnedContext _dbContext;
        private readonly ILogger<PwnedWsService> _logger;

        public PwnedWsService(PwnedWsServer wsServer, PwnedContext dbContext, ILogger<PwnedWsService> logger)
        {
            _wsServer = wsServer;
            _dbContext = dbContext;
            _logger = logger;
        }

        public void StartWs()
        {
            _wsServer.Start();
        }

        public void StopWs()
        {
            _wsServer.Stop();
        }

        public long GetSlaveCount()
        {
            return _wsServer.ConnectedSessions;
        }

        public long GetCrackedHashesCount()
        {
            return _dbContext.Hashes.LongCount();
        }

        public void ScaleSlaves()
        {

        }

        public void CrackMd5()
        {

        }

        public void SaveSolution(Md5PwnedHash hash)
        {
            if (hash == null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            _logger.LogInformation("Saving solution {Solution} for hash {Hash}", hash.Value, hash.Hash);
            _dbContext.Hashes.Insert(hash);
        }
    }
}
