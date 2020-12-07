using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Services.Core
{
    public class ItemManager : BaseCoreService, IItemManager
    {
        /// <summary>
        /// Scoped instance of <see cref="IVirtualStorageManager"/>
        /// </summary>
        protected readonly IVirtualStorageManager _virtualStorageManager;

        /// <summary>
        /// Scoped instance of <see cref="IConstraintGroupManager"/>
        /// </summary>
        protected readonly IConstraintGroupManager _constraintGroupManager;

        /// <summary>
        /// Scoped instance of <see cref="IPropertyGroupManager"/>
        /// </summary>
        protected readonly IPropertyGroupManager _propertyGroupManager;
        
        /// <summary>
        /// Default constructor with necessary IoC injected service dependencies
        /// </summary>
        /// <param name="log"></param>
        /// <param name="dbContext"></param>
        /// <param name="virtualStorageManager"></param>
        /// <param name="constraintGroupManager"></param>
        /// <param name="propertyGroupManager"></param>
        public ItemManager(ILogger<ItemManager> log, SqlDbContext dbContext, 
            IVirtualStorageManager virtualStorageManager,
            IConstraintGroupManager constraintGroupManager,
            IPropertyGroupManager propertyGroupManager) : base(log, dbContext)
        {
            _virtualStorageManager = virtualStorageManager;
            _constraintGroupManager = constraintGroupManager;
            _propertyGroupManager = propertyGroupManager;
            _log.LogDebug("Creating new instance");
        }

        /// <inheritdoc cref="IItemManager.CreateItemFromFormFileAsync"/>
        public async Task<Item> CreateItemFromFormFileAsync(Collection collection, Dictionary<string, object> properties, IFormFile source)
        {
            throw new System.NotImplementedException();
        }
        
        /// <inheritdoc cref="IItemManager.GetItemsForCollectionAsync"/> 
        public async Task<List<Item>> GetItemsForCollectionAsync(Collection collection)
        {
                var items = await _dbContext.Items.
                    Where(i => i.Collection.Id == collection.Id)
                    .ToListAsync();
                return items;
        }
        /// <inheritdoc cref="IItemManager.GetItemForCollection"/>
        public async Task<Item> GetItemForCollection(Collection collection, Guid itemId)
        {
            if (await _dbContext.Items.AnyAsync(i => i.Id == itemId))
            {
                var item = await _dbContext.Items.FirstAsync(i => i.Id == itemId);
                return item;
            }
            else
            {
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status404NotFound,
                    "The specified item does not exist"); 
            }
        }
        
        /// <inheritdoc cref="IItemManager.CountItemsAsync" />
        public async Task<int> CountItemsAsync(Collection collection)
        {
            return await _dbContext.Items.CountAsync(c => c.Collection.Id == collection.Id);
        }
        
        /// <inheritdoc cref="IItemManager.CountItemsAsync"/>
        public async Task<int> CountItemsAsync()
        {
            return await _dbContext.Items.CountAsync();
        }

        /// <summary>
        /// Interact with the storage provider to store the actual item in the storage layer
        /// </summary>
        /// <param name="collection">The parent collection</param>
        /// <param name="item">The freshly minted item</param>
        /// <param name="source">The underlying source for the item</param>
        /// <returns></returns>
        private async Task PerformProviderItemCreationActions(Collection collection, Item item, IFormFile source)
        {
            _log.LogDebug($"Looking up a virtual storage provider with tag [{collection.ProviderTag}");
            var provider = _virtualStorageManager.GetProvider(collection.ProviderTag);
            var creationResult= await provider.CreateCollectionItemAsync(collection, item, source);
            if (creationResult.Properties != null)
            {
                item.PropertyGroup.MergeDictionary(creationResult.Properties);
                _dbContext.Update(item);
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <inheritdoc cref="IItemManager.AddItemToCollectionAsync"/>
        public async Task<Item> AddItemToCollectionAsync(Collection collection, Dictionary<string, object>? properties,
            IFormFile inboundFile)
        {
            var propertyGroup = await _propertyGroupManager.CreatePropertyGroupAsync();
            var item = new Item()
            {
                Collection = collection,
                Name = inboundFile.FileName,
                CreatedDate = DateTime.Now,
                LastModified = DateTime.Now,
                PropertyGroup = propertyGroup
            };
            var itemEntity= await _dbContext.Items.AddAsync(item);
            await _dbContext.SaveChangesAsync();
            item = itemEntity.Entity;
            
            try
            {
                await PerformProviderItemCreationActions(collection, item, inboundFile);
            }
            catch (IVirtualStorageManager.VirtualStorageManagerException ex)
            {
                // roll back the entity changes
                _log.LogWarning($"Caught storage exception whilst attempting item physical operation - rolling back db changes");
                _dbContext.Items.Remove(item);
                await _dbContext.SaveChangesAsync();
                throw new ICollectionManager.CollectionManagerException(ex.ResponseCodeHint,
                    ex.Message, ex);
            }
            catch (Exception ex)
            {
                // roll back the entity changes
                _log.LogWarning($"Caught general exception whilst attempting item physical operation - rolling back db changes");
                _dbContext.Items.Remove(item);
                await _dbContext.SaveChangesAsync();
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status500InternalServerError,
                    ex.Message, ex);
            }
            
            collection.Length = collection.Length + 1;
            _dbContext.Collections.Update(collection);
            await _dbContext.SaveChangesAsync();
            return item;
        }
    }
}