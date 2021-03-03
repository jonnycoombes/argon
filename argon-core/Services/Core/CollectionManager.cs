#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

#pragma warning disable 1574

namespace JCS.Argon.Services.Core
{
    /// <summary>
    ///     Class which manages most of the entity-related operations on <see cref="Collection" /> schema entities
    /// </summary>
    public class CollectionManager : BaseCoreService, ICollectionManager
    {
        /// <summary>
        ///     Static logging instance
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<CollectionManager>();

        /// <summary>
        ///     Default constructor, parameters are injected by the IoC layer
        /// </summary>
        /// <param name="options">The current system configuration</param>
        /// <param name="serviceProvider">The current DI <see cref="IServiceProvider" /></param>
        public CollectionManager(IOptionsMonitor<ApiOptions> options, IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {
            LogMethodCall(_log);
        }

        /// <inheritdoc cref="IDbCache.CountCollectionsAsync" />
        public async Task<int> CountCollectionsAsync()
        {
            LogMethodCall(_log);
            return await DbContext.Collections.CountAsync();
        }


        /// <inheritdoc cref="IDbCache.CountTotalItemsAsync" />
        public async Task<int> CountTotalItemsAsync()
        {
            LogMethodCall(_log);
            return await DbContext.Items.CountAsync();
        }

        /// <inheritdoc cref="IDbCache.CountTotalVersionsAsync" />
        public async Task<int> CountTotalVersionsAsync()
        {
            LogMethodCall(_log);
            return await DbContext.Versions.CountAsync();
        }

        /// <inheritdoc cref="ICollectionManager.ListCollectionsAsync" />
        public async Task<List<Collection>> ListCollectionsAsync()
        {
            LogMethodCall(_log);
            return await DbContext.Collections
                .OrderBy(c => c.Name)
                .Include(c => c.ConstraintGroup)
                .Include(c => c.ConstraintGroup!.Constraints)
                .Include(c => c.PropertyGroup)
                .Include(c => c.PropertyGroup!.Properties)
                .ToListAsync();
        }

        /// <inheritdoc cref="ICollectionManager.CreateCollectionAsync" />
        public async Task<Collection> CreateCollectionAsync(CreateCollectionCommand command)
        {
            LogMethodCall(_log);
            var exists = await CollectionExistsAsync(command.Name);
            if (exists)
            {
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status400BadRequest,
                    "A collection with that name already exists");
            }

            // create the necessary entities first
            ConstraintGroup? constraintGroup;
            if (command.Constraints != null)
            {
                constraintGroup = await ConstraintGroupManager.CreateConstraintGroupAsync(command.Constraints);
            }
            else
            {
                constraintGroup = await ConstraintGroupManager.CreateConstraintGroupAsync();
            }

            var propertyGroup = await PropertyGroupManager.CreatePropertyGroupAsync();
            var op = await DbContext.Collections.AddAsync(new Collection
            {
                Name = command.Name,
                Description = command.Description,
                ProviderTag = command.ProviderTag,
                ConstraintGroup = constraintGroup,
                PropertyGroup = propertyGroup
            });

            await CheckedContextSave();
            var collection = op.Entity;

            // grab the provider and then ask for the physical operations to be performed
            try
            {
                collection = await PerformProviderCollectionCreationActions(command, collection);
                DbContext.Update(collection);
                await CheckedContextSave();
            }
            catch (IVirtualStorageManager.VirtualStorageManagerException ex)
            {
                // roll back the entity changes
                LogWarning(_log, "Caught storage exception whilst attempting collection physical operation - rolling back db changes");
                DbContext.Collections.Remove(collection);
                await CheckedContextSave();
                throw new ICollectionManager.CollectionManagerException(ex.ResponseCodeHint,
                    ex.Message, ex);
            }
            catch (Exception ex)
            {
                // roll back the entity changes
                LogWarning(_log, "Caught general exception whilst attempting collection physical operation - rolling back db changes");
                DbContext.Collections.Remove(collection);
                await CheckedContextSave();
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status500InternalServerError,
                    ex.Message, ex);
            }

            return collection;
        }


        /// <inheritdoc cref="ICollectionManager.GetCollectionAsync" />
        public async Task<Collection> GetCollectionAsync(Guid collectionId)
        {
            LogMethodCall(_log);
            if (!await CollectionExistsAsync(collectionId))
            {
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status404NotFound,
                    "The specified collection does not exist");
            }

