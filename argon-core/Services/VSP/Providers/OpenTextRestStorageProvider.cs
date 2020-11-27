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
    }
}