using System;
using System.Text.Json;
using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Services.Core
{
    public class DbCache : BaseCoreService, IDbCache
    {
        public DbCache(ILogger<DbCache> log, SqlDbContext dbContext) : base(log, dbContext)
        {
        }

        public async Task<CacheEntry> AddOrReplaceStringValueAsync(string partition, string key, string value)
        {
            _log.LogDebug($"Performing cache lookup for key [{partition},{key}]");
            await DeleteEntry(partition, key);
            var addOp = await _dbContext.CacheEntries.AddAsync(new CacheEntry()
            {
                Partition = partition,
                Key = key,
                StringValue = value
            });
            await _dbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        public async Task<CacheEntry> AddOrReplaceLongValueAsync(string partition, string key, long value)
        {
            _log.LogDebug($"Performing cache lookup for key [{partition},{key}]");
            await DeleteEntry(partition, key);
            var addOp = await _dbContext.CacheEntries.AddAsync(new CacheEntry()
            {
                Partition = partition,
                Key = key,
                LongValue = value
            });
            await _dbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        public async Task<CacheEntry> AddOrReplaceIntValueAsync(string partition, string key, int value)
        {
            _log.LogDebug($"Performing cache lookup for key [{partition},{key}]");
            await DeleteEntry(partition, key);
            var addOp = await _dbContext.CacheEntries.AddAsync(new CacheEntry()
            {
                Partition = partition,
                Key = key,
                IntValue = value
            });
            await _dbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        public async Task<CacheEntry> AddOrReplaceJsonValueAsync(string key, JsonDocument value, string partition)
        {
            _log.LogDebug($"Performing cache lookup for key [{partition},{key}]");
            await DeleteEntry(partition, key);
            var addOp = await _dbContext.CacheEntries.AddAsync(new CacheEntry()
            {
                Partition = partition,
                Key = key,
                StringValue = value.ToString()
            });
            await _dbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        public async Task<CacheEntry> AddOrReplaceDatetimeValueAsync(string partition, string key, DateTime value)
        {
            _log.LogDebug($"Performing cache lookup for key [{partition},{key}]");
            await DeleteEntry(partition, key);
            var addOp = await _dbContext.CacheEntries.AddAsync(new CacheEntry()
            {
                Partition = partition,
                Key = key,
                DateTimeValue = value
            });
            await _dbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        public async Task<bool> HasEntry(string partition, string key)
        {
            _log.LogDebug($"Performing cache existence check for key [{partition},{key}]");
            var entry = await _dbContext.CacheEntries.FirstAsync(e => e.Partition == partition && e.Key == key);
            return entry != null;
        }

        public async Task<CacheEntry?> LookupEntry(string partition, string key)
        {
            _log.LogDebug($"Performing cache lookup for key [{partition},{key}]");
            return await _dbContext.CacheEntries.FirstAsync(e => e.Partition == partition && e.Key == key);
        }

        public async Task<bool> DeleteEntry(string partition, string key)
        {
            _log.LogDebug($"Performing cache deletion for key [{partition},{key}]");
            var entry = await LookupEntry(partition, key);
            if (entry != null)
            {
                _dbContext.CacheEntries.Remove(entry);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}