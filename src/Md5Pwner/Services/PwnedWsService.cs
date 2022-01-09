using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Md5Pwner.Database;
using Microsoft.Extensions.Logging;

namespace Md5Pwner.Services
{
    public class PwnedWsService
    {
        private readonly PwnedWsServer _wsServer;
        private readonly PwnedContext _dbContext;
        private readonly ILogger<PwnedWsService> _logger;

        public ConcurrentQueue<Md5PendingHash> PendingHashes { get; init; }

        public List<Md5PwnedHash> PwnedHashes { get; init; }

        public PwnedWsService(PwnedWsServer wsServer, PwnedContext dbContext, ILogger<PwnedWsService> logger)
        {
            _wsServer = wsServer;
            _dbContext = dbContext;
            _logger = logger;

            PendingHashes = new ConcurrentQueue<Md5PendingHash>();
            PwnedHashes = new List<Md5PwnedHash>();
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

        public void ScaleSlaves(int slaveCount)
        {
            var workingDirectory = "/app";
            if (Directory.Exists("/app/md5pwner"))
            {
                workingDirectory = "/app/md5pwner";
            }

            var process = Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = workingDirectory,
                FileName = "docker-compose",
                Arguments = $"up -d --no-recreate --scale pwner={slaveCount}"
            });

            process!.WaitForExit();
        }

        public void AddToQueue(string md5)
        {
            if (string.IsNullOrWhiteSpace(md5))
            {
                return;
            }

            PendingHashes.Enqueue(new() { Hash = md5, InitiatedAt = DateTime.Now });
        }

        public void SaveSolution(Md5PwnedHash hash)
        {
            if (hash == null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            var pendingEquivalent = PendingHashes.FirstOrDefault(x => x.Hash == hash.Hash);
            if (pendingEquivalent is not null)
            {
                hash.InitiatedAt = pendingEquivalent.InitiatedAt;
            }

            _logger.LogInformation("Saving solution {Solution} for hash {Hash}", hash.Value, hash.Hash);
            _dbContext.Hashes.Insert(hash);

            PwnedHashes.Add(hash);
            if (PwnedHashes.Count == 20)
            {
                PwnedHashes.RemoveAt(0);
            }
        }
    }
}
