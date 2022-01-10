using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Md5Pwner.Services
{
    /// <summary>
    /// Represents a background service that attempts to ask clients to resolves the different hashes when possible.
    /// </summary>
    public class PwningService : BackgroundService
    {
        private readonly PwnedWsService _service;
        private readonly PwnedWsServer _wsServer;

        private static readonly List<char> _alphabet = new()
        {
            'a',
            'b',
            'c',
            'd',
            'e',
            'f',
            'g',
            'h',
            'i',
            'j',
            'k',
            'l',
            'm',
            'n',
            'o',
            'p',
            'q',
            'r',
            's',
            't',
            'u',
            'v',
            'w',
            'x',
            'y',
            'z',
            'A',
            'B',
            'C',
            'D',
            'E',
            'F',
            'G',
            'H',
            'I',
            'J',
            'K',
            'L',
            'M',
            'N',
            'O',
            'P',
            'Q',
            'R',
            'S',
            'T',
            'U',
            'V',
            'W',
            'X',
            'Y',
            'Z',
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9'
        };

        public PwningService(PwnedWsService service, PwnedWsServer wsServer)
        {
            _service = service;
            _wsServer = wsServer;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(500, stoppingToken);

                var sessionCount = _wsServer.Sessions.Count;
                if (sessionCount == 0)
                {
                    // if no slave are available, just yield...
                    continue;
                }

                var queue = _service.PendingHashes;
                var current = queue.FirstOrDefault();
                if (current == null || current.Processing)
                {
                    // if we already have an hash that is being processed, just yield...
                    continue;
                }

                // marks the hash as being processed.
                current.Processing = true;

                // load-balances the different slaves to equally brute-force the hash.
                var beginNumber = GetNumber(_service.Begin);
                var endNumber = GetNumber(_service.End);

                var perSession = endNumber / sessionCount;
                var lastEnd = beginNumber;
                for (var i = 0; i < sessionCount; i++)
                {
                    // send its instruction to each available slave.
                    _wsServer.Sessions[i].SendSearch(current.Hash, lastEnd.ToString(), (lastEnd + perSession).ToString());
                    lastEnd += perSession;
                }
            }
        }

        /// <summary>
        /// Gets the number for the slave to search properly within a range depending on the given string.
        /// </summary>
        /// <param name="word">String to transform to a number.</param>
        public static long GetNumber(string word)
        {
            var number = 0L;
            for (var i = word.Length - 1; i >= 0; i--)
            {
                var letter = word[i];
                var index = _alphabet.IndexOf(letter);
                if (index == -1)
                {
                    return -1;
                }

                var x = (index + 1) * (long)Math.Pow(62, word.Length - (i + 1));
                number += x;
            }

            return number - 1;
        }
    }
}
