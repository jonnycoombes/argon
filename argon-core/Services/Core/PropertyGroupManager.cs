using System;
using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

namespace JCS.Argon.Services.Core
{
    /// <summary>
    /// Default implementation of a property manager
    /// </summary>
    public class PropertyGroupManager : BaseCoreService, IPropertyGroupManager
    {
        /// <summary>
        /// Static logger
        /// </summary>
        private static ILogger _log = Log.ForContext<Core.PropertyGroupManager>();
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="serviceProvider"></param>
        public PropertyGroupManager(IOptionsMonitor<ApiConfiguration> options, IServiceProvider serviceProvider)
        :base(options, serviceProvider)
        {
            LogMethodCall(_log);
        }

        /// <inheritdoc cref="IPropertyGroupManager.CreatePropertyGroupAsync"/>
        public async Task<PropertyGroup> CreatePropertyGroupAsync()
        {
            LogMethodCall(_log);
            try
            {
                var addOp = await DbContext.AddAsync(new PropertyGroup());
                await DbContext.SaveChangesAsync();
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