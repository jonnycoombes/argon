using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;

namespace JCS.Argon.Services.Core
{
    public interface ICollectionManager
    {
        
        /// <summary>
        /// Thrown in the event of a failure within the collection manager
        /// </summary>
        public sealed class CollectionManagerException : ResponseAwareException
        {
            public CollectionManagerException(int? statusHint, string? message) : base(statusHint, message)
            {
                Source = nameof(ICollectionManager);
            }

            public CollectionManagerException(int? statusHint, string? message, Exception? inner) : base(statusHint, message, inner)
            {
                Source = nameof(ICollectionManager); 
            }
        }
        
        /// <summary>
        /// Retrieves a list of all current collections
        /// </summary>
        /// <returns></returns>
        public Task<List<Collection>> ListCollectionsAsync();

        /// <summary>
        /// Return a count of all currently known collections 
        /// </summary>
        /// <returns></returns>
        public Task<int> CountCollectionsAsync();

        /// <summary>
        /// Attempts the creation of a new <see cref="Collection"/> object, along with
        /// all associated VSP structures
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public Task<Collection> CreateCollectionAsync(CreateCollectionCommand cmd);

        /// <summary>
        /// Reads a specific collection from the underlying store, based on a supplied collection identifier
        /// </summary>
        /// <param name="collectionId">The unique GUID associated with the collection</param>
        /// <returns></returns>
        public Task<Collection> ReadCollectionAsync(Guid collectionId);

        /// <summary>
        /// Attempts to update the meta-data associated with a given <see cref="Collection"/> instance,
        /// along with all associated VSP structures
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <param name="cmd">The things to change</param>
        /// <returns></returns>
        public Task<Collection> UpdateCollectionAsync(Guid collectionId, PatchCollectionCommand cmd);

        /// <summary>
        /// Retrieves a list of currently configured <see cref="VirtualStorageBinding"/> instances
        /// </summary>
        /// <returns></returns>
        public List<VirtualStorageBinding> GetStorageBindings();


    }
}