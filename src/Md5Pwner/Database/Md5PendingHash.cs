using System;

namespace Md5Pwner.Database
{
    /// <summary>
    /// Represents a pending MD5 hash.
    /// </summary>
    public class Md5PendingHash
    {
        /// <summary>
        /// Gets or sets the hash to crack.
        /// </summary>
        public string Hash { get; set; } = null!;

        /// <summary>
        /// Gets or sets the date time the hash cracking was initiated.
        /// </summary>
        public DateTime InitiatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets whether the hash is being cracked or not.
        /// </summary>
        public bool Processing { get; set; } = false;
    }
}
