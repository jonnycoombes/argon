using System;
using System.IO;
using System.Threading.Tasks;
using JCS.Argon.Model.Schema;
using JCS.Argon.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Version = JCS.Argon.Model.Schema.Version;

namespace JCS.Argon.Services.VSP.Providers
{
    public class OpenTextRestStorageProvider : BaseVirtualStorageProvider
    {
        public override string ProviderType => "openTextRestful";

        /// <summary>
        /// An internal instance of <see cref="OpenTextRestClient"/>
        /// </summary>
        private OpenTextRestClient _client= null!;

        /// <summary>
        /// The root storage path, relative to the Enterprise workspace
        /// </summary>
        private string? _rootCollectionPath; 
        
        /// <summary>
        /// Default constructor - since providers are loaded dynamically at run time,
        /// this is really just delegated to the base class
        /// </summary>
        /// <param name="log"></param>
        public OpenTextRestStorageProvider(ILogger log) : base(log)
        {
        }

        /// <summary>
        /// Called after an instance of this provider has been bound, so in here we create a new instance
        /// of an <see cref="OpenTextRestClient"/>
        /// </summary>
        public override void AfterBind()
        {
            _log.LogDebug($"{ProviderType}: AfterBind called - performing initialisation, creating new OTCS REST client");
            _rootCollectionPath = Binding!.Properties["rootCollectionPath"].ToString()!;
            this.AssertNotNull(_rootCollectionPath, "Root path hasn't been specified!");
            _log.LogDebug($"{ProviderType}: rootCollectionPath set to {_rootCollectionPath}");
            _client = CreateRestClient();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        private string GenerateCollectionPath(Collection collection)
        {
            return Path.Combine(_rootCollectionPath!, collection.Id.ToString()!);
        }

        public override async Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionAsync(Collection collection)
        {
            try
            {
                await _client.Authenticate();
                return new IVirtualStorageProvider.StorageOperationResult()
                {
                    Status = IVirtualStorageProvider.StorageOperationStatus.Failed
                };
            }
            catch (OpenTextRestClient.OpenTextRestClientException ex)
            {
                throw new IVirtualStorageProvider.VirtualStorageProviderException(ex.ResponseCodeHint,
                    $"An exception was caught from within the OTCS REST layer: {ex.GetBaseException().Message}", ex);
            }
        }

        /// <summary>
        /// Creates a new OpenText rest client
        /// </summary>
        /// <returns></returns>
        private OpenTextRestClient CreateRestClient()
        {
            this.AssertNotNull(_httpClient, "HTTP client has not been injected!");
            var client = new OpenTextRestClient(_log, _dbCache)
            {
                CachePartition = Binding!.Tag,
                EndpointAddress = Binding!.Properties["endpoint"].ToString(),
                UserName = Binding!.Properties["user"].ToString(),
                Password = Binding!.Properties["password"].ToString(),
                HttpClient = _httpClient
            };
            return client;
        }

        public override async Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemVersionAsync(Collection collection, Item item, Version version, IFormFile source)
        {
            throw new NotImplementedException();
        }

        public override async Task<IVirtualStorageProvider.StorageOperationResult> ReadCollectionItemVersionAsync(Collection collection, Item item, Version version)
        {
            throw new NotImplementedException();
        }
    }
}