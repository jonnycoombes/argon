namespace JCS.Argon.Model.Configuration
{
    public class ApiOptions
    {
        public const string ConfigurationSection = "argon";

        /// <summary>
        /// Whether or not the application is being hosted externally (i.e. within IIS)
        /// </summary>
        public bool ExternallyHosted { get; set; } = false; 

        /// <summary>
        /// The current virtual storage options
        /// </summary>
        public VirtualStorageOptions VirtualStorageOptions { get; set; } = null!;
    }
}