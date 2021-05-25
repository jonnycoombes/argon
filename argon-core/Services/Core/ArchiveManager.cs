using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.Soap.Opentext;
using JCS.Argon.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
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
                    return await DownloadNodeArchive(client, path, items, archiveType);
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
        /// <param name="path">The original path in the request</param>
        /// <param name="items">A list of pairs, consisting of a node identifier and a version number</param>
        /// <param name="archiveType">The type of archive file to produce</param>
        /// <returns>
        ///     A <see cref="IArchiveManager.DownloadContentResult" /> which contains information and payload associated with
        ///     the resultant archive file
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<IArchiveManager.DownloadContentResult> DownloadNodeArchive(WebServiceClient client, string path,
            (long, long)[] items,
            IArchiveManager.ArchiveType archiveType)
        {
            LogMethodCall(_log);
            LogVerbose(_log, $"Starting bulk download request with maximum concurrency of {_options.MaxConcurrentRequests}");
            var throttleSemaphore = new SemaphoreSlim(_options.MaxConcurrentRequests);
            var files = new List<string>();
            var tasks = new List<Task>();
            var tempDirectory = FileHelper.CreateTempDirectory();

            try
            {
                foreach (var item in items)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await throttleSemaphore.WaitAsync();
                        var version = await DownloadNodeVersion(client, item.Item1, item.Item2);
                        var tempFile = Path.Combine(tempDirectory.FullName, version.Filename);
                        LogVerbose(_log, $"Writing to temp location \"{tempFile}\"");
                        await using (var outStream = File.OpenWrite(tempFile))
                        {
                            await version.Stream.CopyToAsync(outStream);
                        }

                        files.Add(tempFile);
                        throttleSemaphore.Release();
                    }));
                }

                LogVerbose(_log, "Awaiting completion parallel tasks");
                Task.WaitAll(tasks.ToArray());
                LogVerbose(_log, "All parallel tasks completed");
            }
            catch (Exception ex)
            {
                LogExceptionWarning(_log, ex);
                throw new ArchiveManagerException(StatusCodes.Status500InternalServerError,
                    $"A downstream exception was caught whilst attempting bulk download of items: \"{ex.Message}\"");
            }

            try
            {
                IArchiveManager.DownloadContentResult result;
                switch (archiveType)
                {
                    case IArchiveManager.ArchiveType.PdfArchive:
                        LogVerbose(_log, $"Creating zip new zip archive based on \"{tempDirectory.FullName}\"");
                        result = await CreatePdfArchive(path, tempDirectory.FullName, files);
                        break;
                    default:
                        result = await CreateZipArchive(path, tempDirectory.FullName);
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                LogExceptionWarning(_log, ex);
                throw new ArchiveManagerException(StatusCodes.Status500InternalServerError,
                    $"An exception was caught whilst attempting create archive: \"{ex.Message}\"");
            }
        }

        /// <summary>
        ///     Creates a single zip archive containing the contents of the supplied directory, and then returns the contents as an instance
        ///     of <see cref="IArchiveManager.DownloadContentResult" />
        /// </summary>
        /// <param name="path">The original path from the originating request</param>
        /// <param name="sourceDirectory">The entire contents of the supplied directory will be zipped and returned in the response</param>
        /// <returns>An instance of <see cref="IArchiveManager.DownloadContentResult" /></returns>
        private static async Task<IArchiveManager.DownloadContentResult> CreateZipArchive(string path, string sourceDirectory)
        {
            LogMethodCall(_log);
            var zipDirectory = FileHelper.CreateTempDirectory();
            var zipLocation = Path.Combine(zipDirectory.FullName, Path.GetRandomFileName());
            LogVerbose(_log, $"Creating temporary zip archive in the following location: \"{zipLocation}\"");

            ZipFile.CreateFromDirectory(sourceDirectory, zipLocation);
            MemoryStream outStream;
            await using (var inStream = File.OpenRead(zipLocation))
            {
                outStream = new MemoryStream();
                await inStream.CopyToAsync(outStream);
                await inStream.FlushAsync();
                await outStream.FlushAsync();
            }

            try
            {
                File.Delete(zipLocation);
                LogVerbose(_log, $"Removing \"{zipDirectory.FullName}\"");
                Directory.Delete(zipDirectory.FullName);
            }
            catch (Exception)
            {
                LogWarning(_log, "Caught an unexpected exception whilst attempting to clean up temp locations");
            }

            outStream.Seek(0, SeekOrigin.Begin);
            return new IArchiveManager.DownloadContentResult
            {
                Stream = outStream,
                MimeType = "application/zip",
                Filename = $"{path.Replace('/', '_')}.zip"
            };
        }

        /// <summary>
        ///     Creates a single PDF archive formed from the concatenation/attachment of all the files specified, and then returns the result
        ///     as an instance of <see cref="IArchiveManager.DownloadContentResult" />
        /// </summary>
        /// <param name="path">The original path from the originating request</param>
        /// <param name="sourceDirectory">The source or working directory</param>
        /// <param name="files">A list of filenames (fully qualified) to be used in order to form the archive</param>
        /// <returns>An instance of <see cref="IArchiveManager.DownloadContentResult" /></returns>
        private static async Task<IArchiveManager.DownloadContentResult> CreatePdfArchive(string path, string sourceDirectory,
            IEnumerable<string>
                files)
        {
            LogMethodCall(_log);
            var filename = $"{path.Replace('/', '_')}.pdf";
            var fileDirectory = FileHelper.CreateTempDirectory();
            var filepath = Path.Combine(fileDirectory.FullName, filename);

            if (PdfHelper.CombineFilesToPdf(filepath, files))
            {
                await using var inStream = new FileStream(filepath, FileMode.Open);
                var outStream = new MemoryStream();
                await inStream.CopyToAsync(outStream);
                outStream.Seek(0, SeekOrigin.Begin);

                try
                {
                    LogVerbose(_log, "Performing post PDF archive clean up");
                    File.Delete(filepath);
                    LogVerbose(_log, $"Removing \"{fileDirectory.FullName}\"");
                    Directory.Delete(fileDirectory.FullName);
                    LogVerbose(_log, $"Removing \"{sourceDirectory}\"");
                    Directory.Delete(sourceDirectory);
                }
                catch (Exception)
                {
                    LogWarning(_log, "Caught an unexpected exception whilst attempting to clean up temp locations");
                }

                return new IArchiveManager.DownloadContentResult
                {
                    Stream = outStream,
                    Filename = filename,
                    MimeType = "application/pdf"
                };
            }

            throw new ArchiveManagerException(StatusCodes.Status500InternalServerError,
                "Pdf archiving operation failed - please check logs for further information");
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