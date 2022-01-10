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
    /// <summary>
    /// Represents a websocket service.
    /// </summary>
    public class PwnedWsService
    {
        private readonly PwnedWsServer _wsServer;
        private readonly PwnedContext _dbContext;
        private readonly ILogger<PwnedWsService> _logger;

        /// <summary>
        /// Gets the begin range tell the slaves from where to start brute forcing.
        /// </summary>
        public string Begin { get; private set; } = null!;

        /// <summary>
        /// Gets the end range tell the slaves from where to stop brute forcing.
        /// </summary>
        public string End { get; private set; } = null!;

        /// <summary>
        /// Gets the different hashes pending for being cracked.
        /// </summary>
        public List<Md5PendingHash> PendingHashes { get; init; }

        /// <summary>
        /// Gets the different known pwned hashes to be cached in the webpage.
        /// </summary>
        public List<Md5PwnedHash> PwnedHashes { get; init; }

        /// <summary>
        /// Creates a new websocket service.
        /// </summary>
        /// <param name="wsServer">Websocket server.</param>
        /// <param name="dbContext">LiteDB database context.</param>
        /// <param name="logger">Logger of the service.</param>
        public PwnedWsService(PwnedWsServer wsServer, PwnedContext dbContext, ILogger<PwnedWsService> logger)
        {
            _wsServer = wsServer;
            _dbContext = dbContext;
            _logger = logger;

            PendingHashes = new List<Md5PendingHash>();
            PwnedHashes = new List<Md5PwnedHash>();
        }

        /// <summary>
        /// Starts the websocket server.
        /// </summary>
        public void StartWs()
        {
            _wsServer.Server.Start();
        }

        /// <summary>
        /// Stops the websocket server.
        /// </summary>
        public void StopWs()
        {
            _wsServer.Server.Stop();
        }

        /// <summary>
        /// Gets the amount of slaves available.
        /// </summary>
        /// <returns></returns>
        public long GetSlaveCount()
        {
            return _wsServer.Sessions?.Count ?? 0;
        }

        /// <summary>
        /// Gets the amount of hashes that have been cracked and saved in the database.
        /// </summary>
        /// <returns></returns>
        public long GetCrackedHashesCount()
        {
            return _dbContext.Hashes.LongCount();
        }

        /// <summary>
        /// Uses docker-compose "--scale" flag to create or destroy MD5 cracking slaves.
        /// </summary>
        /// <param name="slaveCount">Amount of slaves to have.</param>
        public void ScaleSlaves(int slaveCount)
        {
            var workingDirectory = "/app";
            if (Directory.Exists("/app/md5pwner"))
            {
                // should always goes through this when using the current configuration.
                workingDirectory = "/app/md5pwner";
            }

            // invokes docker-compose executable. 
            var process = Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = workingDirectory,
                FileName = "docker-compose",
                Arguments = $"up -d --no-recreate --scale pwner={slaveCount}"
            });

            // block and wait for exit.
            process!.WaitForExit();
        }

        /// <summary>
        /// Sets the range to pass to the slaves when cracking up hashes.
        /// </summary>
        /// <param name="begin">Begin characters to start with.</param>
        /// <param name="end">End characters to end with.</param>
        public void SetRange(string begin, string end)
        {
            Begin = begin;
            End = end;
        }

        /// <summary>
        /// Aborts and remove a tag from the pending list.
        /// </summary>
        /// <param name="hash">Hash to abort.</param>
        public void Abort(string? hash)
        {
            if (string.IsNullOrEmpty(hash))
            {
                return;
            }

            PendingHashes.RemoveAll(x => x.Hash == hash);
        }

        /// <summary>
        /// Adds a md5 hash to crack to the queue.
        /// </summary>
        /// <param name="md5">MD5 hash to crack.</param>
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

        /// <summary>
        /// Saves a found solution to the database and the cache.
        /// </summary>
        /// <param name="hash">Hash that was resolved.</param>
        /// <exception cref="ArgumentNullException">Thrown when the passed hash is null.</exception>
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
