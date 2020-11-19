using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JCS.Argon.Model.Configuration;
#pragma warning disable 8618

namespace JCS.Argon.Model.Responses
{
    public class ConfigurationResponse
    {

        /// <summary>
        /// The current hostname
        /// </summary>
        [Required]
        public string HostName { get; init; }
        
        /// <summary>
        /// The current root endpoint for the API
        /// </summary>
        [Required]
        public string Endpoint { get; init; }
        
        /// <summary>
        /// The internal version of the API
        /// </summary>
        [Required]
        public string Version { get; init; }
        
        /// <summary>
        /// A list of the currently configured VSP bindings 
        /// </summary>
        [Required]
        public List<VSPBinding> Bindings { get; init; }
        
    }
}