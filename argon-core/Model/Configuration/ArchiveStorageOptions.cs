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
        ///     The maximum number of concurrent requests to be executed for a single archive operation
        /// </summary>
        public uint MaxConcurrentRequests = 4;

        /// <summary>
        ///     All the currently configured bindings
        /// </summary>
        public List<ArchiveStorageBinding> Bindings { get; set; }
    }
}