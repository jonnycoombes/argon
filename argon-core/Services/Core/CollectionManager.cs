using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Utility;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#pragma warning disable 1574

namespace JCS.Argon.Services.Core
{
    public class CollectionManager : BaseCoreService, ICollectionManager
    {
        /// <summary>
        /// Static logging instance
        /// </summary>
        private static ILogger _log = Log.ForContext<CollectionManager>();
        
        /// <summary>
        /// Default constructor, parameters are DI'd by the IoC layer
        /// </summary>
        /// <param name="options">The current system configuration</param>
        /// <param name="serviceProvider">The current DI <see cref="IServiceProvider"/></param>
        public CollectionManager(IOptionsMonitor<ApiConfiguration> options, IServiceProvider serviceProvider) 
            : base(options, serviceProvider)
        {
            LogMethodCall(_log);
        }

        public async Task<int> CountCollectionsAsync()
        {
            LogMethodCall(_log);
            return await DbContext.Collections.CountAsync();
        }

        public async Task<int> CountTotalItemsAsync()
        {
            LogMethodCall(_log);
            return await DbContext.Items.CountAsync();
        }

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
                .Include(c => c.ConstraintGroup)
                .Include(c => c.ConstraintGroup!.Constraints)
                .Include(c => c.PropertyGroup)
                .Include(c => c.PropertyGroup!.Properties)
                .ToListAsync();
        }

