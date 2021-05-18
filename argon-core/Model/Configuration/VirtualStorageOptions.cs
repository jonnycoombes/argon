#region

using System.Collections.Generic;

#endregion

namespace JCS.Argon.Model.Configuration
{
    /// <summary>
    ///     Options class for the set of all currently configured <see cref="VirtualStorageBinding" /> instances
    /// </summary>
    public class VirtualStorageOptions
    {
        /// <summary>
        ///     Section of the configuration file to which these options may be bound
        /// </summary>
        public const string ConfigurationSection = "virtualStorageOptions";

        /// <summary>
        ///     The list of all <see cref="VirtualStorageBinding" /> instances
        /// </summary>
        public List<VirtualStorageBinding> Bindings { get; set; }
    }
}