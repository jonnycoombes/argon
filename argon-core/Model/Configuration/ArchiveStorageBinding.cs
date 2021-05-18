using System.ComponentModel.DataAnnotations;

namespace JCS.Argon.Model.Configuration
{
    /// <summary>
    ///     The binding for a specific archiving endpoint
    /// </summary>
    public class ArchiveStorageBinding
    {
        /// <summary>
        ///     The endpoint for the CWS service(s)
        /// </summary>
        [Required]
        public string Endpoint { get; set; }

        /// <summary>
        ///     The tag representing this particular archive binding
        /// </summary>
        [Required]
        public string Tag { get; set; }

        /// <summary>
        ///     An optional prefix which will be applied to paths presented to all archiving operations
        /// </summary>
        public string? PathPrefix { get; set; }

        /// <summary>
        ///     The user name to use if basic authentication is to be used
        /// </summary>
        public string? User { get; set; }

        /// <summary>
        ///     The password to use if basic authentication is to be used
        /// </summary>
        public string? Password { get; set; }
    }
}