using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Helpers;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.VSP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Services.Core
{
    public class CollectionManager  : BaseCoreService, ICollectionManager
    {

        /// <summary>
        /// The currently configured <see cref="IVSPManager"/> instance
        /// </summary>
        protected IVSPManager _vspManager;

        /// <summary>
        /// The currently scoped <see cref="IPropertyGroupManager"/> instance
        /// </summary>
        protected IPropertyGroupManager _propertyGroupManager;

        /// <summary>
        /// The current scoped <see cref="IConstraintGroupManager"/> instance
        /// </summary>
        protected IConstraintGroupManager _constraintGroupManager;

        /// <summary>
        /// Default constructor, parameters are DI'd by the IoC layer
        /// </summary>
        /// <param name="log"></param>
        /// <param name="dbContext"></param>
        /// <param name="vspManager"></param>
        /// <param name="propertyGroupManager">A scoped implementation of a <see cref="IPropertyGroupManager"/></param>
        /// <param name="constraintGroupManager">A scope implementation of a <see cref="IConstraintGroupManager"/></param>
        public CollectionManager(ILogger<CollectionManager> log, SqlDbContext dbContext, 
            IVSPManager vspManager, 
            IPropertyGroupManager propertyGroupManager,
            IConstraintGroupManager constraintGroupManager)
        :base(log, dbContext)
        {
            _vspManager = vspManager;
            _propertyGroupManager = propertyGroupManager;
            _constraintGroupManager = constraintGroupManager;
            _log.LogDebug("Creating new instance");
        }
        
        public async Task<int> CountCollectionsAsync()
        {
            return await _dbContext.Collections.CountAsync();
        }

        public Task<int> CountItemsAsync()
        {
            throw new System.NotImplementedException();
        }

#pragma warning disable 1574
        /// <inheritdoc cref="ICollectionManager.CountItemsAsync" />
#pragma warning restore 1574
        public async Task<int> CountItemsAsync(Guid collectionId)
        {
            if (!await CollectionExistsAsync(collectionId))
            {
                throw new ICollectionManager.CollectionManagerException(500, "The specified collection does not exist");
            }
            else
            {
                return await _dbContext.Items.CountAsync(c => c.Collection.Id == collectionId);
            }
        }

        /// <inheritdoc cref="ICollectionManager.ListCollectionsAsync" />
        public async Task<List<Collection>> ListCollectionsAsync()
        {
            return  await _dbContext.Collections
                .Include(c => c.ConstraintGroup)
                .Include(c =>c.ConstraintGroup.Constraints)
                .Include(c => c.PropertyGroup)
                .Include(c => c.PropertyGroup.Properties)
                .ToListAsync();
        }

        /// <summary>
        /// Checks whether a given collection already exists within the database
        /// </summary>
        /// <param name="name"></param>
        /// <returns><code>true</code>if it exists, <code>false</code> otherwise</returns>
        protected async Task<bool> CollectionExistsAsync(string name)
        {
            if (await _dbContext.Collections.AnyAsync())
            {
                var existing = await _dbContext.Collections
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
            if (await _dbContext.Collections.AnyAsync())
            {
                var existing = await _dbContext.Collections.FirstOrDefaultAsync(c => c.Id.Equals(collectionId));
                return !(existing is null);
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc cref="ICollectionManager.CreateCollectionAsync"/>
        public async Task<Collection> CreateCollectionAsync(CreateCollectionCommand cmd)
        {
                var exists = await CollectionExistsAsync(cmd.Name);
                
                if (!exists)
                {
                    ConstraintGroup? constraintGroup;
                    PropertyGroup? propertyGroup;
                    
                    if (cmd.Constraints != null)
                    {
                        constraintGroup = await _constraintGroupManager.CreateConstraintGroupAsync(cmd.Constraints);
                    }
                    else
                    {
                        constraintGroup = await _constraintGroupManager.CreateConstraintGroupAsync();
                    }

                    propertyGroup = await _propertyGroupManager.CreatePropertyGroupAsync();
                    
                    var collection = await _dbContext.Collections.AddAsync(new Collection()
                    {
                        Name = cmd.Name,
                        Description = cmd.Description,
                        ProviderTag = cmd.ProviderTag,
                        ConstraintGroup = constraintGroup,
                        PropertyGroup = propertyGroup
                    });
                    
                    await _dbContext.SaveChangesAsync();
                    return collection.Entity;
                }
                else
                {
                    throw new ICollectionManager.CollectionManagerException(400, "A collection with that name already exists");
                }
        }

        /// <inheritdoc cref="ICollectionManager.ReadCollectionAsync"/> 
        public async Task<Collection> ReadCollectionAsync(Guid collectionId)
        {
            if (await CollectionExistsAsync(collectionId))
            {
                return await _dbContext.Collections
                    .Include(c => c.ConstraintGroup)
                    .Include(c => c.ConstraintGroup.Constraints)
                    .Include(c => c.PropertyGroup)
                    .Include(c => c.PropertyGroup.Properties)
                    .FirstAsync(c => c.Id == collectionId);
            }
            else
            {
                throw new ICollectionManager.CollectionManagerException(404, "The specified collection does not exist");
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
        
        /// <inheritdoc cref="ICollectionManager.UpdateCollectionAsync"/>
        public async Task<Collection> UpdateCollectionAsync(Guid collectionId, PatchCollectionCommand cmd)
        {
            if (!await CollectionExistsAsync(collectionId))
            {
                throw new ICollectionManager.CollectionManagerException(404, "The specified collection does not exist");
            }
            else
            {
                var collection = await _dbContext.Collections.FirstAsync(c => c.Id == collectionId);
                if (collection != null)
                {
                    var validationErrors = await ValidateCollectionUpdateAsync(collection, cmd);
                    if (validationErrors.Count == 0)
                    {
                        collection.Name = cmd.Name ?? collection.Name;
                        collection.Description = cmd.Description ?? collection.Description;
                        _dbContext.Collections.Update(collection);
                        await _dbContext.SaveChangesAsync();
                        return collection;
                    }
                    else
                    {
                        throw new ICollectionManager.CollectionManagerException(400,
                            $"Validation errors occurred: {StringHelper.CollapseStringList(validationErrors)}");
                    }
                }
                throw new ICollectionManager.CollectionManagerException(500,
                    "Collection has moved or cannot be found - shouldn't happen");
            }
        }

        /// <summary>
        /// Returns the list of current <see cref="VSPBinding"/> instances
        /// </summary>
        /// <returns></returns>
        public List<VSPBinding> GetVSPBindings()
        {
            return _vspManager.GetBindings();
        }
    }
}