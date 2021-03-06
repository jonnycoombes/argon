#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

namespace JCS.Argon.Services.Core
{
    /// <summary>
    ///     Class which manages most of the operations associated with <see cref="Item" /> and <see cref="ItemVersion" /> schema entities
    /// </summary>
    public class ItemManager : BaseCoreService, IItemManager
    {
        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<ItemManager>();

        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="options">Current configuration</param>
        /// <param name="serviceProvider">Current DI <see cref="IServiceProvider" /></param>
        public ItemManager(IOptionsMonitor<ApiOptions> options, IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {
            LogMethodCall(_log);
        }

        /// <inheritdoc cref="IItemManager.GetItemsForCollectionAsync" />
        public async Task<List<Item>> GetItemsForCollectionAsync(Collection collection)
        {
            LogMethodCall(_log);
            var items = await DbContext.Items.Where(i => i.Collection.Id == collection.Id)
                .Include(i => i.Versions)
                .Include(i => i.PropertyGroup)
                .Include(i => i.PropertyGroup.Properties)
                .ToListAsync();
            // null out the collection so that it isn't repeated for each item in the JSON response
            items = items.Select(i =>
            {
                i.Collection = null;
                return i;
            }).ToList();
            return items;
        }

        /// <inheritdoc cref="IItemManager.GetItemForCollectionAsync" />
        public async Task<Item> GetItemForCollectionAsync(Collection collection, Guid itemId)
        {
            LogMethodCall(_log);
            if (!await DbContext.Items.AnyAsync(i => i.Id == itemId && i.Collection.Id == collection.Id))
            {
                if (await DbContext.Items.AnyAsync(i => i.Id == itemId))
                {
                    LogWarning(_log, "Aattempt to retrieve item with incorrect collection id");
                    throw new ICollectionManager.CollectionManagerException(StatusCodes.Status404NotFound,
                        "The item exists, but isn't associated with this collection.  Please check the collection id.");
                }

                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status404NotFound,
                    "The specified collection item does not exist.");
            }

            var item = await DbContext.Items
                .Include(i => i.PropertyGroup)
                .Include(i => i.PropertyGroup.Properties)
                .Include(i => i.Versions)
                .FirstAsync(i => i.Id == itemId);
            return await CommitItemAndUpdateLastAccessed(item);
        }

        /// <inheritdoc cref="IItemManager.DeleteItemFromCollection" />
        public async Task DeleteItemFromCollection(Collection collection, Guid itemId)
        {
            LogMethodCall(_log);
            if (!await ItemExists(collection, itemId))
            {
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status404NotFound,
                    "The specified item does not exist");
            }

            try
            {
                {
                    var item = await DbContext.Items
                        .Include(i => i.PropertyGroup)
                        .Include(i => i.PropertyGroup.Properties)
                        .Include(i => i.Versions)
                        .FirstAsync(i => i.Id == itemId);
                    await PerformProviderItemDeletionActions(collection, item);
                    collection.NumberOfItems -= 1;
                    collection.TotalSizeBytes -= await GetItemTotalSize(item);
                    DbContext.Collections.Update(collection);
                    DbContext.Items.Remove(item);
                    await CheckedContextSave();
                }
            }
            catch (IVirtualStorageManager.VirtualStorageManagerException ex)
            {
                // roll back the entity changes
                LogWarning(_log, "Caught storage exception whilst attempting item physical operation");
                throw new IItemManager.ItemManagerException(ex.ResponseCodeHint,
                    ex.Message, ex);
            }
            catch (Exception ex)
            {
                // roll back the entity changes
                LogWarning(_log, $"Caught general exception whilst attempting item physical operation \"{ex.Message}\"");
                LogExceptionError(_log, ex);
                throw new IItemManager.ItemManagerException(StatusCodes.Status500InternalServerError,
                    ex.Message, ex);
            }
        }

        /// <inheritdoc cref="IItemManager.GetItemVersionAsync(Collection, Item, Guid)" />
        public async Task<ItemVersion> GetItemVersionAsync(Collection collection, Item item, Guid versionId)
        {
            LogMethodCall(_log);
            if (!await ItemVersionExists(collection, item, versionId))
            {
                throw new IItemManager.ItemManagerException(StatusCodes.Status404NotFound,
                    "The specified item version does not exist");
            }

            await CommitItemAndUpdateLastAccessed(item);
            return await DbContext.Versions
                .SingleAsync(v => v.Id == versionId && v.Item.Id == item.Id && v.Item.Collection.Id == collection.Id);
        }