            var collection = await DbContext.Collections
                .Include(c => c.ConstraintGroup)
                .Include(c => c.ConstraintGroup!.Constraints)
                .Include(c => c.PropertyGroup)
                .Include(c => c.PropertyGroup!.Properties)
                .FirstAsync(c => c.Id == collectionId);
            await CommitCollectionAndUpdateLastAccessed(collection);
            return collection;
        }

        /// <inheritdoc cref="ICollectionManager.UpdateCollectionAsync" />
        public async Task<Collection> UpdateCollectionAsync(Guid collectionId, PatchCollectionCommand command)
        {
            LogMethodCall(_log);
            if (!await CollectionExistsAsync(collectionId))
            {
                throw new ICollectionManager.CollectionManagerException(404, "The specified collection does not exist");
            }

            var collection = await DbContext.Collections
                .Include(c => c.ConstraintGroup)
                .Include(c => c.ConstraintGroup!.Constraints)
                .Include(c => c.PropertyGroup)
                .Include(c => c.PropertyGroup!.Properties)
                .FirstAsync(c => c.Id == collectionId);

            if (collection == null)
            {
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status404NotFound,
                    "Collection has moved or cannot be found - shouldn't happen");
            }

            var validationErrors = await ValidateCollectionUpdateAsync(collection, command);
            if (validationErrors.Count != 0)
            {
                var message = validationErrors.Aggregate((s, t) => s + Environment.NewLine + t);
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status400BadRequest,
                    $"Validation errors occurred: {message}");
            }

            collection.Name = command.Name ?? collection.Name;
            collection.Description = command.Description ?? collection.Description;
            await CommitCollectionAndUpdateLastAccessed(collection);
            return collection;
        }

        /// <summary>
        ///     Returns the list of current <see cref="VirtualStorageBinding" /> instances
        /// </summary>
        /// <returns></returns>
        public List<VirtualStorageBinding> GetStorageBindings()
        {
            LogMethodCall(_log);
            return VirtualStorageManager.GetBindings();
        }

        /// <inheritdoc cref="ICollectionManager.UpdateCollectionConstraints" />
        public async Task<Collection> UpdateCollectionConstraints(Guid collectionId, List<CreateOrUpdateConstraintCommand> commands)
        {
            LogMethodCall(_log);
            if (!await CollectionExistsAsync(collectionId))
            {
                throw new ICollectionManager.CollectionManagerException(404, "The specified collection does not exist");
            }

            var collection = await GetCollectionAsync(collectionId);
            collection = await ConstraintGroupManager.UpdateAndMergeCollectionConstraints(collection, commands);
            collection = await CommitCollectionAndUpdateLastAccessed(collection);
            return collection;
        }

        /// <inheritdoc cref="ICollectionManager.TouchCollection" />
        public Task<Collection> TouchCollection(Collection collection)
        {
            return CommitCollectionAndUpdateLastAccessed(collection);
        }

        /// <inheritdoc cref="ICollectionManager.DeleteCollection" />
        public async Task DeleteCollection(Guid collectionId)
        {
            if (!await CollectionExistsAsync(collectionId))
            {
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status404NotFound,
                    "The specified collection does not exist");
            }

            var collection = await GetCollectionAsync(collectionId);
            if (await ItemManager.CountItemsAsync(collection) > 0)
            {
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status400BadRequest,
                    "The specified collection is not empty");
            }

            try
            {
                await PerformProviderCollectionDeletionActions(collection);
                DbContext.Collections.Remove(collection);
                await CheckedContextSave();
            }
            catch (Exception ex)
            {
                LogExceptionError(_log, ex);
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status500InternalServerError,
                    "An internal error occurred during collection deletion operation");
            }
        }

        /// <summary>
        ///     Interact with the appropriate <see cref="IVirtualStorageProvider" /> in order to delete a given collection
        /// </summary>
        /// <param name="collection">The <see cref="Collection" /> to be deleted </param>
        /// <returns></returns>
        private async Task PerformProviderCollectionDeletionActions(Collection collection)
        {
            LogMethodCall(_log);
            var provider = VirtualStorageManager.GetProviderByTag(collection.ProviderTag);
            await provider.DeleteCollectionAsync(collection);
        }

        /// <summary>
        ///     Update the last accessed time for a given <see cref="Collection" /> instance.  The current system (server-side) timestamp
        ///     is used.
        /// </summary>
        /// <param name="collection">The collection to update</param>
        /// <returns>The updated collection</returns>
        /// <exception cref="ICollectionManager.CollectionManagerException">Thrown in the event of a db commit fail</exception>
        private async Task<Collection> CommitCollectionAndUpdateLastAccessed(Collection collection)
        {
            try
            {
                LogMethodCall(_log);
                if (collection.PropertyGroup == null)
                {
                    return collection;
                }

                collection.PropertyGroup.AddOrReplaceProperty($"{Collection.StockCollectionProperties.LastAccessed}", PropertyType.DateTime,
                    DateTime.Now);
                DbContext.Collections.Update(collection);
                await CheckedContextSave();
            }
            catch (Exception ex)
            {
                LogExceptionWarning(_log, ex);
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status500InternalServerError,
                    "An exception occurred whilst attempting to commit a collection change");
            }

            return collection;
        }

        /// <summary>
        ///     Checks whether a given collection already exists within the database
        /// </summary>
        /// <param name="name"></param>
        /// <returns><code>true</code>if it exists, <code>false</code> otherwise</returns>
        private async Task<bool> CollectionExistsAsync(string name)
        {
            LogMethodCall(_log);
            if (!await DbContext.Collections.AnyAsync())
            {
                return false;
            }

            var existing = await DbContext.Collections
                .FirstOrDefaultAsync(c => c.Name.Equals(name));
            return !(existing is null);
        }

        /// <summary>
        ///     Checks whether a given collection already exists within the database
        /// </summary>
        /// <param name="collectionId">The unique GUID for the collection</param>
        /// <returns>true if it exists, false otherwise</returns>
        private async Task<bool> CollectionExistsAsync(Guid collectionId)
        {
            LogMethodCall(_log);
            if (await DbContext.Collections.AnyAsync())
            {
                var existing = await DbContext.Collections.FirstOrDefaultAsync(c => c.Id.Equals(collectionId));
                return !(existing is null);
            }

            LogWarning(_log, $"Failed to locate a collection with id \"{collectionId.ToString()}\"");
            return false;
        }

        /// <summary>
        ///     Performs the provider actions required for the creation of a new collection
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        /// <exception cref="ICollectionManager.CollectionManagerException">
        ///     Thrown if <see cref="IVirtualStorageProvider" /> throws
        ///     an error
        /// </exception>
        private async Task<Collection> PerformProviderCollectionCreationActions(CreateCollectionCommand cmd, Collection collection)
        {
            LogDebug(_log, $"Looking up a virtual storage provider with tag [{cmd.ProviderTag}");
            var provider = VirtualStorageManager.GetProviderByTag(cmd.ProviderTag);
            var creationResult = await provider.CreateCollectionAsync(collection);

            if (creationResult.Status != IVirtualStorageProvider.StorageOperationStatus.Ok)
            {
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status500InternalServerError,
                    $"Got a potentially retryable error whilst creating collection: {creationResult.ErrorMessage}");
            }

            if (creationResult.Properties != null)
            {
                collection.PropertyGroup!.MergeDictionary(creationResult.Properties);
            }

            return collection;
        }

        /// <summary>
        ///     Performs a number of checks to ensure that a collection update is valid
        /// </summary>
        /// <param name="target">The collection against which the validation is performed</param>
        /// <param name="cmd">A <see cref="PatchCollectionCommand" /> instance</param>
        /// <returns>A list of validation errors if any occur, otherwise an empty list</returns>
        private async Task<List<string>> ValidateCollectionUpdateAsync(Collection target, PatchCollectionCommand cmd)
        {
            LogMethodCall(_log);
            var validationErrors = new List<string>();
            if (cmd.Name == null)
            {
                return validationErrors;
            }

            if (cmd.Name.Length == 0)
            {
                validationErrors.Add("A collection cannot have an empty name");
            }

            if (target.Name == cmd.Name)
            {
                return validationErrors;
            }

            var exists = await CollectionExistsAsync(cmd.Name);
            if (exists)
            {
                validationErrors.Add("A collection with the supplied name already exists");
            }

            return validationErrors;
        }
    }
}