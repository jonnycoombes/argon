#region

using System;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

namespace JCS.Argon.Services.Core
{
    /// <summary>
    ///     Default implementation of a property manager.  Responsible for managing db changes relating to <see cref="PropertyGroup" />
    ///     and <see cref="Property" /> schema entities
    /// </summary>
    public class PropertyGroupManager : BaseCoreService, IPropertyGroupManager
    {
        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<PropertyGroupManager>();

        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="serviceProvider"></param>
        public PropertyGroupManager(IOptionsMonitor<ApiOptions> options, IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {
            LogMethodCall(_log);
        }

        /// <inheritdoc cref="IPropertyGroupManager.CreatePropertyGroupAsync" />
        public async Task<PropertyGroup> CreatePropertyGroupAsync()
        {
            LogMethodCall(_log);
            try
            {
                var addOp = await DbContext.AddAsync(new PropertyGroup());
                await CheckedContextSave();
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