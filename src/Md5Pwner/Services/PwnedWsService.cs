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

        public string Begin { get; private set; }
        public string End { get; private set; }

        public List<Md5PendingHash> PendingHashes { get; init; }

        public List<Md5PwnedHash> PwnedHashes { get; init; }

        public PwnedWsService(PwnedWsServer wsServer, PwnedContext dbContext, ILogger<PwnedWsService> logger)
        {
            _wsServer = wsServer;
            _dbContext = dbContext;
            _logger = logger;

            PendingHashes = new List<Md5PendingHash>();
            PwnedHashes = new List<Md5PwnedHash>();
        }

        public void StartWs()
        {
            _wsServer.Server.Start();
        }

        public void StopWs()
        {
            _wsServer.Server.Stop();
        }

        public long GetSlaveCount()
        {
            return _wsServer.Sessions?.Count ?? 0;
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

        public void SetRange(string begin, string end)
        {
            Begin = begin;
            End = end;
        }

        public void AddToQueue(string md5)
        {
            if (string.IsNullOrWhiteSpace(md5))
            {
                return;
            }

            var existing = _dbContext.Hashes.FindOne(x => x.Hash == md5);
            if (existing is not null)
            {
                _logger.LogInformation("Hash {Hash} is already solved and present in database with value {Value}", md5, existing.Value);
                PwnedHashes.Add(existing);
                return;
            }

            PendingHashes.Add(new() { Hash = md5, InitiatedAt = DateTime.Now });
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
                PendingHashes.Remove(pendingEquivalent);
            }

            foreach (var session in _wsServer.Sessions)
            {
                session.SendStop();
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
