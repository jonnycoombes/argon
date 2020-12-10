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

        public async Task<CacheEntry> AddOrReplaceStringValueAsync(string key, string value)
        {
            _log.LogDebug($"Performing cache lookup for key [{key}]");
            await DeleteEntry(key);
            var addOp = await _dbContext.CacheEntries.AddAsync(new CacheEntry()
            {
                Key = key,
                StringValue = value
            });
            await _dbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        public async Task<CacheEntry> AddOrReplaceLongValueAsync(string key, long value)
        {
            _log.LogDebug($"Performing cache lookup for key [{key}]");
            await DeleteEntry(key);
            var addOp = await _dbContext.CacheEntries.AddAsync(new CacheEntry()
            {
                Key = key,
                LongValue = value
            });
            await _dbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        public async Task<CacheEntry> AddOrReplaceIntValueAsync(string key, int value)
        {
            _log.LogDebug($"Performing cache lookup for key [{key}]");
            await DeleteEntry(key);
            var addOp = await _dbContext.CacheEntries.AddAsync(new CacheEntry()
            {
                Key = key,
                IntValue = value
            });
            await _dbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        public async Task<CacheEntry> AddOrReplaceJsonValueAsync(string key, JsonDocument value)
        {
            _log.LogDebug($"Performing cache lookup for key [{key}]");
            await DeleteEntry(key);
            var addOp = await _dbContext.CacheEntries.AddAsync(new CacheEntry()
            {
                Key = key,
                StringValue = value.ToString()
            });
            await _dbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        public async Task<CacheEntry> AddOrReplaceDatetimeValueAsync(string key, DateTime value)
        {
            _log.LogDebug($"Performing cache lookup for key [{key}]");
            await DeleteEntry(key);
            var addOp = await _dbContext.CacheEntries.AddAsync(new CacheEntry()
            {
                Key = key,
                DateTimeValue = value
            });
            await _dbContext.SaveChangesAsync();
            return addOp.Entity;
        }

        public async Task<bool> HasEntry(string key)
        {
            _log.LogDebug($"Performing cache existence check for key [{key}]");
            var entry = await _dbContext.CacheEntries.FirstAsync(e => e.Key == key);
            return entry != null;
        }

        public async Task<CacheEntry?> LookupEntry(string key)
        {
            _log.LogDebug($"Performing cache lookup for key [{key}]");
            return await _dbContext.CacheEntries.FirstAsync(e => e.Key == key);
        }

        public async Task<bool> DeleteEntry(string key)
        {
            _log.LogDebug($"Performing cache deletion for key [{key}]");
            var entry = await LookupEntry(key);
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