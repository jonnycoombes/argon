using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.VSP;
using JCS.Argon.Utility;
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

        public Task<int> CountItemsAsync()
        {
            throw new System.NotImplementedException();
        }

#pragma warning disable 1574
        /// <inheritdoc cref="ICollectionManager.CountItemsAsync" />
#pragma warning restore 1574
        public async Task<int> CountItemsAsync(Guid collectionId)
        {
            if (!await CollectionExists(collectionId))
            {
                throw new ICollectionManager.CollectionManagerException(500, "The specified collection does not exist");
            }
            else
            {
                return await _dbContext.Items.CountAsync(c => c.CollectionId == collectionId);
            }
        }

        /// <inheritdoc cref="ICollectionManager.ListCollections" />
        public async Task<List<Collection>> ListCollections()
        {
            return await _dbContext.Collections
                .ToListAsync();
        }

        protected async Task<bool> CollectionExists(string name)
        {
            if (await _dbContext.Collections.AnyAsync())
            {
                var existing = await _dbContext.Collections
                    .FirstOrDefaultAsync(c => c.Name.Equals(name));
                return !(existing is null);
            }
            else
            {
                return false;
            }
        }

        protected async Task<bool> CollectionExists(Guid collectionId)
        {
            if (await _dbContext.Collections.AnyAsync())
            {
                var existing = await _dbContext.Collections.FirstOrDefaultAsync(c => c.Id.Equals(collectionId));
                return !(existing is null);
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc cref="ICollectionManager.CreateCollection"/>
        public async Task<Collection> CreateCollection(CreateCollectionCommand cmd)
        {
                var exists = await CollectionExists(cmd.Name);
                if (!exists)
                {
                    var collection = await _dbContext.Collections.AddAsync(new Collection()
                    {
                        Name = cmd.Name,
                        Description = cmd.Description
                    });
                    await _dbContext.SaveChangesAsync();
                    return collection.Entity;
                }
                else
                {
                    throw new ICollectionManager.CollectionManagerException(400, "A collection with that name already exists");
                }
        }

        /// <inheritdoc cref="ICollectionManager.ReadCollection"/> 
        public async Task<Collection> ReadCollection(Guid collectionId)
        {
            if (await CollectionExists(collectionId))
            {
                return await _dbContext.Collections.FirstAsync(c => c.Id == collectionId);
            }
            else
            {
                throw new ICollectionManager.CollectionManagerException(404, "The specified collection does not exist");
            }
        }

        /// <summary>
        /// Performs a number of checks to ensure that a collection update is valid
        /// </summary>
        /// <param name="target"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected async Task<List<string>> ValidateCollectionUpdate(Collection target, PatchCollectionCommand cmd)
        {
            List<string> validationErrors = new List<string>();
            if (cmd.Name != null)
            {
                if (target.Name != cmd.Name)
                {
                    var exists = await CollectionExists(cmd.Name);
                    if (exists)
                    {
                        validationErrors.Add("A collection with the supplied name already exists");
                    }    
                }
            }
            return validationErrors;
        }
        
        /// <inheritdoc cref="ICollectionManager.UpdateCollection"/>
        public async Task<Collection> UpdateCollection(Guid collectionId, PatchCollectionCommand cmd)
        {
            if (!await CollectionExists(collectionId))
            {
                throw new ICollectionManager.CollectionManagerException(404, "The specified collection does not exist");
            }
            else
            {
                var collection = await _dbContext.Collections.FirstAsync(c => c.Id == collectionId);
                if (collection != null)
                {
                    var validationErrors = await ValidateCollectionUpdate(collection, cmd);
                    if (validationErrors.Count == 0)
                    {
                        collection.Name = cmd.Name ?? collection.Name;
                        collection.Description = cmd.Description ?? collection.Description;
                        _dbContext.Collections.Update(collection);
                        await _dbContext.SaveChangesAsync();
                        return collection;
                    }
                    else
                    {
                        throw new ICollectionManager.CollectionManagerException(400,
                            $"Validation errors occurred: {StringUtilities.CollapseStringList(validationErrors)}");
                    }
                }
                throw new ICollectionManager.CollectionManagerException(500,
                    "Collection has moved or cannot be found - shouldn't happen");
            }
        }
    }
}