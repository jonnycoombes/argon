using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;

namespace JCS.Argon.Services.Core
{
    /// <summary>
    /// Service which is responsible for basic/atomic item operations
    /// </summary>
    public interface IItemManager
    {
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
    }
}