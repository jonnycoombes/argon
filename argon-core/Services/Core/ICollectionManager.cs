using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Model.Schema;

namespace JCS.Argon.Services.Core
{
    public interface ICollectionManager
    {
        
        /// <summary>
        /// Thrown in the event of a failure within the collection manager
        /// </summary>
        public sealed class CollectionManagerException : BaseResponseException
        {
            public CollectionManagerException(int? statusHint, string? message) : base(statusHint, message)
            {
                Source = nameof(CollectionManager);
            }

            public CollectionManagerException(int? statusHint, string? message, Exception? inner) : base(statusHint, message, inner)
            {
                Source = nameof(CollectionManager); 
            }
        }

        /// <summary>
        /// Return a count of all currently known collections 
        /// </summary>
        /// <returns></returns>
        public Task<int> CountCollectionsAsync();

        /// <summary>
        /// Return a total count of all currently known items
        /// </summary>
        /// <returns></returns>
        public Task<int> CountItemsAsync();

        /// <summary>
        /// Return a count of items for a specific collection
        /// </summary>
        /// <param name="collectionId"></param>
        /// <returns></returns>
        public Task<int> CountItemsAsync(Guid collectionId);

        /// <summary>
        /// Retrieves a list of all current collections
        /// </summary>
        /// <returns></returns>
        public Task<List<Collection>> ListCollections();

        /// <summary>
        /// Attempts the creation of a new <see cref="Collection"/> object, along with
        /// all associated VSP structures
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public Task<Collection> CreateCollection(CreateCollectionCommand cmd);

        /// <summary>
        /// Reads a specific collection from the underlying store, based on a supplied collection identifier
        /// </summary>
        /// <param name="collectionId">The unique GUID associated with the collection</param>
        /// <returns></returns>
        public Task<Collection> ReadCollection(Guid collectionId);

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
        public Task<Collection> UpdateCollection(Guid collectionId, PatchCollectionCommand cmd);

        /// <summary>
        /// Retrieves a list of currently configured <see cref="VSPBinding"/> instances
        /// </summary>
        /// <returns></returns>
        public List<VSPBinding> GetVSPBindings();
    }
}