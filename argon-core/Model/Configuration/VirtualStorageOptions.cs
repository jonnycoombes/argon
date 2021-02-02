#region

using System.Collections.Generic;

#endregion

#pragma warning disable 8618

namespace JCS.Argon.Model.Configuration
{
    public class VirtualStorageOptions
    {
        /// <summary>
        ///     Section of the configuration file to which these options may be bound
        /// </summary>
        public const string ConfigurationSection = "vsp";

        public List<VirtualStorageBinding> Bindings { get; set; }
    }
}