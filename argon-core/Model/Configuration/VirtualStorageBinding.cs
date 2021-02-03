#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

#endregion

#pragma warning disable 8618

namespace JCS.Argon.Model.Configuration
{
    /// <summary>
    ///     Options class which is bound during startup.  Contains the options for various providers.
    /// </summary>
    public class VirtualStorageBinding
    {
        /// <summary>
        ///     The tag for VSP provider
        /// </summary>
        [Required]
        public string Tag { get; set; }

        /// <summary>
        ///     The provider type of the VSP provider
        /// </summary>
        [Required]
        public string ProviderType { get; set; }

        /// <summary>
        ///     A description for the VSP provider
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Generic set of properties for the provider, which will vary based on the underlying
        ///     implementation.  The native filesystem provider will have a different set of properties from the Otcs provider for
        ///     example.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        ///     This override just converts the options to Json.  Useful returning the options in response to an API call
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}