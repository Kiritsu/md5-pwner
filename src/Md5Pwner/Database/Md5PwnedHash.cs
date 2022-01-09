using System;
using LiteDB;

namespace Md5Pwner.Database
{
    /// <summary>
    /// Represents a MD5 pwned hash.
    /// </summary>
    public class Md5PwnedHash
    {
        /// <summary>
        /// Gets or sets the hash that has been pwned.
        /// </summary>
        [BsonId]
        public string Hash { get; set; } = null!;

        /// <summary>
        /// Gets or sets the value of the hash.
        /// </summary>
        public string Value { get; set; } = null!;

        /// <summary>
        /// Gets or sets the date time the hash was found.
        /// </summary>
        public DateTime FoundAt { get; set; }

        /// <summary>
        /// Gets or sets the date time the hash cracking was initiated.
        /// </summary>
        public DateTime InitiatedAt { get; set; }

        /// <summary>
        /// Gets the elapsed time.
        /// </summary>
        public TimeSpan ElapsedTime => FoundAt - InitiatedAt;
    }
}
