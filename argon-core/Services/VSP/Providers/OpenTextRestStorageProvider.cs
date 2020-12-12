using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.Core;
using JCS.Argon.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Version = JCS.Argon.Model.Schema.Version;

namespace JCS.Argon.Services.VSP.Providers
{
    public class OpenTextRestStorageProvider : BaseVirtualStorageProvider
    {
        public override string ProviderType => "openTextRestful";

        private OpenTextRestClient _client= null!;
        
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
            _client = CreateRestClient();
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
            this.AssertNotNull(_serviceProvider, "Service provider has not been injected!");
            this.AssertNotNull(_httpClient, "HTTP client has not been injected!");
            var client = new OpenTextRestClient(_log, (IDbCache?) _serviceProvider!.GetService(typeof(IDbCache)))
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