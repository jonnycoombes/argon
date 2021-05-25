using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.Soap.Opentext;
using JCS.Argon.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute.Routing.Handlers;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

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
        public async Task<IArchiveManager.DownloadContentResult> DownloadArchivedDocument(string tag, string path,
            IArchiveManager.ArchiveType archiveType = IArchiveManager.ArchiveType.ZipArchive)
        {
            LogMethodCall(_log);
            var client = BindWebServiceClient(tag);
            try
            {
                var elements = path.Split(PathSeperator);
                var node = await client.GetNodeByPath(RootId, elements);
                if (node == null)
                {
                    throw new ArchiveManagerException(StatusCodes.Status404NotFound, "The specified item cannot be found in the archive");
                }

                if (node.IsContainer)
                {
                    LogVerbose(_log, $"Processing archive download request for path location \"{path}\"");
                    var items = (await client.GetChildren(node.ID))
                        .Where(n => n.IsContainer == false)
                        .Select(n => (n.ID, n.VersionInfo.VersionNum)).ToArray();
                    return await DownloadNodeArchive(client, items, archiveType);
                }

                return await DownloadNodeVersion(client, node.ID, node.VersionInfo.VersionNum);
            }
            catch (WebServiceClientException ex)
            {
                throw new ArchiveManagerException(StatusCodes.Status500InternalServerError,
                    "An error occurred whilst attempting to retrieve the specified item");
            }
        }

        /// <inheritdoc cref="IArchiveManager.DownloadArchivedMetadata" />
        public async Task<string> DownloadArchivedMetadata(string tag, string path)
        {
            LogMethodCall(_log);
            var client = BindWebServiceClient(tag);

            try
            {
                var elements = path.Split(PathSeperator);
                var node = await client.GetNodeByPath(2000, elements);
                if (node == null)
                {
                    throw new ArchiveManagerException(StatusCodes.Status404NotFound, "The specified item cannot be found in the archive");
                }

                Node[]? children = null;
                if (node.IsContainer)
                {
                    children = await client.GetChildren(node.ID);
                }

                return JsonHelper.OpenTextNodeToJson(node, children, JsonHelper.OpenTextDefaultNodeSerialisationFunction);
            }
            catch (WebServiceClientException ex)
            {
                throw new ArchiveManagerException(StatusCodes.Status500InternalServerError,
                    "An error occurred whilst attempting to retrieve the specified item");
            }
        }

        /// <summary>
        ///     Downloads multiple node versions.  This is done through the use of a semaphore in order to control the number of concurrent
        ///     tasks (requests) executing at any one time.
        /// </summary>
        /// <param name="client">The <see cref="WebServiceClient" /> instance to use</param>
        /// <param name="items">A list of pairs, consisting of a node identifier and a version number</param>
        /// <param name="archiveType">The type of archive file to produce</param>
        /// <returns>
        ///     A <see cref="IArchiveManager.DownloadContentResult" /> which contains information and payload associated with
        ///     the resultant archive file
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<IArchiveManager.DownloadContentResult> DownloadNodeArchive(WebServiceClient client, (long, long)[] items,
            IArchiveManager.ArchiveType archiveType)
        {
            LogMethodCall(_log);
            LogVerbose(_log, $"Starting bulk download request with maximum concurrency of {_options.MaxConcurrentRequests}");
            var throttleSemaphore = new SemaphoreSlim(_options.MaxConcurrentRequests);
            var results = new List<string>();
            var tasks = new List<Task>();
            var tempDirectory = FileHelper.CreateTempDirectory();
            
            foreach(var item in items)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await throttleSemaphore.WaitAsync();
                    var result = await DownloadNodeVersion(client, item.Item1, item.Item2);
                    var tempFile = Path.Combine(tempDirectory.FullName, result.Filename);
                    LogVerbose(_log, $"Writing to temp location \"{tempFile}\"");
                    await using (var outStream = File.OpenWrite(tempFile))
                    {
                        await result.Stream.CopyToAsync(outStream);
                    }
                    results.Add(tempFile);
                    throttleSemaphore.Release();
                }) );    
            }

            Task.WaitAll(tasks.ToArray());
            
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Downloads a single <see cref="Node" /> version
        /// </summary>
        /// <param name="client">The <see cref="WebServiceClient" /> instance to use (should be pre-initialised)</param>
        /// <param name="id">The id of the node to retrieve</param>
        /// <param name="versionNum">The version number of the node to retrieve</param>
        /// <returns>A new instance of <see cref="IArchiveManager.DownloadContentResult" /></returns>
        private async Task<IArchiveManager.DownloadContentResult> DownloadNodeVersion(WebServiceClient client, long id, long versionNum)
        {
            var version = await client.GetItemVersion(id, versionNum);
            var attachment = await client.GetVersionContents(id, versionNum);
            return new IArchiveManager.DownloadContentResult
            {
                Stream = new MemoryStream(attachment.Contents),
                Filename = version.Filename,
                MimeType = version.MimeType
            };
        }

        

        

        

        /// <summary>
        ///     Resolves and instantiates a new instance of <see cref="WebServiceClient" /> based on the binding tag supplied
        /// </summary>
        /// <param name="tag">The binding tag used to look up the client settings</param>
        /// <returns>A new instance of <see cref="WebServiceClient" /></returns>
        /// <exception cref="ArchiveManagerException">Thrown in the event of a fault occurring</exception>
        private WebServiceClient BindWebServiceClient(string tag)
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