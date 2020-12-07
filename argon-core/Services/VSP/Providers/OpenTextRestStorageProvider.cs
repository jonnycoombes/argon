using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Services.VSP.Providers
{
    public class OpenTextRestStorageProvider : BaseVirtualStorageProvider
    {
        public override string ProviderType => "openTextRestful";
        
        
        public OpenTextRestStorageProvider(ILogger log) : base(log)
        {
        }
        
        public override void AfterBind()
        {
            _log.LogDebug($"{ProviderType}: AfterBind called - performing initialisation ");
        }

        public override Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionAsync(Collection collection)
        {
            throw new System.NotImplementedException();
        }

        public override Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemAsync(Collection collection, Item item,
            IFormFile source)
        {
            throw new System.NotImplementedException();
        }

        public override Task<IVirtualStorageProvider.StorageOperationResult> CreateCollectionItemVersionAsync(Collection collection, Item item, Version version, FileStream source)
        {
            throw new System.NotImplementedException();
        }
    }
}