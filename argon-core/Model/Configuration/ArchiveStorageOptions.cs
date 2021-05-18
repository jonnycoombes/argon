using System.Collections.Generic;

namespace JCS.Argon.Model.Configuration
{
    /// <summary>
    ///     The configuration relating to archiving endpoints
    /// </summary>
    public class ArchiveStorageOptions
    {
        /// <summary>
        ///     The configuration section relating to these options
        /// </summary>
        public const string ConfigurationSection = "archiveStorageOptions";

        /// <summary>
        ///     All the currently configured bindings
        /// </summary>
        public List<ArchiveStorageBinding> Bindings { get; set; }
    }
}