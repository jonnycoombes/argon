using JCS.Argon.Contexts;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Services.Core
{
    /// <summary>
    /// Default implementation of a property manager
    /// </summary>
    public class PropertyManager : BaseCoreService, IPropertyManager
    {

        /// <summary>
        /// Default constructor, parameters are DI'd
        /// </summary>
        /// <param name="log"></param>
        /// <param name="dbContext"></param>
        public PropertyManager(ILogger<IPropertyManager> log, SqlDbContext dbContext)
        :base(log, dbContext)
        {
            _log.LogDebug("Creating new instance");
        }
    }
}