        /// <inheritdoc cref="IItemManager.GetCurrentItemVersionAsync(Collection, Guid)" />
        public async Task<ItemVersion> GetCurrentItemVersionAsync(Collection collection, Guid itemId)
        {
            LogMethodCall(_log);
            var item = await GetItemForCollectionAsync(collection, itemId);
            var maxVersion = await DbContext.Versions.Where(v => v.Item.Id == itemId).MaxAsync(v => v.Major);
            await CommitItemAndUpdateLastAccessed(item);
            return await DbContext.Versions
                .SingleAsync(v => v.Major == maxVersion && v.Item.Id == itemId);
        }

        /// <inheritdoc cref="IItemManager.GetCurrentItemVersionAsync(Collection, Item)" />
        public async Task<ItemVersion> GetCurrentItemVersionAsync(Collection collection, Item item)
        {
            LogMethodCall(_log);
            var maxVersion = await DbContext.Versions.Where(v => v.Item.Id == item.Id).MaxAsync(v => v.Major);
            await CommitItemAndUpdateLastAccessed(item);
            return await DbContext.Versions
                .SingleAsync(v => v.Major == maxVersion && v.Item.Id == item.Id);
        }

        /// <inheritdoc cref="IItemManager.CountItemsAsync(Collection)" />
        public async Task<int> CountItemsAsync(Collection collection)
        {
            LogMethodCall(_log);
            return await DbContext.Items.CountAsync(c => c.Collection.Id == collection.Id);
        }

        /// <inheritdoc cref="IItemManager.CountItemsAsync()" />
        public async Task<int> CountItemsAsync()
        {
            LogMethodCall(_log);
            return await DbContext.Items.CountAsync();
        }

        /// <inheritdoc cref="IItemManager.AddItemToCollectionAsync" />
        public async Task<Item> AddItemToCollectionAsync(Collection collection, Dictionary<string, object>? properties,
            IFormFile inboundFile)
        {
            LogMethodCall(_log);
            try
            {
                var version = await CreateNewVersionTemplate(inboundFile);
                var item = await CreateNewItemTemplate(collection, version, properties);
                ValidatePropertiesAgainstConstraints(collection, item);
                var op = await DbContext.Items.AddAsync(item);
                await CheckedContextSave();
                item = op.Entity;
                await PerformProviderItemCreationActions(collection, item, version, inboundFile);
                collection.NumberOfItems += 1;
                collection.TotalSizeBytes += version.Size;
                DbContext.Collections.Update(collection);
                await CheckedContextSave();
                return item;
            }
            catch (IVirtualStorageManager.VirtualStorageManagerException ex)
            {
                // roll back the entity changes
                LogWarning(_log, "Caught storage exception whilst attempting item physical operation");
                throw new IItemManager.ItemManagerException(ex.ResponseCodeHint,
                    ex.Message, ex);
            }
            catch (IItemManager.ItemManagerException ex)
            {
                LogWarning(_log, "Caught item manager exception whilst attempting to upload new item");
                throw new IItemManager.ItemManagerException(ex.ResponseCodeHint,
                    ex.Message, ex);
            }
            catch (Exception ex)
            {
                // roll back the entity changes
                LogWarning(_log, $"Caught general exception whilst attempting item physical operation \"{ex.Message}\"");
                LogExceptionError(_log, ex);
                throw new IItemManager.ItemManagerException(StatusCodes.Status500InternalServerError,
                    ex.Message, ex);
            }
        }

        /// <inheritdoc cref="IItemManager.AddItemVersionToCollectionAsync" />
        public async Task<Item> AddItemVersionToCollectionAsync(Collection collection, Item item, Dictionary<string, object>? properties,
            IFormFile inboundFile)
        {
            LogMethodCall(_log);
            try
            {
                var maxVersion = await DbContext.Versions.Where(v => v.Item.Id == item.Id).MaxAsync(v => v.Major);
                var version = await CreateNewVersionTemplate(inboundFile, maxVersion + 1);
                version.Item = item;
                var op = await DbContext.Versions.AddAsync(version);
                await CheckedContextSave();
                version = op.Entity;
                await PerformProviderItemCreationActions(collection, item, version, inboundFile);
                collection.TotalSizeBytes += version.Size;
                DbContext.Collections.Update(collection);
                await CheckedContextSave();
                return item;
            }
            catch (IVirtualStorageManager.VirtualStorageManagerException ex)
            {
                // roll back the entity changes
                LogWarning(_log, "Caught storage exception whilst attempting item physical operation");
                throw new IItemManager.ItemManagerException(ex.ResponseCodeHint,
                    ex.Message, ex);
            }
            catch (Exception ex)
            {
                // roll back the entity changes
                LogWarning(_log, "Caught general exception whilst attempting version physical operation - rolling back db changes");
                throw new IItemManager.ItemManagerException(StatusCodes.Status500InternalServerError,
                    ex.Message, ex);
            }
        }

