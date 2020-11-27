using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JCS.Argon.Model.Configuration;
#pragma warning disable 8618

namespace JCS.Argon.Model.Responses
{
    public class Metrics
    {
        /// <summary>
        /// The current total number of managed collections
        /// </summary>
        public int TotalCollections { get; set; } = 0;

        /// <summary>
        /// The current total number of managed documents
        /// </summary>
        public int TotalDocuments { get; set; } = 0;
    }
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
        /// The internal schema version of the API
        /// </summary>
        [Required]
        public string SchemaVersion { get; init; }
        
        /// <summary>
        /// A list of the currently configured VSP bindings 
        /// </summary>
        [Required]
        public List<VirtualStorageBinding> Bindings { get; init; }
        
        /// <summary>
        /// The current metrics for the instance
        /// </summary>
        public Metrics Metrics { get; set; }
        
    }
}