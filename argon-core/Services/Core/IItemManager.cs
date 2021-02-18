#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;

#endregion

namespace JCS.Argon.Services.Core
{
    /// <summary>
    ///     Service which is responsible for basic/atomic item operations
    /// </summary>
    public interface IItemManager
    {
        /// <summary>
        ///     Returns a list of items for a given collection
        /// </summary>
        /// <param name="collection">The <see cref="Collection" /></param>
        /// <returns>A list of <see cref="Item" /> instances</returns>
        public Task<List<Item>> GetItemsForCollectionAsync(Collection collection);

        /// <summary>
        ///     Returns the meta-data for a given collection item
        /// </summary>
        /// <param name="collection">The parent <see cref="Collection" /></param>
        /// <param name="itemId">The id for <see cref="Item" /></param>
        /// <returns>The specific <see cref="Item" /> instance</returns>
        public Task<Item> GetItemForCollectionAsync(Collection collection, Guid itemId);

        /// <summary>
        ///     Attempts the deletion of an item from the given collection
        /// </summary>
        /// <param name="collection">The parent collection</param>
        /// <param name="itemId">The id of the item to delete</param>
        /// <returns></returns>
        public Task DeleteItemFromCollection(Collection collection, Guid itemId);

        /// <summary>
        ///     Retrieve a specified version for an item
        /// </summary>
        /// <param name="collection">The parent <see cref="Collection" /></param>
        /// <param name="item">The specific <see cref="Item" /></param>
        /// <param name="versionId">The identifier for the version</param>
        /// <returns>A specific <see cref="ItemVersion" /></returns>
        public Task<ItemVersion> GetItemVersionAsync(Collection collection, Item item, Guid versionId);

        /// <summary>
        ///     Gets the current version for a specified item
        /// </summary>
        /// <param name="collection">The parent <see cref="Collection" /></param>
        /// <param name="itemId">The id for the <see cref="Item" /></param>
        /// <returns>A specific <see cref="ItemVersion" /></returns>
        public Task<ItemVersion> GetCurrentItemVersionAsync(Collection collection, Guid itemId);

        /// <summary>
        ///     Gets the current version for a specified item
        /// </summary>
        /// <param name="collection">The parent <see cref="Collection" /></param>
        /// <param name="item">The specific item</param>
        /// <returns>The latest <see cref="ItemVersion" /></returns>
        public Task<ItemVersion> GetCurrentItemVersionAsync(Collection collection, Item item);


        /// <summary>
        ///     Return a count of items for a specific collection
        /// </summary>
        /// <param name="collection">The <see cref="Collection" /></param>
        /// <returns>An integer count of the items in the collection</returns>
        public Task<int> CountItemsAsync(Collection collection);

        /// <summary>
        ///     Return a total count of all currently known items
        /// </summary>
        /// <returns>An integer count.  Will be greater than or equal to zero</returns>
        public Task<int> CountItemsAsync();

        /// <summary>
        ///     Adds an item to a collection with a specific collection id
        /// </summary>
        /// <param name="collection">The <see cref="Collection" /> to add to</param>
        /// <param name="properties">Dictionary of properties to be associated with the item</param>
        /// <param name="inboundFile">The <see cref="IFormFile" /> used to retrieve the actual content for the new item</param>
        /// <returns>A freshly minted <see cref="Item" /></returns>
        public Task<Item> AddItemToCollectionAsync(Collection collection, Dictionary<string, object>? properties, IFormFile inboundFile);

        /// <summary>
        ///     Adds a new version to a pre-existing item within a collection
        /// </summary>
        /// <param name="collection">The <see cref="Collection" /> containing the item</param>
        /// <param name="item">The <see cref="Item" /> to add a version to</param>
        /// <param name="properties">The revised properties for the new version</param>
        /// <param name="inboundFile">The <see cref="IFormFile" /> which can be used to get the contents for the new version</param>
        /// <returns>The original <see cref="Item" /> with updated version information</returns>
        public Task<Item> AddItemVersionToCollectionAsync(Collection collection, Item item, Dictionary<string, object>? properties,
            IFormFile inboundFile);

        /// <summary>
        ///     Retrieves a stream for a given <see cref="System.Version" />
        /// </summary>
        /// <param name="collection">The <see cref="Collection" /></param>
        /// <param name="item">The <see cref="Item" /> to retrieve</param>
        /// <param name="itemVersion">The specific <see cref="ItemVersion" /> to retrieve</param>
        /// <returns>A <see cref="Stream" /> containing the body of the version requested</returns>
        public Task<Stream> GetStreamForVersionAsync(Collection collection, Item item, ItemVersion itemVersion);

        /// <summary>
        ///     Will update the properties associated with a given <see cref="Item" /> by merging the supplied properties with any
        ///     existing <see cref="PropertyGroup" /> associated with the <see cref="Item" />
        /// </summary>
        /// <param name="collection">The parent <see cref="Collection" /> for the <see cref="Item" /></param>
        /// <param name="item">The <see cref="Item" /> to update</param>
        /// <param name="properties">A dictionary of string value pairs used during the update operation</param>
        /// <returns>The updated <see cref="Item" /></returns>
        /// <exception cref="IItemManager.ItemManagerException">Thrown in the event of the update failing</exception>
        public Task<Item> UpdateItemProperties(Collection collection, Item item, Dictionary<string, object> properties);

        /// <summary>
        ///     Removes the specified properties from a given item, but checks constraints prior to completion of the operation
        /// </summary>
        /// <param name="collection">The parent <see cref="Collection" /> for the item</param>
        /// <param name="item">The <see cref="Item" /></param>
        /// <param name="propertyNames">An array of property names.  Each will be removed, subject to constraints</param>
        /// <returns>The updated <see cref="Item" /></returns>
        public Task<Item> DeleteItemProperties(Collection collection, Item item, string[] propertyNames);

        /// <summary>
        ///     Exception type which may be thrown by methods across this interface
        /// </summary>
        public sealed class ItemManagerException : ResponseAwareException
        {
            public ItemManagerException(int? statusHint, string? message) : base(statusHint, message)
            {
                Source = nameof(IItemManager);
            }

            public ItemManagerException(int? statusHint, string? message, Exception? inner) : base(statusHint, message, inner)
            {
                Source = nameof(IItemManager);
            }
        }
    }
}