        /// <inheritdoc cref="IItemManager.GetStreamForVersionAsync" />
        public async Task<Stream> GetStreamForVersionAsync(Collection collection, Item item, ItemVersion itemVersion)
        {
            if (!await ItemVersionExists(collection, item, itemVersion.Id.Value))
            {
                throw new IItemManager.ItemManagerException(StatusCodes.Status404NotFound,
                    "The specified item version does not exist");
            }

            await CommitItemAndUpdateLastAccessed(item);
            return await PerformProviderVersionRetrievalActions(collection, item, itemVersion);
        }

        /// <inheritdoc cref="IItemManager.UpdateItemProperties" />
        public async Task<Item> UpdateItemProperties(Collection collection, Item item, Dictionary<string, object> properties)
        {
            LogMethodCall(_log);
            if (properties == null)
            {
                throw new IItemManager.ItemManagerException(StatusCodes.Status400BadRequest,
                    "No properties have been specified for the update operation");
            }

            if (item.PropertyGroup == null)
            {
                LogWarning(_log, "Null property group located - creating new property group");
                item.PropertyGroup = await PropertyGroupManager.CreatePropertyGroupAsync();
            }

            item.PropertyGroup.MergeDictionary(properties);
            ValidatePropertiesAgainstConstraints(collection, item);
            DbContext.PropertyGroups.Update(item.PropertyGroup);
            DbContext.Items.Update(item);
            return await CommitItemAndUpdateLastAccessed(item);
        }

        /// <inheritdoc cref="IItemManager.DeleteItemProperties" />
        public async Task<Item> DeleteItemProperties(Collection collection, Item item, string[] propertyNames)
        {
            LogMethodCall(_log);
            if (propertyNames.Length > 0)
            {
                foreach (var propertyName in propertyNames)
                {
                    item.PropertyGroup.RemoveProperty(propertyName);
                }
            }

            ValidatePropertiesAgainstConstraints(collection, item);
            DbContext.PropertyGroups.Update(item.PropertyGroup);
            return await CommitItemAndUpdateLastAccessed(item);
        }

        /// <summary>
        ///     Convenience function for checking the existence of a given <see cref="Item" /> against a specific parent <see cref="Collection" />
        /// </summary>
        /// <param name="collection">The parent <see cref="Collection" /></param>
        /// <param name="itemId">A <see cref="Guid" /> identifier for the item</param>
        /// <returns></returns>
        private async Task<bool> ItemExists(Collection collection, Guid itemId)
        {
            return await DbContext.Items.Where(i => i.Collection.Id == collection.Id && i.Id == itemId).AnyAsync();
        }

        /// <summary>
        ///     Aggregates up the total size of a given item, by totalling up the sizes of its versions
        /// </summary>
        /// <param name="item">The item to operate on</param>
        /// <returns>A long which gives the total size occupied by an items versions in bytes</returns>
        private async Task<long> GetItemTotalSize(Item item)
        {
            var sum = await DbContext.Versions.Where(v => v.Item.Id == item.Id).SumAsync(v => v.Size);
            return sum;
        }

        /// <summary>
        ///     Checks whether a specific item version exists
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item">The parent <see cref="Item" /></param>
        /// <param name="versionId">The id of the version to try and locate</param>
        /// <returns></returns>
        private async Task<bool> ItemVersionExists(Collection collection, Item item, Guid versionId)
        {
            LogMethodCall(_log);
            return await DbContext.Versions.AnyAsync(v => v.Id == versionId
                                                          && v.Item.Id == item.Id
                                                          && v.Item.Collection.Id == collection.Id);
        }

        /// <summary>
        ///     Checks a given <see cref="Item" />'s property set against a specified set of contraints, if they exist within a
        ///     given collection
        /// </summary>
        /// <param name="collection">The collection which may or may not have constraints</param>
        /// <param name="item">The <see cref="Item" /> owning the properties</param>
        /// <exception cref="ICollectionManager.CollectionManagerException"></exception>
        private void ValidatePropertiesAgainstConstraints(Collection collection, Item item)
        {
            if (collection.ConstraintGroup == null)
            {
                return;
            }

            var constraintCheckResults = ConstraintGroupManager.ValidatePropertiesAgainstConstraints(collection.ConstraintGroup,
                item.PropertyGroup);
            if (constraintCheckResults.Count <= 0)
            {
                return;
            }

            var message = constraintCheckResults.Aggregate((s, t) => s + ',' + t);
            throw new IItemManager.ItemManagerException(StatusCodes.Status400BadRequest,
                message);
        }

