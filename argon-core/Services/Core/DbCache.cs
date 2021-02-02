#region

using System;
using System.Text.Json;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion


namespace JCS.Argon.Services.Core
{
    public class DbCache : BaseCoreService, IDbCache
    {
        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<DbCache>();

        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="options">The current api configuration</param>
        /// <param name="serviceProvider">The current DI <see cref="IServiceProvider" /></param>
        public DbCache(IOptionsMonitor<ApiOptions> options, IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {
            LogMethodCall(_log);
        }

        /// <inheritdoc cref="IDbCache.AddOrReplaceStringValueAsync" />
        public async Task<CacheEntry> AddOrReplaceStringValueAsync(string partition, string key, string value)
        {
            LogMethodCall(_log);
            await DeleteEntry(partition, key);
            var addOp = await DbContext.CacheEntries.AddAsync(new CacheEntry
            {
                Partition = partition,
                Key = key,
                StringValue = value
            });
            await DbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        /// <inheritdoc cref="IDbCache.AddOrReplaceLongValueAsync" />
        public async Task<CacheEntry> AddOrReplaceLongValueAsync(string partition, string key, long value)
        {
            LogMethodCall(_log);
            await DeleteEntry(partition, key);
            var addOp = await DbContext.CacheEntries.AddAsync(new CacheEntry
            {
                Partition = partition,
                Key = key,
                LongValue = value
            });
            await DbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        /// <inheritdoc cref="IDbCache.AddOrReplaceIntValueAsync" />
        public async Task<CacheEntry> AddOrReplaceIntValueAsync(string partition, string key, int value)
        {
            LogMethodCall(_log);
            await DeleteEntry(partition, key);
            var addOp = await DbContext.CacheEntries.AddAsync(new CacheEntry
            {
                Partition = partition,
                Key = key,
                IntValue = value
            });
            await DbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        /// <inheritdoc cref="IDbCache.AddOrReplaceJsonValueAsync" />
        public async Task<CacheEntry> AddOrReplaceJsonValueAsync(string key, JsonDocument value, string partition)
        {
            LogMethodCall(_log);
            await DeleteEntry(partition, key);
            var addOp = await DbContext.CacheEntries.AddAsync(new CacheEntry
            {
                Partition = partition,
                Key = key,
                StringValue = value.ToString()
            });
            await DbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        /// <inheritdoc cref="IDbCache.AddOrReplaceDatetimeValueAsync" />
        public async Task<CacheEntry> AddOrReplaceDatetimeValueAsync(string partition, string key, DateTime value)
        {
            LogMethodCall(_log);
            await DeleteEntry(partition, key);
            var addOp = await DbContext.CacheEntries.AddAsync(new CacheEntry
            {
                Partition = partition,
                Key = key,
                DateTimeValue = value
            });
            await DbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        /// <inheritdoc cref="IDbCache.HasEntry" />
        public async Task<bool> HasEntry(string partition, string key)
        {
            LogMethodCall(_log);
            var entry = await DbContext.CacheEntries.FirstOrDefaultAsync(e => e.Partition == partition && e.Key == key);
            return entry != null;
        }

        /// <inheritdoc cref="IDbCache.LookupEntry" />
        public async Task<CacheEntry?> LookupEntry(string partition, string key)
        {
            LogMethodCall(_log);
            return await DbContext.CacheEntries.FirstOrDefaultAsync(e => e.Partition == partition && e.Key == key);
        }

        /// <inheritdoc cref="IDbCache.DeleteEntry" />
        public async Task<bool> DeleteEntry(string partition, string key)
        {
            LogMethodCall(_log);
            var entry = await LookupEntry(partition, key);
            if (entry == null) return false;
            DbContext.CacheEntries.Remove(entry);
            await DbContext.SaveChangesAsync();
            return true;
        }
    }
}