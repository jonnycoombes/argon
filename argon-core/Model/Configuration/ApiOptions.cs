using Microsoft.Extensions.Options;

namespace JCS.Argon.Model.Configuration
{
    /// <summary>
    ///     This is the main options class for Argon.  Bound during application startup and passed through to most services wrapped inside a
    ///     <see cref="IOptionsMonitor{TOptions}" /> instance so that values can be altered at runtime if required
    /// </summary>
    public class ApiOptions
    {
        /// <summary>
        ///     The configuration section which the options are bound to
        /// </summary>
        public const string ConfigurationSection = "argon";

        /// <summary>
        ///     Whether or not the application is being hosted externally (i.e. within IIS)
        /// </summary>
        public bool ExternallyHosted { get; set; } = false;

        /// <summary>
        ///     The current virtual storage options
        /// </summary>
        public VirtualStorageOptions VirtualStorageOptions { get; set; } = null!;
    }
}