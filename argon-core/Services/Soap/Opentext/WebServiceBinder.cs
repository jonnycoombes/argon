using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

namespace JCS.Argon.Services.Soap.Opentext
{
    /// <summary>
    ///     This class encapsulates the logic to bind to OTCS Content Web Services.  It is intended to be used by any classes/methods that need
    ///     to authenticate against various CWS services and then retrieve the underlying service interfaces
    /// </summary>
    public class WebServiceBinder
    {
        /// <summary>
        ///     The authentication type to be used. Two are supported, basic authentication (with username and password) or integrated
        ///     if the CWS endpoints are configured for IWA-based authentication
        /// </summary>
        public enum AuthenticationType
        {
            Basic,
            Integrated
        }

        /// <summary>
        ///     The authentication service suffix
        /// </summary>
        private const string AuthenticationServiceSuffix = "Authentication.svc";

        /// <summary>
        ///     The document management service suffix
        /// </summary>
        private const string DocumentManagementServiceSuffix = "DocumentManagement.svc";

        /// <summary>
        ///     Static logger for this class
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<WebServiceBinder>();

        /// <summary>
        ///     Instantiates an instance of the binder and configures it for <see cref="AuthenticationType.Basic" /> authentication
        /// </summary>
        /// <param name="endpoint">The endpoint prefix for the services to be bound</param>
        /// <param name="username">The user name to be used</param>
        /// <param name="password">The password to be used</param>
        public WebServiceBinder(string endpoint, string username, string password)
        {
            LogMethodCall(_log);
        }

        /// <summary>
        ///     Instantiates an instance of the binder and configures it for <see cref="AuthenticationType.Integrated" />
        ///     authentication
        /// </summary>
        /// <param name="endpoint">The endpoint prefix for the services to be bound</param>
        public WebServiceBinder(string endpoint)
        {
            LogMethodCall(_log);
        }

        /// <summary>
        ///     The current type of authentication to use.  The default is set to be integrated (IWA), so that no user or password information
        ///     is required within the binding information for the provider
        /// </summary>
        public AuthenticationType Authentication { get; set; } = AuthenticationType.Integrated;

        /// <summary>
        ///     The base endpoint address for the current bindings
        /// </summary>
        public string BaseEndpointAddress { get; set; }

        /// <summary>
        ///     The current user name (may be null
        /// </summary>
        public string? User { get; set; }

        /// <summary>
        ///     The current password
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        ///     The current <see cref="OTAuthentication" /> (only valid post-authentication)
        /// </summary>
        public OTAuthentication? Token { get; set; }
    }
}