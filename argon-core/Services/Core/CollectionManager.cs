using System.Collections.Generic;
using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.VSP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Services.Core
{
    public class CollectionManager  : BaseCoreService, ICollectionManager
    {

        /// <summary>
        /// The currently configured <see cref="IVSPFactory"/> instance
        /// </summary>
        protected IVSPFactory _vspFactory;

        /// <summary>
        /// Default constructor, parameters are DI'd by the IoC layer
        /// </summary>
        /// <param name="log"></param>
        /// <param name="dbContext"></param>
        /// <param name="vspFactory"></param>
        public CollectionManager(ILogger<CollectionManager> log, SqlDbContext dbContext, IVSPFactory vspFactory)
        :base(log, dbContext)
        {
            _vspFactory = vspFactory;
            _log.LogDebug("Creating new instance");
        }
        
        public async Task<int> CountCollectionsAsync()
        {
            return await _dbContext.Collections.CountAsync();
        }

        public Task<int> CountDocumentsAsync()
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<Collection>> ListCollections()
        {
            return await _dbContext.Collections.ToListAsync();
        }

        public async Task<Collection> CreateCollection(CreateCollectionCommand cmd)
        {
            var collection= await _dbContext.Collections.AddAsync(new Collection()
            {
                Name = cmd.Name,
                Description = cmd.Description
            });
            return collection.Entity;
        }
    }
}