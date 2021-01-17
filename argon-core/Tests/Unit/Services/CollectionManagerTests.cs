using System;
using JCS.Argon.Model.Commands;
using JCS.Argon.Services.Core;
using Xunit;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;
using static JCS.Neon.Glow.Helpers.General.ParseHelpers;

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

        [Fact(DisplayName = "Must not be able to retrieve a collection from an empty database")]
        [Trait("Test Type", "Unit")]
        [Trait("Target Service", "CollectionManager")]
        public void AttemptRetrievalFromEmptyDatabase()
        {
            LogMethodCall(_log);
            Assert.ThrowsAsync<ICollectionManager.CollectionManagerException>(async () =>
            {
                await _collectionManager.GetCollectionAsync(Guid.NewGuid());
            });
        }

        [Theory(DisplayName = "Attempting to load collections with random identifiers should fail")]
        [Trait("Test Type", "Unit")]
        [Trait("Target Service", "CollectionManager")]
        [InlineData("0ddd1bb5-3dce-4ec1-9d3e-a6115783a638")]
        [InlineData("7220c4e1-c609-46ea-9bef-efc8b826e379")]
        [InlineData("f7e87df8-18ba-436b-9891-ae13714cbac4")]
        [InlineData("1583490d-2531-48c9-b62b-691a56d67ea4")]
        [InlineData("36b0b63a-925d-4def-95ca-8dae0e98d042")]
        public  void RetrieveBogusCollection(string collectionId)
        {
            LogMethodCall(_log);
            Assert.ThrowsAsync<ICollectionManager.CollectionManagerException>(async () =>
            {
                await _collectionManager.GetCollectionAsync(Guid.Parse(collectionId));
            });
        }

        [Theory(DisplayName = "Must be able to create new collections")]
        [Trait("Test Type", "Unit")]
        [Trait("Target Service", "CollectionManager")]
        [InlineData("Test Collection 1")]
        [InlineData("Test Collection 2")]
        [InlineData("Test Collection 3")]
        [InlineData("Test Collection 4")]
        [InlineData("Test Collection 5")]
        public async void CreateCollection(string name)
        {
            LogMethodCall(_log);
            var cmd = new CreateCollectionCommand()
            {
                Name = name,
                Description = "Test description",
                ProviderTag = "TestFS"
            };
            var collection = await _collectionManager.CreateCollectionAsync(cmd);
            Assert.IsType<Guid>(collection.Id);
            Assert.Equal(0, collection.Length);
        }

        [Fact(DisplayName = "Must not be able to create collections with a duplicate name")]
        [Trait("Test Type", "Unit")]
        [Trait("Target Service", "CollectionManager")]
        public async void AttemptCreationWithDuplicateName()
        {
            var name = "Test Duplicate Collection";
            var cmd = new CreateCollectionCommand()
            {
                Name = name,
                Description = "Duplicate collection",
                ProviderTag = "TestFS"
            };
            var collection = await _collectionManager.CreateCollectionAsync(cmd);
            Assert.IsType<Guid>(collection.Id);
            Assert.Equal(0, collection.Length);
            await Assert.ThrowsAsync<ICollectionManager.CollectionManagerException>(async () =>
            {
                var duplicate = await _collectionManager.CreateCollectionAsync(cmd);
            });
        }

        [Fact(DisplayName = "Must be able to count collections")]
        [Trait("Test Type", "Unit")]
        [Trait("Target Service", "CollectionManager")]
        public async void CountCollections()
        {
            var cmds = new CreateCollectionCommand[]
            {
                new CreateCollectionCommand("Collection 1", "TestFS", ""),
                new CreateCollectionCommand("Collection 2", "TestFS", ""),
                new CreateCollectionCommand("Collection 3", "TestFS", ""),
                new CreateCollectionCommand("Collection 4", "TestFS", ""),
                new CreateCollectionCommand("Collection 5", "TestFS", ""),
                new CreateCollectionCommand("Collection 6", "TestFS", ""),
                new CreateCollectionCommand("Collection 7", "TestFS", ""),
                new CreateCollectionCommand("Collection 8", "TestFS", ""),
                new CreateCollectionCommand("Collection 9", "TestFS", ""),
                new CreateCollectionCommand("Collection 10", "TestFS", ""),
            };
            foreach (var cmd in cmds)
            {
                var collection = await _collectionManager.CreateCollectionAsync(cmd);
            }

            var collectionCount = await _collectionManager.CountCollectionsAsync();
            Assert.Equal(10, collectionCount);
        }

        [Fact(DisplayName = "Must be able to list collections")]
        [Trait("Test Type", "Unit")]
        [Trait("Target Service", "CollectionManager")]
        public async void ListCollections()
        {
            var cmds = new CreateCollectionCommand[]
            {
                new CreateCollectionCommand("Collection 1", "TestFS", ""),
                new CreateCollectionCommand("Collection 2", "TestFS", ""),
                new CreateCollectionCommand("Collection 3", "TestFS", ""),
                new CreateCollectionCommand("Collection 4", "TestFS", ""),
                new CreateCollectionCommand("Collection 5", "TestFS", "")
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

        [Fact(DisplayName = "Must be able to retrieve a single collection by id")]
        [Trait("Test Type", "Unit")]
        [Trait("Target Service", "CollectionManager")]
        public async void RetrieveCollection()
        {
            var cmd = new CreateCollectionCommand("Test Collection", "TestFS", null);
            var collection = await _collectionManager.CreateCollectionAsync(cmd);
            Assert.NotNull(collection);
            var id = collection.Id.Value;
            var retrieved = await _collectionManager.GetCollectionAsync(id);
            Assert.NotNull(retrieved);
            Assert.Equal("Test Collection", retrieved.Name);
        }
    }
}