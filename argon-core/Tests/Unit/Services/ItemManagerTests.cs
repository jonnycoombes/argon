using System.Collections;
using System.Collections.Generic;
using JCS.Argon.Model.Commands;
using JCS.Argon.Services.Core;
using JCS.Neon.Glow.Helpers.Crypto;
using Xunit;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;
using static JCS.Neon.Glow.Types.Extensions.ByteArrayExtensions;
using static JCS.Neon.Glow.Helpers.Crypto.PassphraseHelpers;

namespace JCS.Argon.Tests.Unit.Services
{
    /// <summary>
    /// Test suite for <see cref="IItemManager"/>
    /// </summary>
    [Collection("Units")]
    [Trait("Test Type", "Unit")]
    [Trait("Target Service", "ItemManager")]
    public class ItemManagerTests : AbstractTestBase
    {
        /// <summary>
        /// Static logger
        /// </summary>
        private static ILogger _log = Log.ForContext<CollectionManagerTests>();

        [Fact(DisplayName = "Must be able to count the items in a collection")]
        [Trait("Test Type", "Unit")]
        [Trait("Target Service", "ItemManager")]
        public async void CountCollectionItems()
        {
            LogMethodCall(_log);
            var cmd = new CreateCollectionCommand("Test Collection", "TestFS", null);
            var collection = await _collectionManager.CreateCollectionAsync(cmd);
            var count = await _itemManager.CountItemsAsync(collection);
            Assert.Equal(0, count);
        }

        [Theory(DisplayName = "Must be able to create new items in a collection")]
        [Trait("Test Type", "Unit")]
        [Trait("Target Service", "ItemManager")]
        [Trait("Category", "ItemManager")]
        [InlineData(1024)]
        [InlineData(12024)]
        [InlineData(4096)]
        [InlineData(8192)]
        [InlineData(10)]
        [InlineData(65536)]
        [InlineData(100000)]
        [InlineData(150000)]
        [InlineData(644440)]
        [InlineData(33333333)]
        public async void CreateCollectionItems(int sizeInBytes)
        {
            LogMethodCall(_log);
            var cmd = new CreateCollectionCommand("Test Collection", "TestFS", null);
            var randomContents = PassphraseHelpers.GenerateRandomPassphrase(builder =>
            {
                builder.SetRequiredLength(sizeInBytes);
                builder.SetBase64Encoding(true);
            });
            var formFile = CreateTestFormFile("test", randomContents);
            var properties = new Dictionary<string, object>();
            var collection = await _collectionManager.CreateCollectionAsync(cmd);
            var item = await _itemManager.AddItemToCollectionAsync(collection, properties, formFile);
            Assert.NotNull(item);
        }
    }
}