        /// <summary>
        ///     Interact with the storage provider to store the actual item in the storage layer
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
            var provider = VirtualStorageManager.GetProviderByTag(collection.ProviderTag);
            var creationResult = await provider.CreateCollectionItemVersionAsync(collection, item, itemVersion, source);
            if (creationResult.Properties != null)
            {
                item.PropertyGroup.MergeDictionary(creationResult.Properties);
                DbContext.Update(item);
            }
        }

        /// <summary>
        ///     Interact with the appropriate <see cref="IVirtualStorageProvider" /> in order to delete a given item
        /// </summary>
        /// <param name="collection">The parent <see cref="Collection" /> for the item to be deleted</param>
        /// <param name="item">The <see cref="Item" /> to be deleted</param>
        /// <returns></returns>
        private async Task PerformProviderItemDeletionActions(Collection collection, Item item)
        {
            LogMethodCall(_log);
            var provider = VirtualStorageManager.GetProviderByTag(collection.ProviderTag);
            await provider.DeleteCollectionItemAsync(collection, item);
        }

        /// <summary>
        ///     Helper method that does the heavy lifting around the retrieval of specific version streams from storage
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <param name="itemVersion"></param>
        /// <returns></returns>
        /// <exception cref="IItemManager.ItemManagerException"></exception>
        private async Task<Stream> PerformProviderVersionRetrievalActions(Collection collection, Item item, ItemVersion itemVersion)
        {
            var retries = 5;
            LogMethodCall(_log);
            LogDebug(_log, $"Looking up a virtual storage provider with tag [{collection.ProviderTag}");
            var provider = VirtualStorageManager.GetProviderByTag(collection.ProviderTag);
            var retrievalResult = await provider.ReadCollectionItemVersionAsync(collection, item, itemVersion);
            if (retrievalResult.Status == IVirtualStorageProvider.StorageOperationStatus.Ok && retrievalResult.Stream != null)
            {
                return retrievalResult.Stream;
            }

            LogWarning(_log, $"Failed to retrieve a specific item version - will retry after 1s (retries = {retries})");
            throw new IItemManager.ItemManagerException(StatusCodes.Status500InternalServerError,
                "An unhandled error occurred whilst attempting to retrieve a version from storage");
        }

        /// <summary>
        ///     Creates a new <see cref="Item" /> template which includes an initial version, along with a populated property group
        /// </summary>
        /// <param name="collection">The parent <see cref="Collection" /> for the new item</param>
        /// <param name="itemVersion">The initial <see cref="ItemVersion" /> for the new item</param>
        /// <param name="properties">A (pre-validated) property bag for the new item</param>
        /// <returns></returns>
        protected async Task<Item> CreateNewItemTemplate(Collection collection, ItemVersion itemVersion,
            Dictionary<string, object>? properties)
        {
            LogMethodCall(_log);
            var propertyGroup = await PropertyGroupManager.CreatePropertyGroupAsync();
            propertyGroup.MergeDictionary(properties);
            var item = new Item
            {
                Name = itemVersion.Name, Collection = collection, CreatedDate = DateTime.Now, LastModified = DateTime.Now,
                PropertyGroup = propertyGroup, Versions = new List<ItemVersion>()
            };
            item.Versions.Add(itemVersion);
            itemVersion.Item = item;
            return item;
        }

        /// <summary>
        ///     Creates a new version template
        /// </summary>
        /// <param name="source">The source object</param>
        /// <param name="majorVersion">Optional major version (defaults to 1)</param>
        /// <param name="minorVersion">Optional minor version (default to 0)</param>
        /// <returns></returns>
        private static Task<ItemVersion> CreateNewVersionTemplate(IFormFile source, int majorVersion = 1,
            int minorVersion = 0)
        {
            LogMethodCall(_log);
            LogVerbose(_log, $"Handling form file [{source.Name}, {source.Length}, {source.FileName}]");
            return Task.Run(() =>
            {
                var version = new ItemVersion
                {
                    Name = source.FileName, MIMEType = source.Headers == null ? "text/plain" : source.ContentType, Size = source.Length,
                    Major = majorVersion, Minor = minorVersion
                };
                return version;
            });
        }

        /// <summary>
        ///     Update the last accessed time for a given <see cref="Item" /> instance.  The current system (server-side) timestamp
        ///     is used.
        /// </summary>
        /// <param name="item">The collection to update</param>
        /// <returns>The updated collection</returns>
        /// <exception cref="ICollectionManager.CollectionManagerException">Thrown in the event of a db commit fail</exception>
        private async Task<Item> CommitItemAndUpdateLastAccessed(Item item)
        {
            try
            {
                LogMethodCall(_log);
                if (item.PropertyGroup == null)
                {
                    return item;
                }

                item.PropertyGroup.AddOrReplaceProperty($"{Collection.StockCollectionProperties.LastAccessed}", PropertyType.DateTime,
                    DateTime.Now);
                item.LastModified = DateTime.Now;
                DbContext.Items.Update(item);
                await CheckedContextSave();
            }
            catch (Exception ex)
            {
                LogExceptionWarning(_log, ex);
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status500InternalServerError,
                    "An exception occurred whilst attempting to commit a collection change");
            }

            return item;
        }
    }
}