        /// <inheritdoc cref="ICollectionManager.CreateCollectionAsync"/>
        public async Task<Collection> CreateCollectionAsync(CreateCollectionCommand cmd)
        {
            LogMethodCall(_log);
            var exists = await CollectionExistsAsync(cmd.Name);
            if (!exists)
            {
                // create the necessary entities first
                ConstraintGroup? constraintGroup;
                if (cmd.Constraints != null)
                {
                    constraintGroup = await ConstraintGroupManager.CreateConstraintGroupAsync(cmd.Constraints);
                }
                else
                {
                    constraintGroup = await ConstraintGroupManager.CreateConstraintGroupAsync();
                }

                var propertyGroup = await PropertyGroupManager.CreatePropertyGroupAsync();
                var addOp = await DbContext.Collections.AddAsync(new Collection()
                {
                    Name = cmd.Name,
                    Description = cmd.Description,
                    ProviderTag = cmd.ProviderTag,
                    ConstraintGroup = constraintGroup,
                    PropertyGroup = propertyGroup
                });

                await DbContext.SaveChangesAsync();
                var collection = addOp.Entity;

                // grab the provider and then ask for the physical operations to be performed
                try
                {
                    collection = await PerformProviderCollectionCreationActions(cmd, collection);
                    DbContext.Update(collection);
                    await DbContext.SaveChangesAsync();
                }
                catch (IVirtualStorageManager.VirtualStorageManagerException ex)
                {
                    // roll back the entity changes
                    LogWarning(_log, $"Caught storage exception whilst attempting collection physical operation - rolling back db changes");
                    DbContext.Collections.Remove(collection);
                    await DbContext.SaveChangesAsync();
                    throw new ICollectionManager.CollectionManagerException(ex.ResponseCodeHint,
                        ex.Message, ex);
                }
                catch (Exception ex)
                {
                    // roll back the entity changes
                    LogWarning(_log, $"Caught general exception whilst attempting collection physical operation - rolling back db changes");
                    DbContext.Collections.Remove(collection);
                    await DbContext.SaveChangesAsync();
                    throw new ICollectionManager.CollectionManagerException(StatusCodes.Status500InternalServerError,
                        ex.Message, ex);
                }

                return collection;
            }
            else
            {
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status400BadRequest,
                    "A collection with that name already exists");
            }
        }


        /// <inheritdoc cref="ICollectionManager.GetCollectionAsync"/> 
        public async Task<Collection> GetCollectionAsync(Guid collectionId)
        {
            LogMethodCall(_log);
            if (await CollectionExistsAsync(collectionId))
            {
                return await DbContext.Collections
                    .Include(c => c.ConstraintGroup)
                    .Include(c => c.ConstraintGroup!.Constraints)
                    .Include(c => c.PropertyGroup)
                    .Include(c => c.PropertyGroup!.Properties)
                    .FirstAsync(c => c.Id == collectionId);
            }
            else
            {
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status404NotFound,
                    "The specified collection does not exist");
            }
        }

        /// <inheritdoc cref="ICollectionManager.UpdateCollectionAsync"/>
        public async Task<Collection> UpdateCollectionAsync(Guid collectionId, PatchCollectionCommand cmd)
        {
            LogMethodCall(_log);
            if (!await CollectionExistsAsync(collectionId))
            {
                throw new ICollectionManager.CollectionManagerException(404, "The specified collection does not exist");
            }
            else
            {
                var collection = await DbContext.Collections.FirstAsync(c => c.Id == collectionId);
                if (collection != null)
                {
                    var validationErrors = await ValidateCollectionUpdateAsync(collection, cmd);
                    if (validationErrors.Count == 0)
                    {
                        collection.Name = cmd.Name ?? collection.Name;
                        collection.Description = cmd.Description ?? collection.Description;
                        DbContext.Collections.Update(collection);
                        await DbContext.SaveChangesAsync();
                        return collection;
                    }
                    else
                    {
                        throw new ICollectionManager.CollectionManagerException(StatusCodes.Status400BadRequest,
                            $"Validation errors occurred: {StringHelper.CollapseStringList(validationErrors)}");
                    }
                }

                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status404NotFound,
                    "Collection has moved or cannot be found - shouldn't happen");
            }
        }

        /// <summary>
        /// Returns the list of current <see cref="VirtualStorageBinding"/> instances
        /// </summary>
        /// <returns></returns>
        public List<VirtualStorageBinding> GetStorageBindings()
        {
            LogMethodCall(_log);
            return VirtualStorageManager.GetBindings();
        }

        /// <summary>
        /// Checks whether a given collection already exists within the database
        /// </summary>
        /// <param name="name"></param>
        /// <returns><code>true</code>if it exists, <code>false</code> otherwise</returns>
        protected async Task<bool> CollectionExistsAsync(string name)
        {
            LogMethodCall(_log);
            if (await DbContext.Collections.AnyAsync())
            {
                var existing = await DbContext.Collections
                    .FirstOrDefaultAsync(c => c.Name.Equals(name));
                return !(existing is null);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether a given collection already exists within the database
        /// </summary>
        /// <param name="collectionId">The unique GUID for the collection</param>
        /// <returns><code>true</code>if it exists, <code>false</code> otherwise</returns>
        protected async Task<bool> CollectionExistsAsync(Guid collectionId)
        {
            LogMethodCall(_log);
            if (await DbContext.Collections.AnyAsync())
            {
                var existing = await DbContext.Collections.FirstOrDefaultAsync(c => c.Id.Equals(collectionId));
                return !(existing is null);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Performs the provider actions required for the creation of a new collection
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        /// <exception cref="ICollectionManager.CollectionManagerException"></exception>
        private async Task<Collection> PerformProviderCollectionCreationActions(CreateCollectionCommand cmd, Collection collection)
        {
            LogDebug(_log, $"Looking up a virtual storage provider with tag [{cmd.ProviderTag}");
            var provider = VirtualStorageManager.GetProvider(cmd.ProviderTag);
            var creationResult = await provider.CreateCollectionAsync(collection);
            if (creationResult.Status == IVirtualStorageProvider.StorageOperationStatus.Ok)
            {
                if (creationResult.Properties != null)
                {
                    collection.PropertyGroup!.MergeDictionary(creationResult.Properties);
                }

                return collection;
            }
            else
            {
                throw new ICollectionManager.CollectionManagerException(StatusCodes.Status500InternalServerError,
                    $"Got a potentially retryable error whilst creating collection: {creationResult.ErrorMessage}");
            }
        }

        /// <summary>
        /// Performs a number of checks to ensure that a collection update is valid
        /// </summary>
        /// <param name="target">The collection against which the validation is performed</param>
        /// <param name="cmd">A <see cref="PatchCollectionCommand"/> instance</param>
        /// <returns>A list of validation errors if any occur, otherwise an empty list</returns>
        protected async Task<List<string>> ValidateCollectionUpdateAsync(Collection target, PatchCollectionCommand cmd)
        {
            LogMethodCall(_log);
            List<string> validationErrors = new List<string>();
            if (cmd.Name != null)
            {
                if (target.Name != cmd.Name)
                {
                    var exists = await CollectionExistsAsync(cmd.Name);
                    if (exists)
                    {
                        validationErrors.Add("A collection with the supplied name already exists");
                    }
                }
            }

            return validationErrors;
        }
    }
}