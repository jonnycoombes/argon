using System;
using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Services.Core
{
    /// <summary>
    /// Default implementation of a property manager
    /// </summary>
    public class PropertyGroupManager : BaseCoreService, IPropertyGroupManager
    {

        /// <summary>
        /// Default constructor, parameters are DI'd
        /// </summary>
        /// <param name="log"></param>
        /// <param name="dbContext"></param>
        public PropertyGroupManager(ILogger<IPropertyGroupManager> log, SqlDbContext dbContext)
        :base(log, dbContext)
        {
            _log.LogDebug("Creating new instance");
        }

        /// <inheritdoc cref="IPropertyGroupManager.CreatePropertyGroupAsync"/>
        public async Task<PropertyGroup> CreatePropertyGroupAsync()
        {
            try
            {
                var addOp = await _dbContext.AddAsync(new PropertyGroup());
                await _dbContext.SaveChangesAsync();
                return addOp.Entity;
            }
            catch (Exception ex)
            {
                throw new IPropertyGroupManager.PropertyGroupManagerException(StatusCodes.Status500InternalServerError,
                    "Failed to create new property group", ex);
            }
        }
    }
}