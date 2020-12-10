using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Version = JCS.Argon.Model.Schema.Version;

namespace JCS.Argon.Services.Core
{
    public class ItemManager : BaseCoreService, IItemManager
    {
        /// <summary>
        /// Scoped instance of <see cref="IConstraintGroupManager"/>
        /// </summary>
        protected readonly IConstraintGroupManager _constraintGroupManager;

        /// <summary>
        /// Scoped instance of <see cref="IPropertyGroupManager"/>
        /// </summary>
        protected readonly IPropertyGroupManager _propertyGroupManager;

        /// <summary>
        /// Scoped instance of <see cref="IVirtualStorageManager"/>
        /// </summary>
        protected readonly IVirtualStorageManager _virtualStorageManager;

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

        /// <inheritdoc cref="IItemManager.GetItemsForCollectionAsync"/> 
        public async Task<List<Item>> GetItemsForCollectionAsync(Collection collection)
        {
                var items = await _dbContext.Items.
                    Where(i => i.Collection.Id == collection.Id)
                    .Include(i => i.Versions)
                    .ToListAsync();
                return items;
        }

        /// <inheritdoc cref="IItemManager.GetItemForCollectionAsync"/>
        public async Task<Item> GetItemForCollectionAsync(Collection collection, Guid itemId)
        {
            if (await _dbContext.Items.AnyAsync(i => i.Id == itemId))
            {
                var item = await _dbContext.Items
                    .Include(i => i.PropertyGroup)
                    .Include(i => i.PropertyGroup.Properties)
                    .Include(i => i.Versions)
                    .FirstAsync(i => i.Id == itemId);
                return item;
            }
            else
            {
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status404NotFound,
                    "The specified item does not exist"); 
            }
        }

        public async Task<Version> GetItemVersionAsync(Collection collection, Item item, Guid versionId)
        {
            return await _dbContext.Versions
                .SingleAsync(v => (v.Id == versionId && v.Item.Id == item.Id));
        }

        /// <inheritdoc />
        public async Task<Version> GetCurrentItemVersionAsync(Collection collection, Guid itemId)
        {
            var maxVersion= await _dbContext.Versions.Where(v => v.Item.Id == itemId).MaxAsync(v => v.Major);
            return await _dbContext.Versions
                .SingleAsync(v => (v.Major == maxVersion && v.Item.Id == itemId));
        }

        /// <inheritdoc/>
        public async Task<Version> GetCurrentItemVersionAsync(Collection collection, Item item)
        {
            var maxVersion= await _dbContext.Versions.Where(v => v.Item.Id == item.Id).MaxAsync(v => v.Major);
            return await _dbContext.Versions
                .SingleAsync(v => (v.Major == maxVersion && v.Item.Id == item.Id));
        }

        /// <inheritdoc></inheritdoc>
        public async Task<int> CountItemsAsync(Collection collection)
        {
            return await _dbContext.Items.CountAsync(c => c.Collection.Id == collection.Id);
        }

        /// <inheritdoc></inheritdoc>
        public async Task<int> CountItemsAsync()
        {
            return await _dbContext.Items.CountAsync();
        }

