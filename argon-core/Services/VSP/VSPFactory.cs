using System.Collections.Generic;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Services.VSP
{
    /// <summary>
    /// Default implementation of the VSP registry
    /// </summary>
    public class VSPFactory  : IVSPFactory
    {
        /// <summary>
        /// The overall application configuration, used to extract VSP bindings
        /// </summary>
        private readonly ApiConfiguration _apiConfiguration;

        /// <summary>
        /// Logger for logging
        /// </summary>
        private readonly ILogger<VSPFactory> _log;

        /// <summary>
        /// The currently active db context
        /// </summary>
        private readonly SqlDbContext _dbContext;

        public VSPFactory(ILogger<VSPFactory> log, ApiConfiguration apiConfiguration, SqlDbContext dbContext)
        {
            log.LogDebug("Creating new instance");
            _apiConfiguration = apiConfiguration;
            _log = log;
            _dbContext = dbContext;
            _apiConfiguration.VspConfiguration.DumpToLog(_log);
        }
        
        public IEnumerable<VSPBinding> GetConfigurations()
        {
            return _apiConfiguration.VspConfiguration.Bindings;
        }

        public IVSPProvider GetProvider(string tag)
        {
            _log.LogDebug($"Attempting instantiation of VSP provider with tag [{tag}]");
            throw new System.NotImplementedException();
        }
    }
}