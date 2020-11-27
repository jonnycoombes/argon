using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable 8618

namespace JCS.Argon.Model.Configuration
{
    public class VirtualStorageBinding
    {
        /// <summary>
        /// The tag for VSP provider
        /// </summary>
        [Required]
        public string Tag { get; set; }
        
        /// <summary>
        /// The provider type of the VSP provider 
        /// </summary>
        [Required]
        public string ProviderType { get; set; }
        
        /// <summary>
        /// A description for the VSP provider
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Generic set of properties for the provider, which will vary based on the underlying
        /// implementation
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        public VirtualStorageBinding()
        {
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}