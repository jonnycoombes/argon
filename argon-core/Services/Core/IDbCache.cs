using System;
using System.Text.Json;
using System.Threading.Tasks;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Model.Schema;

namespace JCS.Argon.Services.Core
{
    /// <summary>
    /// Simple DB-backed cache which may be used by internal Argon services
    /// </summary>
    public interface IDbCache
    {
        /// <summary>
        /// Adds or replaces a string value
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<CacheEntry> AddOrReplaceStringValueAsync(string partition, string key, string value);

        /// <summary>
        /// Adds or replaces a long value
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<CacheEntry> AddOrReplaceLongValueAsync(string partition, string key, long value);

        /// <summary>
        /// Adds or replaces an integer value
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<CacheEntry> AddOrReplaceIntValueAsync(string partition, string key, int value);

        /// <summary>
        /// Adds or replaces Json value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="partition"></param>
        /// <returns></returns>
        public Task<CacheEntry> AddOrReplaceJsonValueAsync(string key, JsonDocument value, string partition);

        /// <summary>
        /// Adds or replaces a date time value
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<CacheEntry> AddOrReplaceDatetimeValueAsync(string partition, string key, DateTime value);

        /// <summary>
        /// Checks whether a given cache value exists
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<bool> HasEntry(string partition, string key);

        /// <summary>
        /// Looks up a particular value, return a null if it doesn't exist
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<CacheEntry?> LookupEntry(string partition, string key);

        /// <summary>
        /// Deletes an entry with a given key value
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="key"></param>
        /// <returns><code>true</code> if the entry existed, <code>false</code> otherwise</returns>
        public Task<bool> DeleteEntry(string partition, string key);

        /// <summary>
        /// Can optionally be thrown if something goes Pete Tong within a cache operation
        /// </summary>
        public class DbCacheException : ResponseAwareException
        {
            public DbCacheException(int? statusHint, string? message) : base(statusHint, message)
            {
            }

            public DbCacheException(int? statusHint, string? message, Exception? inner) : base(statusHint, message, inner)
            {
            }
        }
    }
}