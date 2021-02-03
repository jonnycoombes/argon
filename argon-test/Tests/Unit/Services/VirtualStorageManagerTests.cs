#region

using Serilog;
using Xunit;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

namespace JCS.Argon.Tests.Tests.Unit.Services
{
    /// <summary>
    ///     Test suite for <see cref="JCS.Argon.Services.VSP.IVirtualStorageManager" /> instances
    /// </summary>
    [Collection("Units")]
    [Trait("Category", "Unit")]
    public class VirtualStorageManagerTests : AbstractTestBase
    {
        /// <summary>
        ///     Static test logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<VirtualStorageManagerTests>();

        [Fact(DisplayName = "Must be at least a single binding retrievable from the current storage provider")]
        [Trait("Category", "Unit")]
        public void VerifyBindingCounts()
        {
            LogMethodCall(_log);
            var bindings = _virtualStorageManager.GetBindings();
            var bindingCount = bindings.Count;
            LogVerbose(_log, $"Current binding count is \"{bindingCount}\"");
            Assert.True(bindingCount > 1);
        }

        [Theory(DisplayName = "Must be able to retrieve providers by their respective tags")]
        [Trait("Category", "Unit")]
        [InlineData("TestFS")]
        [InlineData("TestOTCSCollection")]
        public void GetProviderByName(string name)
        {
            LogMethodCall(_log);
            var provider = _virtualStorageManager.GetProviderByTag(name);
            Assert.NotNull(provider);
        }
    }
}