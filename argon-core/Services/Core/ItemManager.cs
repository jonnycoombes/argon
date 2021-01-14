using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

namespace JCS.Argon.Services.Core
{
    public class ItemManager : BaseCoreService, IItemManager
    {
        /// <summary>
        /// Static logger
        /// </summary>
        private static ILogger _log = Log.ForContext<ItemManager>();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="options">Current configuration</param>
        /// <param name="serviceProvider">Current DI <see cref="IServiceProvider"/></param>
        public ItemManager(IOptionsMonitor<ApiConfiguration> options, IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {
            LogMethodCall(_log);
        }

        /// <inheritdoc cref="IItemManager.GetItemsForCollectionAsync"/> 
        public async Task<List<Item>> GetItemsForCollectionAsync(Collection collection)
        {
            LogMethodCall(_log);
            var items = await DbContext.Items.Where(i => i.Collection.Id == collection.Id)
                .Include(i => i.Versions)
                .ToListAsync();
            return items;
        }

        /// <inheritdoc cref="IItemManager.GetItemForCollectionAsync"/>
        public async Task<Item> GetItemForCollectionAsync(Collection collection, Guid itemId)
        {
            LogMethodCall(_log);
            if (await DbContext.Items.AnyAsync(i => i.Id == itemId))
            {
                var item = await DbContext.Items
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

        public async Task<ItemVersion> GetItemVersionAsync(Collection collection, Item item, Guid versionId)
        {
            LogMethodCall(_log);
            return await DbContext.Versions
                .SingleAsync(v => (v.Id == versionId && v.Item.Id == item.Id));
        }

        /// <inheritdoc />
        public async Task<ItemVersion> GetCurrentItemVersionAsync(Collection collection, Guid itemId)
        {
            LogMethodCall(_log);
            var maxVersion = await DbContext.Versions.Where(v => v.Item.Id == itemId).MaxAsync(v => v.Major);
            return await DbContext.Versions
                .SingleAsync(v => (v.Major == maxVersion && v.Item.Id == itemId));
        }

        /// <inheritdoc/>
        public async Task<ItemVersion> GetCurrentItemVersionAsync(Collection collection, Item item)
        {
            LogMethodCall(_log);
            var maxVersion = await DbContext.Versions.Where(v => v.Item.Id == item.Id).MaxAsync(v => v.Major);
            return await DbContext.Versions
                .SingleAsync(v => (v.Major == maxVersion && v.Item.Id == item.Id));
        }

        /// <inheritdoc></inheritdoc>
        public async Task<int> CountItemsAsync(Collection collection)
        {
            LogMethodCall(_log);
            return await DbContext.Items.CountAsync(c => c.Collection.Id == collection.Id);
        }

        /// <inheritdoc></inheritdoc>
        public async Task<int> CountItemsAsync()
        {
            LogMethodCall(_log);
            return await DbContext.Items.CountAsync();
        }

        /// <inheritdoc cref="IItemManager.AddItemToCollectionAsync"/>
        public async Task<Item> AddItemToCollectionAsync(Collection collection, Dictionary<string, object>? properties,
            IFormFile inboundFile)
        {
            LogMethodCall(_log);
            try
            {
                var version = await CreateNewVersionTemplate(inboundFile);
                var item = await CreateNewItemTemplate(collection, version, properties);
                var addOp = await DbContext.Items.AddAsync(item);
                await DbContext.SaveChangesAsync();
                item = addOp.Entity;
                await PerformProviderItemCreationActions(collection, item, version, inboundFile);
                collection.Length = collection.Length + 1;
                collection.Size = collection.Size + version.Size;
                DbContext.Collections.Update(collection);
                await DbContext.SaveChangesAsync();
                return item;
            }
            catch (IVirtualStorageManager.VirtualStorageManagerException ex)
            {
                // roll back the entity changes
                LogWarning(_log, $"Caught storage exception whilst attempting item physical operation");
                throw new ICollectionManager.CollectionManagerException(ex.ResponseCodeHint,
                    ex.Message, ex);
            }
            catch (Exception ex)
            {
                // roll back the entity changes
                LogWarning(_log, $"Caught general exception whilst attempting item physical operation");
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status500InternalServerError,
                    ex.Message, ex);
            }
        }

        /// <inheritdoc cref="IItemManager.AddItemVersionToCollectionAsync"/>
        public async Task<Item> AddItemVersionToCollectionAsync(Collection collection, Item item, Dictionary<string, object>? properties,
            IFormFile inboundFile)
        {
            LogMethodCall(_log);
            try
            {
                var maxVersion = await DbContext.Versions.Where(v => v.Item.Id == item.Id).MaxAsync(v => v.Major);
                var version = await CreateNewVersionTemplate(inboundFile, majorVersion: maxVersion + 1);
                version.Item = item;
                var addOp = await DbContext.Versions.AddAsync(version);
                await DbContext.SaveChangesAsync();
                version = addOp.Entity;
                await PerformProviderItemCreationActions(collection, item, version, inboundFile);
                collection.Length = collection.Length + 1;
                collection.Size = collection.Size + version.Size;
                DbContext.Collections.Update(collection);
                await DbContext.SaveChangesAsync();
                return item;
            }
            catch (IVirtualStorageManager.VirtualStorageManagerException ex)
            {
                // roll back the entity changes
                LogWarning(_log, $"Caught storage exception whilst attempting item physical operation");
                throw new ICollectionManager.CollectionManagerException(ex.ResponseCodeHint,
                    ex.Message, ex);
            }
            catch (Exception ex)
            {
                // roll back the entity changes
                LogWarning(_log, $"Caught general exception whilst attempting version physical operation - rolling back db changes");
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status500InternalServerError,
                    ex.Message, ex);
            }

        }

        /// <inheritdoc cref="IItemManager.GetStreamForVersionAsync"/>
        public async Task<Stream> GetStreamForVersionAsync(Collection collection, Item item, ItemVersion itemVersion)
        {
            return await PerformProviderVersionRetrievalActions(collection, item, itemVersion);
        }

        /// <summary>
        /// Interact with the storage provider to store the actual item in the storage layer
        /// </summary>
        /// <param name="collection">The parent collection</param>
        /// <param name="item">The freshly minted item</param>
        /// <param name="itemVersion"></param>
        /// <param name="source">The underlying source for the item</param>
        /// <returns></returns>
        private async Task PerformProviderItemCreationActions(Collection collection, Item item, ItemVersion itemVersion, IFormFile source)
        {
            LogMethodCall(_log);
            LogDebug(_log, $"Looking up a virtual storage provider with tag [{collection.ProviderTag}");
            var provider = VirtualStorageManager.GetProvider(collection.ProviderTag);
            var creationResult = await provider.CreateCollectionItemVersionAsync(collection, item, itemVersion, source);
            if (creationResult.Properties != null)
            {
                item.PropertyGroup.MergeDictionary(creationResult.Properties);
                DbContext.Update(item);
            }
        }

        /// <summary>
        /// Helper method that does the heavy lifting around the retrieval of specific version streams from storage
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <param name="itemVersion"></param>
        /// <returns></returns>
        /// <exception cref="IItemManager.ItemManagerException"></exception>
        private async Task<Stream> PerformProviderVersionRetrievalActions(Collection collection, Item item, ItemVersion itemVersion)
        {
            LogMethodCall(_log);
            LogDebug(_log,$"Looking up a virtual storage provider with tag [{collection.ProviderTag}");
            var provider = VirtualStorageManager.GetProvider(collection.ProviderTag);
            var retrievalResult = await provider.ReadCollectionItemVersionAsync(collection, item, itemVersion);
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

        protected async Task<Item> CreateNewItemTemplate(Collection collection, ItemVersion itemVersion,
            Dictionary<string, object>? properties)
        {
            LogMethodCall(_log);
            var propertyGroup = await PropertyGroupManager.CreatePropertyGroupAsync();
            propertyGroup.MergeDictionary(properties);
            var item = new Item()
            {
                Name = itemVersion.Name,
                Collection = collection,
                CreatedDate = DateTime.Now,
                LastModified = DateTime.Now,
                PropertyGroup = propertyGroup,
                Versions = new List<JCS.Argon.Model.Schema.ItemVersion>()
            };
            item.Versions.Add(itemVersion);
            itemVersion.Item = item;
            return item;
        }

        /// <summary>
        /// Creates a new version template
        /// </summary>
        /// <param name="source">The source object</param>
        /// <param name="majorVersion">Optional major version (defaults to 1)</param>
        /// <param name="minorVersion">Optional minor version (default to 0)</param>
        /// <returns></returns>
        protected Task<JCS.Argon.Model.Schema.ItemVersion> CreateNewVersionTemplate(IFormFile source, int majorVersion = 1, int minorVersion = 0)
        {
            return Task.Run(() =>
            {
                var version = new ItemVersion()
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