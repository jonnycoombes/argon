using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable 8618

namespace JCS.Argon.Model.Configuration
{
    public class VSPBindingOptions
    {
        /// <summary>
        /// The tag for VSP provider
        /// </summary>
        public string Tag { get; set; }
        
        /// <summary>
        /// The provider type of the VSP provider 
        /// </summary>
        public string ProviderType { get; set; }
        
        /// <summary>
        /// A description for the VSP provider
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// The class that implements this particular provider
        /// </summary>
        public string ProviderClass { get; set; }

        /// <summary>
        /// Generic set of properties for the provider, which will vary based on the underlying
        /// implementation
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        public VSPBindingOptions()
        {
            
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}