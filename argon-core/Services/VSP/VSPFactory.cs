using System.Collections.Generic;
using JCS.Argon.Model.Configuration;
using Microsoft.Extensions.Configuration;

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

        public VSPFactory(ApiConfiguration apiConfiguration)
        {
            _apiConfiguration = apiConfiguration;
        }
        
        public IEnumerable<VSPBindingOptions> GetConfigurations()
        {
            return _apiConfiguration.VspConfigurationOptions.Bindings;
        }

        public IVSPProvider GetProvider(string tag)
        {
            throw new System.NotImplementedException();
        }
    }
}