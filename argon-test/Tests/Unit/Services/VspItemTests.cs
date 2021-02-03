#region

using System.Collections.Generic;
using System.Linq;
using JCS.Argon.Model.Commands;
using JCS.Argon.Services.Core;
using Serilog;
using Xunit;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;
using static JCS.Neon.Glow.Helpers.Crypto.PassphraseHelpers;

#endregion

namespace JCS.Argon.Tests.Tests.Unit.Services
{
    /// <summary>
    ///     Test suite for <see cref="IItemManager" />
    /// </summary>
    [Collection("Units")]
    [Trait("Category", "Unit")]
    [Trait("Provider", "VSP")]
    public class VspItemTests : AbstractTestBase
    {
        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<VspCollectionTests>();

        [Theory(DisplayName = "Must be able to count the items in a collection")]
        [Trait("Category", "Unit")]
        [Trait("Provider", "VSP")]
        [InlineData("TestFS")]
        public async void CountCollectionItems(string providerTag)
        {
            LogMethodCall(_log);
            var cmd = new CreateCollectionCommand("Test Collection", providerTag, null);
            var collection = await _collectionManager.CreateCollectionAsync(cmd);
            var count = await _itemManager.CountItemsAsync(collection);
            Assert.Equal(0, count);
        }

        [Theory(DisplayName = "Must be able to create new items in a collection")]
        [Trait("Category", "Unit")]
        [Trait("Provider", "VSP")]
        [InlineData(1024, "TestFS")]
        [InlineData(12024, "TestFS")]
        [InlineData(4096, "TestFS")]
        [InlineData(8192, "TestFS")]
        [InlineData(10, "TestFS")]
        [InlineData(65536, "TestFS")]
        [InlineData(100000, "TestFS")]
        [InlineData(150000, "TestFS")]
        [InlineData(644440, "TestFS")]
        [InlineData(33333333, "TestFS")]
        public async void CreateCollectionItems(int sizeInBytes, string providerTag)
        {
            LogMethodCall(_log);
            var cmd = new CreateCollectionCommand("Test Collection", providerTag, null);
            var randomContents = GenerateRandomPassphrase(builder =>
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


        [Theory(DisplayName = "Must be able to add new items and subsequently retrieve them")]
        [Trait("Category", "Unit")]
        [Trait("Provider", "VSP")]
        [InlineData(1024, "TestFS")]
        [InlineData(12024, "TestFS")]
        [InlineData(4096, "TestFS")]
        [InlineData(8192, "TestFS")]
        [InlineData(10, "TestFS")]
        [InlineData(65536, "TestFS")]
        [InlineData(100000, "TestFS")]
        [InlineData(150000, "TestFS")]
        [InlineData(644440, "TestFS")]
        [InlineData(33333333, "TestFS")]
        public async void AddAndRetrieveItems(int sizeInBytes, string providerTag)
        {
            LogMethodCall(_log);
            var cmd = new CreateCollectionCommand("Test Collection", providerTag, null);
            var randomContents = GenerateRandomPassphrase(builder =>
            {
                builder.SetRequiredLength(sizeInBytes);
                builder.SetBase64Encoding(true);
            });
            var formFile = CreateTestFormFile("test", randomContents);
            var properties = new Dictionary<string, object>();
            var collection = await _collectionManager.CreateCollectionAsync(cmd);
            var item = await _itemManager.AddItemToCollectionAsync(collection, properties, formFile);
            Assert.NotNull(item);
            var versionId = item.Versions.First().Id.Value;
            var itemVersion = await _itemManager.GetItemVersionAsync(collection, item, versionId);
            Assert.NotNull(itemVersion);
            var stream = await _itemManager.GetStreamForVersionAsync(collection, item, itemVersion);
            Assert.NotNull(stream);
        }
    }
}