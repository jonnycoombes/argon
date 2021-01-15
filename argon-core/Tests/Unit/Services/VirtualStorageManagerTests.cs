using JCS.Argon.Services.VSP.Providers;
using Xunit;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

namespace JCS.Argon.Tests.Unit.Services
{
    /// <summary>
    /// Test suite for <see cref="IVirtualStorageManager"/> instances
    /// </summary>
    [Collection("Units")]
    [Trait("Test Type", "Unit")]
    [Trait("Target Service", "VirtualStorageManager")]
    public class VirtualStorageManagerTests : AbstractTestBase
    {
        /// <summary>
        /// Static test logger
        /// </summary>
        private static ILogger _log = Log.ForContext<VirtualStorageManagerTests>();

        [Fact(DisplayName = "Must be at least a single binding retrievable from the current storage provider")]
        [Trait("Test Type", "Unit")]
        [Trait("Target Service", "VirtualStorageManager")]
        public void VerifyBindingCounts()
        {
            LogMethodCall(_log);
            var bindings= _virtualStorageManager.GetBindings();
            var bindingCount = bindings.Count;
            LogVerbose(_log, $"Current binding count is \"{bindingCount}\"");
            Assert.True(bindingCount > 1);
        }

        [Theory(DisplayName = "Must be able to retrieve providers by their respective tags")]
        [Trait("Test Type", "Unit")]
        [Trait("Target Service", "VirtualStorageManager")]
        [InlineData("Test FS")]    
        [InlineData("TestOTCSCollection")]    
        public void GetProviderByName(string name)
        {
            LogMethodCall(_log);
            var provider= _virtualStorageManager.GetProviderByTag(name);
            Assert.NotNull(provider);
        }
        
    }
}