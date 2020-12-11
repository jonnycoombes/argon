using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Version = JCS.Argon.Model.Schema.Version;

namespace JCS.Argon.Services.VSP.Providers
{
    public class OpenTextRestStorageProvider : BaseVirtualStorageProvider
    {

        public OpenTextRestStorageProvider(ILogger log) : base(log)
        {
        }

        public override string ProviderType => "openTextRestful";

        public override void AfterBind()
        {
            _log.LogDebug($"{ProviderType}: AfterBind called - performing initialisation, creating new OTCS REST client");
            
        }

        public override async Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionAsync(Collection collection)
        {
            try
            {
                var client = new OpenTextRestClient(_log)
                {
                    EndpointAddress = Binding!.Properties["endpoint"].ToString(),
                    UserName = Binding!.Properties["user"].ToString(),
                    Password = Binding!.Properties["password"].ToString(),
                    HttpClient = _httpClient
                };
                await client.Authenticate();
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