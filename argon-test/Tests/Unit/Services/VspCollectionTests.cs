#region

using System;
using JCS.Argon.Model.Commands;
using JCS.Argon.Services.Core;
using JCS.Argon.Tests.Unit;
using Serilog;
using Xunit;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

namespace JCS.Argon.Tests.Tests.Unit.Services
{
    /// <summary>
    ///     Test suite for <see cref="ICollectionManager" /> against a self-contained VSP provider
    /// </summary>
    [Collection("Units")]
    [Trait("Category", "Unit")]
    [Trait("Provider", "VSP")]
    public class VspCollectionTests : AbstractTestBase
    {
        /// <summary>
        ///     Static logger for this test suite
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<VspCollectionTests>();

        [Fact(DisplayName = "An empty database must contain no collections")]
        [Trait("Category", "Unit")]
        public async void CountEmptyDatabase()
        {
            LogMethodCall(_log);
            var count = await _collectionManager.CountCollectionsAsync();
            Assert.True(count == 0);
        }

        [Fact(DisplayName = "Must not be able to retrieve a collection from an empty database")]
        [Trait("Category", "Unit")]
        public void AttemptRetrievalFromEmptyDatabase()
        {
            LogMethodCall(_log);
            Assert.ThrowsAsync<ICollectionManager.CollectionManagerException>(async () =>
            {
                await _collectionManager.GetCollectionAsync(Guid.NewGuid());
            });
        }

        [Theory(DisplayName = "Attempting to load collections with random identifiers should fail")]
        [Trait("Category", "Unit")]
        [InlineData("0ddd1bb5-3dce-4ec1-9d3e-a6115783a638")]
        [InlineData("7220c4e1-c609-46ea-9bef-efc8b826e379")]
        [InlineData("f7e87df8-18ba-436b-9891-ae13714cbac4")]
        [InlineData("1583490d-2531-48c9-b62b-691a56d67ea4")]
        [InlineData("36b0b63a-925d-4def-95ca-8dae0e98d042")]
        public void RetrieveBogusCollection(string collectionId)
        {
            LogMethodCall(_log);
            Assert.ThrowsAsync<ICollectionManager.CollectionManagerException>(async () =>
            {
                await _collectionManager.GetCollectionAsync(Guid.Parse(collectionId));
            });
        }

        [Theory(DisplayName = "Must be able to create new collections")]
        [Trait("Category", "Unit")]
        [InlineData("Test Collection 1", "TestFS")]
        [InlineData("Test Collection 2", "TestFS")]
        [InlineData("Test Collection 3", "TestFS")]
        [InlineData("Test Collection 4", "TestFS")]
        [InlineData("Test Collection 5", "TestFS")]
        [InlineData("Test Collection 1", "TestOTCSCollection")]
        [InlineData("Test Collection 2", "TestOTCSCollection")]
        [InlineData("Test Collection 3", "TestOTCSCollection")]
        [InlineData("Test Collection 4", "TestOTCSCollection")]
        [InlineData("Test Collection 5", "TestOTCSCollection")]
        public async void CreateCollection(string name, string providerTag)
        {
            LogMethodCall(_log);
            var cmd = new CreateCollectionCommand
            {
                Name = name,
                Description = "Test description",
                ProviderTag = providerTag
            };
            var collection = await _collectionManager.CreateCollectionAsync(cmd);
            Assert.IsType<Guid>(collection.Id);
            Assert.Equal(0, collection.Length);
        }

        [Theory(DisplayName = "Must not be able to create collections with a duplicate name")]
        [Trait("Category", "Unit")]
        [InlineData("TestFS")]
        [InlineData("TestOTCSCollection")]
        public async void AttemptCreationWithDuplicateName(string providerTag)
        {
            var name = "Test Duplicate Collection";
            var cmd = new CreateCollectionCommand
            {
                Name = name,
                Description = "Duplicate collection",
                ProviderTag = providerTag
            };
            var collection = await _collectionManager.CreateCollectionAsync(cmd);
            Assert.IsType<Guid>(collection.Id);
            Assert.Equal(0, collection.Length);
            await Assert.ThrowsAsync<ICollectionManager.CollectionManagerException>(async () =>
            {
                var duplicate = await _collectionManager.CreateCollectionAsync(cmd);
            });
        }

        [Theory(DisplayName = "Must be able to count collections")]
        [Trait("Category", "Unit")]
        [InlineData("TestFS")]
        [InlineData("TestOTCSCollection")]
        public async void CountCollections(string providerTag)
        {
            var cmds = new[]
            {
                new CreateCollectionCommand("Collection 1", providerTag, ""),
                new CreateCollectionCommand("Collection 2", providerTag, ""),
                new CreateCollectionCommand("Collection 3", providerTag, ""),
                new CreateCollectionCommand("Collection 4", providerTag, ""),
                new CreateCollectionCommand("Collection 5", providerTag, ""),
                new CreateCollectionCommand("Collection 6", providerTag, ""),
                new CreateCollectionCommand("Collection 7", providerTag, ""),
                new CreateCollectionCommand("Collection 8", providerTag, ""),
                new CreateCollectionCommand("Collection 9", providerTag, ""),
                new CreateCollectionCommand("Collection 10", providerTag, "")
            };
            foreach (var cmd in cmds)
            {
                var collection = await _collectionManager.CreateCollectionAsync(cmd);
            }

            var collectionCount = await _collectionManager.CountCollectionsAsync();
            Assert.Equal(10, collectionCount);
        }

        [Theory(DisplayName = "Must be able to list collections")]
        [Trait("Category", "Unit")]
        [InlineData("TestFS")]
        [InlineData("TestOTCSCollection")]
        public async void ListCollections(string providerTag)
        {
            var cmds = new[]
            {
                new CreateCollectionCommand("Collection 1", providerTag, ""),
                new CreateCollectionCommand("Collection 2", providerTag, ""),
                new CreateCollectionCommand("Collection 3", providerTag, ""),
                new CreateCollectionCommand("Collection 4", providerTag, ""),
                new CreateCollectionCommand("Collection 5", providerTag, "")
            };
            foreach (var cmd in cmds)
            {
                var collection = await _collectionManager.CreateCollectionAsync(cmd);
            }

            var collections = await _collectionManager.ListCollectionsAsync();
            Assert.Equal(5, collections.Count);
            Assert.Collection(collections,
                collection => Assert.Equal("Collection 1", collection.Name),
                collection => Assert.Equal("Collection 2", collection.Name),
                collection => Assert.Equal("Collection 3", collection.Name),
                collection => Assert.Equal("Collection 4", collection.Name),
                collection => Assert.Equal("Collection 5", collection.Name)
            );
        }

        [Theory(DisplayName = "Must be able to retrieve a single collection by id")]
        [Trait("Category", "Unit")]
        [InlineData("TestFS")]
        [InlineData("TestOTCSCollection")]
        public async void RetrieveCollection(string providerTag)
        {
            var cmd = new CreateCollectionCommand("Test Collection", providerTag, null);
            var collection = await _collectionManager.CreateCollectionAsync(cmd);
            Assert.NotNull(collection);
            var id = collection.Id.Value;
            var retrieved = await _collectionManager.GetCollectionAsync(id);
            Assert.NotNull(retrieved);
            Assert.Equal("Test Collection", retrieved.Name);
        }
    }
}