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
        private ApiConfiguration _apiConfiguration; 

        /// <summary>
        /// A list of <see cref="VSPBindingOptions" instances/>
        /// </summary>
        private List<VSPBindingOptions> _vspConfigurations = new List<VSPBindingOptions>();

        public VSPFactory(ApiConfiguration apiConfiguration)
        {
            _apiConfiguration = apiConfiguration;
        }
        
        public IEnumerable<VSPBindingOptions> GetConfigurations()
        {
            return new List<VSPBindingOptions>();
        }

        public IVSPProvider GetProvider(string tag)
        {
            throw new System.NotImplementedException();
        }
    }
}