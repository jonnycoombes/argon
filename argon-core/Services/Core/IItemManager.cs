using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Version = JCS.Argon.Model.Schema.Version;

namespace JCS.Argon.Services.Core
{
    /// <summary>
    /// Service which is responsible for basic/atomic item operations
    /// </summary>
    public interface IItemManager
    {
        /// <summary>
        /// Returns a list of items for a given collection
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public Task<List<Item>> GetItemsForCollectionAsync(Collection collection);

        /// <summary>
        /// Returns the meta-data for a given collection item
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public Task<Item> GetItemForCollectionAsync(Collection collection, Guid itemId);

        /// <summary>
        /// Retrieve a specified version for an item
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public Task<Version> GetItemVersionAsync(Collection collection, Item item, Guid versionId);

        /// <summary>
        /// Gets the current version for a specified item
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public Task<Version> GetCurrentItemVersionAsync(Collection collection, Guid itemId);

        /// <summary>
        /// Gets the current version for a specified item
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public Task<Version> GetCurrentItemVersionAsync(Collection collection, Item item);


        /// <summary>
        /// Return a count of items for a specific collection
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public Task<int> CountItemsAsync(Collection collection);

        /// <summary>
        /// Return a total count of all currently known items
        /// </summary>
        /// <returns></returns>
        public Task<int> CountItemsAsync();

        /// <summary>
        /// Adds an item to a collection with a specific collection id
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="properties"></param>
        /// <param name="inboundFile"></param>
        /// <returns></returns>
        public Task<Item> AddItemToCollectionAsync(Collection collection, Dictionary<string, object>? properties, IFormFile inboundFile);

        /// <summary>
        /// Adds a new version to a pre-existing item within a collection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <param name="properties"></param>
        /// <param name="inboundFile"></param>
        /// <returns></returns>
        public Task<Item> AddItemVersionToCollectionAsync(Collection collection, Item item, Dictionary<string, object>? properties,
            IFormFile inboundFile);

        /// <summary>
        /// Retrieves a stream for a given <see cref="System.Version"/>
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public Task<Stream> GetStreamForVersionAsync(Collection collection, Item item, Version version);

        /// <summary>
        /// Exception type which may be thrown by methods across this interface
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