        /// <inheritdoc cref="IItemManager.AddItemToCollectionAsync"/>
        public async Task<Item> AddItemToCollectionAsync(Collection collection, Dictionary<string, object>? properties,
            IFormFile inboundFile)
        {
            try
            {
                var version = await CreateNewVersionTemplate(inboundFile);
                var item = await CreateNewItemTemplate(collection, version, properties);
                var addOp= await _dbContext.Items.AddAsync(item);
                await _dbContext.SaveChangesAsync();
                item = addOp.Entity;
                await PerformProviderItemCreationActions(collection, item, version, inboundFile);
                collection.Length = collection.Length + 1;
                collection.Size = collection.Size + version.Size;
                _dbContext.Collections.Update(collection);
                await _dbContext.SaveChangesAsync();
                return item;
            }
            catch (IVirtualStorageManager.VirtualStorageManagerException ex)
            {
                // roll back the entity changes
                _log.LogWarning($"Caught storage exception whilst attempting item physical operation");
                throw new ICollectionManager.CollectionManagerException(ex.ResponseCodeHint,
                    ex.Message, ex);
            }
            catch (Exception ex)
            {
                // roll back the entity changes
                _log.LogWarning($"Caught general exception whilst attempting item physical operation");
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status500InternalServerError,
                    ex.Message, ex);
            }
        }

        /// <inheritdoc cref="IItemManager.AddItemVersionToCollectionAsync"/>
        public async Task<Item> AddItemVersionToCollectionAsync(Collection collection, Item item, Dictionary<string, object>? properties,
            IFormFile inboundFile)
        {
            try
            {
                var maxVersion= await _dbContext.Versions.Where(v => v.Item.Id == item.Id).MaxAsync(v => v.Major);
                var version = await CreateNewVersionTemplate(inboundFile, majorVersion: maxVersion+1);
                version.Item = item;
                var addOp= await _dbContext.Versions.AddAsync(version);
                await _dbContext.SaveChangesAsync();
                version = addOp.Entity;
                await PerformProviderItemCreationActions(collection, item, version, inboundFile);
                collection.Length = collection.Length + 1;
                collection.Size = collection.Size + version.Size;
                _dbContext.Collections.Update(collection);
                await _dbContext.SaveChangesAsync();
                return item;
            }
            catch (IVirtualStorageManager.VirtualStorageManagerException ex)
            {
                // roll back the entity changes
                _log.LogWarning($"Caught storage exception whilst attempting item physical operation");
                throw new ICollectionManager.CollectionManagerException(ex.ResponseCodeHint,
                    ex.Message, ex);
            }
            catch (Exception ex)
            {
                // roll back the entity changes
                _log.LogWarning($"Caught general exception whilst attempting version physical operation - rolling back db changes");
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status500InternalServerError,
                    ex.Message, ex);
            }
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IItemManager.GetStreamForVersionAsync"/>
        public async Task<Stream> GetStreamForVersionAsync(Collection collection, Item item, Version version)
        {
            return await PerformProviderVersionRetrievalActions(collection, item, version);
        }

        /// <summary>
        /// Interact with the storage provider to store the actual item in the storage layer
        /// </summary>
        /// <param name="collection">The parent collection</param>
        /// <param name="item">The freshly minted item</param>
        /// <param name="version"></param>
        /// <param name="source">The underlying source for the item</param>
        /// <returns></returns>
        private async Task PerformProviderItemCreationActions(Collection collection, Item item, Version version, IFormFile source)
        {
            _log.LogDebug($"Item version creation operation initiated");
            _log.LogDebug($"Looking up a virtual storage provider with tag [{collection.ProviderTag}");
            var provider = _virtualStorageManager.GetProvider(collection.ProviderTag);
            var creationResult= await provider.CreateCollectionItemVersionAsync(collection, item, version, source);
            if (creationResult.Properties != null)
            {
                item.PropertyGroup.MergeDictionary(creationResult.Properties);
                _dbContext.Update(item);
            }
        }

        /// <summary>
        /// Helper method that does the heavy lifting around the retrieval of specific version streams from storage
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="IItemManager.ItemManagerException"></exception>
        private async Task<Stream> PerformProviderVersionRetrievalActions(Collection collection, Item item, Version version)
        {
            _log.LogDebug($"Item version retrieval operation initiated");
            _log.LogDebug($"Looking up a virtual storage provider with tag [{collection.ProviderTag}");
            var provider = _virtualStorageManager.GetProvider(collection.ProviderTag);
            var retrievalResult= await provider.ReadCollectionItemVersionAsync(collection, item, version);
            if (retrievalResult.Status == IVirtualStorageProvider.StorageOperationStatus.Ok && retrievalResult.Stream != null)
            {
                return retrievalResult.Stream;
            }
            else
            {
                throw new IItemManager.ItemManagerException(StatusCodes.Status500InternalServerError,
                    $"An unhandled error occurred whilst attempting to retrieve a version from storage");
            }
        }

        protected async Task<Item> CreateNewItemTemplate(Collection collection, Version version, 
            Dictionary<string, object>? properties)
        {
            var propertyGroup = await _propertyGroupManager.CreatePropertyGroupAsync();
            propertyGroup.MergeDictionary(properties);
            var item = new Item()
            {
                Name = version.Name,
                Collection= collection,
                CreatedDate = DateTime.Now,
                LastModified = DateTime.Now,
                PropertyGroup = propertyGroup,
                Versions = new List<Version>()
            };
            item.Versions.Add(version);
            version.Item = item;
            return item;
        }

        /// <summary>
        /// Creates a new version template
        /// </summary>
        /// <param name="source">The source object</param>
        /// <param name="majorVersion">Optional major version (defaults to 1)</param>
        /// <param name="minorVersion">Optional minor version (default to 0)</param>
        /// <returns></returns>
        protected Task<Version> CreateNewVersionTemplate(IFormFile source, int majorVersion=1, int minorVersion=0)
        {
            return Task.Run(() =>
            {
                var version = new Version()
                {
                    Name = source.FileName,
                    MIMEType = source.ContentType,
                    Size = source.Length,
                    Major = majorVersion,
                    Minor = minorVersion
                };
                return version;
            });
        }
    }
}