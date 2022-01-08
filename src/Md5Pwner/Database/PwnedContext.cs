using System;
using LiteDB;
using Microsoft.Extensions.Configuration;

namespace Md5Pwner.Database
{
    /// <summary>
    /// Represents the database interaction context.
    /// </summary>
    public class PwnedContext : IDisposable
    {
        private readonly LiteDatabase _context;

        /// <summary>
        /// Gets the md5 pwned hash collection.
        /// </summary>
        public ILiteCollection<Md5PwnedHash> Hashes { get; init; }

        public PwnedContext(IConfiguration configuration)
        {
            _context = new LiteDatabase(configuration["DatabasePath"]);

            Hashes = _context.GetCollection<Md5PwnedHash>();
            Hashes.EnsureIndex(x => x.Hash);
        }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        {
            _context.Dispose();
        }
    }
}
