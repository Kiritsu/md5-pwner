using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Md5Pwner.Services
{
    public class PwningGeneratorService : BackgroundService
    {
        private readonly PwnedWsService _service;
        private readonly Random _random;
        private readonly ILogger<PwningGeneratorService> _logger;

        public PwningGeneratorService(PwnedWsService service, Random random, ILogger<PwningGeneratorService> logger)
        {
            _service = service;
            _random = random;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                if (_service.PendingHashes.Count > 20)
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }

                var minLen = _service.End?.Length - 1 ?? 0;
                if (minLen <= 0)
                {
                    minLen = 1;
                }

                var str = GenerateString(minLen);
                var hash = ToHex(MD5.HashData(Encoding.ASCII.GetBytes(str))).ToLowerInvariant();
                _logger.LogDebug("Attempting to crack {Hash} -> {str} (len: {Len})", hash, str, minLen);
                _service.AddToQueue(hash);
            }
        }

        private string GenerateString(int size)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, size)
                .Select(x => x[_random.Next(x.Length)]).ToArray());
        }

        private static string ToHex(byte[] bytes)
        {
            var result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
            {
                result.Append(bytes[i].ToString("X2"));
            }

            return result.ToString();
        }
    }
}
