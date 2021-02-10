#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Model.Schema;

#endregion

namespace JCS.Argon.Services.Core
{
    /// <summary>
    ///     Service responsible for top-level collection-related operations
    /// </summary>
    public interface ICollectionManager
    {
        /// <summary>
        ///     Retrieves a list of all current collections
        /// </summary>
        /// <returns>A list of <see cref="Collection" /> objects</returns>
        public Task<List<Collection>> ListCollectionsAsync();

        /// <summary>
        ///     Return a count of all currently known collections
        /// </summary>
        /// <returns>An integer count of the collections</returns>
        public Task<int> CountCollectionsAsync();

        /// <summary>
        ///     Count the overall total number of items stored within this instance of Argon
        /// </summary>
        /// <returns>An integer count</returns>
        public Task<int> CountTotalItemsAsync();

        /// <summary>
        ///     Count the overall number of versions stored within this instance of Argon
        /// </summary>
        /// <returns>An integer count</returns>
        public Task<int> CountTotalVersionsAsync();

        /// <summary>
        ///     Attempts the creation of a new <see cref="Collection" /> object, along with
        ///     all associated VSP structures
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="command">The <see cref="CreateCollectionCommand" /> containing the parameters for the new collection</param>
        /// <returns>A new <see cref="Collection" /> instance</returns>
        /// <exception cref="CollectionManagerException">In the event of something going wrong during the creation process</exception>
        public Task<Collection> CreateCollectionAsync(CreateCollectionCommand command);

        /// <summary>
        ///     Reads a specific collection from the underlying store, based on a supplied collection identifier
        /// </summary>
        /// <param name="collectionId">The unique GUID associated with the collection</param>
        /// <returns>A <see cref="Collection" /> instance</returns>
        /// <exception cref="CollectionManagerException">In the event of something going wrong during the retrieval process</exception>
        public Task<Collection> GetCollectionAsync(Guid collectionId);

        /// <summary>
        ///     Attempts to update the meta-data associated with a given <see cref="Collection" /> instance,
        ///     along with all associated VSP structures
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <param name="command">The things to change</param>
        /// <returns>The updated <see cref="Collection" /></returns>
        /// <exception cref="CollectionManagerException">In the event of something going wrong during the update process</exception>
        public Task<Collection> UpdateCollectionAsync(Guid collectionId, PatchCollectionCommand command);

        /// <summary>
        ///     Performs an update (create/update) on the <see cref="ConstraintGroup" /> for a given <see cref="Collection" />.  This operation
        ///     does not allow for the deletion of constraints, this is dealt with through a separate operation.
        /// </summary>
        /// <param name="collectionId">The id for the collection to update</param>
        /// <param name="commands">A list of <see cref="CreateOrUpdateConstraintCommand" /> commands</param>
        /// <returns></returns>
        public Task<Collection> UpdateCollectionConstraints(Guid collectionId, List<CreateOrUpdateConstraintCommand> commands);

        /// <summary>
        ///     Retrieves a list of currently configured <see cref="VirtualStorageBinding" /> instances
        /// </summary>
        /// <returns>
        ///     A list of <see cref="VirtualStorageBinding" /> instances.  These are basically taken from the current system
        ///     configuration
        /// </returns>
        public List<VirtualStorageBinding> GetStorageBindings();

        /// <summary>
        ///     Thrown in the event of a failure within the collection manager
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
    }
}