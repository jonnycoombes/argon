using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.Soap.Opentext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;
using Version = JCS.Argon.Services.Soap.Opentext.Version;

namespace JCS.Argon.Services.Core
{
    /// <summary>
    ///     The default implementation of the <see cref="IArchiveManager" /> interface. The intention
    /// </summary>
    public class ArchiveManager : BaseCoreService, IArchiveManager
    {
        /// <summary>
        ///     The default path separator which is used to split paths
        /// </summary>
        private const string PathSeperator = "/";

        /// <summary>
        ///     The default root node id
        /// </summary>
        private const long RootId = 2000;

        /// <summary>
        ///     The static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<ArchiveManager>();

        /// <summary>
        ///     The current set of options
        /// </summary>
        private readonly ArchiveStorageOptions _options;

        /// <summary>
        ///     The instance of <see cref="WebServiceClient" /> used to interact with the underlying CWS layer
        /// </summary>
        private WebServiceClient? _client;

        /// <summary>
        ///     The default constructor
        /// </summary>
        /// <param name="options">The current set of <see cref="ApiOptions" /> injected by  the DI container</param>
        /// <param name="serviceProvider">The current DI <see cref="IServiceProvider" /> instance, injected by the DI container</param>
        public ArchiveManager(IOptionsMonitor<ApiOptions> options, IServiceProvider serviceProvider) : base(options, serviceProvider)
        {
            LogMethodCall(_log);
            _options = options.CurrentValue.ArchiveStorageOptions;
        }

        /// <inheritdoc cref="IArchiveManager.DownloadArchivedDocument" />
        public async Task<(Version, Stream)> DownloadArchivedDocument(string tag, string path)
        {
            LogMethodCall(_log);
            var client = ResolveWebServiceClient(tag);
            try
            {
                var elements = path.Split(PathSeperator);
                var node = await client.GetNodeByPath(2000, elements);
                if (node == null)
                {
                    throw new ArchiveManagerException(StatusCodes.Status404NotFound, "The specified item cannot be found in the archive");
                }

                if (node.IsContainer)
                {
                    throw new ArchiveManagerException(StatusCodes.Status400BadRequest, "The specified item is a container");
                }

                var version = await client.GetItemVersion(node.ID, node.VersionInfo.VersionNum);
                var attachment = await client.GetVersionContents(node.ID, node.VersionInfo.VersionNum);
                return (version, new MemoryStream(attachment.Contents));
            }
            catch (WebServiceClientException ex)
            {
                throw new ArchiveManagerException(StatusCodes.Status500InternalServerError,
                    "An error occurred whilst attempting to retrieve the specified item");
            }
        }

        /// <inheritdoc cref="IArchiveManager.DownloadArchivedMetadata" />
        public async Task<JsonDocument> DownloadArchivedMetadata(string tag, string path)
        {
            LogMethodCall(_log);
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Resolves and instantiates a new instance of <see cref="WebServiceClient" /> based on the binding tag supplied
        /// </summary>
        /// <param name="tag">The binding tag used to look up the client settings</param>
        /// <returns>A new instance of <see cref="WebServiceClient" /></returns>
        /// <exception cref="ArchiveManagerException">Thrown in the event of a fault occurring</exception>
        private WebServiceClient ResolveWebServiceClient(string tag)
        {
            LogMethodCall(_log);
            if (_options == null)
            {
                throw new WebServiceClientException("The system is not currently configured for archiving - please check configuration");
            }

            if (_options.Bindings.Any(b => b.Tag == tag))
            {
                var binding = _options.Bindings.First(b => b.Tag == tag);
                if (!string.IsNullOrEmpty(binding.User) && !string.IsNullOrEmpty(binding.Password))
                {
                    return new WebServiceClient(binding.Endpoint, binding.User, binding.Password);
                }

                return new WebServiceClient(binding.Endpoint);
            }

            throw new ArchiveManagerException(StatusCodes.Status400BadRequest, $"No archive binding  found for tag \"{tag}\"");
        }
    }
}