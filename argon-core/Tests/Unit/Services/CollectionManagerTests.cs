using Xunit;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

namespace JCS.Argon.Tests.Unit.Services
{
    [Collection("Units")]
    [Trait("Test Type", "Unit")]
    [Trait("Target Service", "CollectionManager")]
    public class CollectionManagerTests : AbstractTestBase
    {
        /// <summary>
        /// Static logger for this test suite
        /// </summary>
        private static ILogger _log = Log.ForContext<CollectionManagerTests>();

        [Fact(DisplayName = "An empty database must contain no collections")]
        [Trait("Test Type", "Unit")]
        [Trait("Target Service", "CollectionManager")]
        public async void CountEmptyDatabase()
        {
            LogMethodCall(_log);
            var count = await _collectionManager.CountCollectionsAsync();
            Assert.True(count == 0);
        }
    }
}