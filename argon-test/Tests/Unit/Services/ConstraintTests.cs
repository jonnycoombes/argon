using System;
using System.Collections.Generic;
using System.Linq;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.Core;
using Xunit;
using static JCS.Neon.Glow.Helpers.Crypto.PassphraseHelpers;

namespace JCS.Argon.Tests.Tests.Unit.Services
{
    /// <summary>
    ///     Test suite for checking various different constraints set against collections.  This test suite
    ///     is based only around the Vsp provider so can be run independently of any supporting Otcs infrastructure
    /// </summary>
    [Collection("Units")]
    [Trait("Category", "Unit")]
    [Trait("Provider", "VSP")]
    public class ConstraintTests : AbstractTestBase
    {
        /// <summary>
        ///     Generates a constraint list which can be used in order to setup a collection
        /// </summary>
        /// <returns></returns>
        private static List<CreateOrUpdateConstraintCommand> GenerateConstraintList()
        {
            var constraints = new List<CreateOrUpdateConstraintCommand>
            {
                new CreateOrUpdateConstraintCommand
                {
                    Name = "Mandatory Title Constraint",
                    ConstraintType = ConstraintType.Mandatory,
                    SourceProperty = "Document Title"
                },
                new CreateOrUpdateConstraintCommand
                {
                    Name = "Mandatory Document Type Constraint",
                    ConstraintType = ConstraintType.Mandatory,
                    SourceProperty = "Document Type"
                },
                new CreateOrUpdateConstraintCommand
                {
                    Name = "String Value Type Constraint",
                    ConstraintType = ConstraintType.AllowableType,
                    SourceProperty = "MustBeString",
                    ValueType = ConstraintValidTypes.String
                },
                new CreateOrUpdateConstraintCommand
                {
                    Name = "Boolean Value Type Constraint",
                    ConstraintType = ConstraintType.AllowableType,
                    SourceProperty = "MustBeBool",
                    ValueType = ConstraintValidTypes.Boolean
                },
                new CreateOrUpdateConstraintCommand
                {
                    Name = "Numeric Value Type Constraint",
                    ConstraintType = ConstraintType.AllowableType,
                    SourceProperty = "MustBeNumber",
                    ValueType = ConstraintValidTypes.Number
                },
                new CreateOrUpdateConstraintCommand
                {
                    Name = "DateTime Value Type Constraint",
                    ConstraintType = ConstraintType.AllowableType,
                    SourceProperty = "MustBeDate",
                    ValueType = ConstraintValidTypes.DateTime
                },
                new CreateOrUpdateConstraintCommand
                {
                    Name = "String Allowable Value Constraint",
                    ConstraintType = ConstraintType.AllowableTypeAndValues,
                    SourceProperty = "AllowableValues",
                    ValueType = ConstraintValidTypes.String,
                    AllowableValues = new[] {"Value 1", "Value 2", "Value 3", "Value 4"}
                },
                new CreateOrUpdateConstraintCommand
                {
                    Name = "DateTime Value Type Constraint",
                    ConstraintType = ConstraintType.AllowableTypeAndValues,
                    SourceProperty = "AllowableNumericValues",
                    ValueType = ConstraintValidTypes.Number,
                    AllowableValues = new[] {"1", "2.5", "8"}
                }
            };

            return constraints;
        }

        /// <summary>
        ///     Check that mandatory constraints are enforced
        /// </summary>
        [Theory(DisplayName = "Mandatory constraints being violated should result in an exception being thrown")]
        [Trait("Category", "Unit")]
        [Trait("Provider", "VSP")]
        [InlineData("TestFS")]
        public async void ViolateMandatoryConstraint(string providerTag)
        {
            var randomContents = GenerateRandomPassphrase(builder =>
            {
                builder.SetRequiredLength(64);
                builder.SetBase64Encoding(true);
            });
            var createCommand = new CreateCollectionCommand("Constrained Collection", providerTag, "")
            {
                Constraints = GenerateConstraintList().Where(c => c.ConstraintType == ConstraintType.Mandatory).ToList()
            };
            var collection = await _collectionManager.CreateCollectionAsync(createCommand);
            var formFile = CreateTestFormFile("test", randomContents);
            var properties = new Dictionary<string, object>
            {
                {"Document Title", "Something"}
            };
            await Assert.ThrowsAsync<IItemManager.ItemManagerException>(async () =>
            {
                await _itemManager.AddItemToCollectionAsync(collection, properties, formFile);
            });
        }

        /// <summary>
        ///     Check that mandatory constraints are enforced
        /// </summary>
        [Theory(DisplayName = "Compliance with mandatory property constraints shouldn't throw any exceptions")]
        [Trait("Category", "Unit")]
        [Trait("Provider", "VSP")]
        [InlineData("TestFS")]
        public async void HonourMandatoryConstraint(string providerTag)
        {
            var randomContents = GenerateRandomPassphrase(builder =>
            {
                builder.SetRequiredLength(64);
                builder.SetBase64Encoding(true);
            });
            var createCommand = new CreateCollectionCommand("Constrained Collection", providerTag, "")
            {
                Constraints = GenerateConstraintList().Where(c => c.ConstraintType == ConstraintType.Mandatory).ToList()
            };
            var collection = await _collectionManager.CreateCollectionAsync(createCommand);
            var formFile = CreateTestFormFile("test", randomContents);
            var properties = new Dictionary<string, object>
            {
                {"Document Title", "Something"},
                {"Document Type", "Some Document Type"}
            };
            try
            {
                await _itemManager.AddItemToCollectionAsync(collection, properties, formFile);
            }
            catch (IItemManager.ItemManagerException ex)
            {
                Assert.True(false);
            }

            Assert.True(true);
        }

        /// <summary>
        ///     Check that mandatory constraints are enforced
        /// </summary>
        [Theory(DisplayName =
            "Type constraints should be enforced with no exception if the properties conform to the constraint specifications")]
        [Trait("Category", "Unit")]
        [Trait("Provider", "VSP")]
        [InlineData("TestFS")]
        public async void CheckTypeConstraints(string providerTag)
        {
            var randomContents = GenerateRandomPassphrase(builder =>
            {
                builder.SetRequiredLength(64);
                builder.SetBase64Encoding(true);
            });
            var createCommand = new CreateCollectionCommand("Constrained Collection", providerTag, "")
            {
                Constraints = GenerateConstraintList().Where(c => c.ConstraintType == ConstraintType.AllowableType).ToList()
            };
            var collection = await _collectionManager.CreateCollectionAsync(createCommand);
            var formFile = CreateTestFormFile("test", randomContents);
            var properties = new Dictionary<string, object>
            {
                {"MustBeBool", true},
                {"MustBeNumber", 123223},
                {"MustBeString", "Cheesy that's what"},
                {"MustBeDate", DateTime.Now}
            };
            try
            {
                await _itemManager.AddItemToCollectionAsync(collection, properties, formFile);
            }
            catch (IItemManager.ItemManagerException ex)
            {
                Assert.True(false);
            }

            Assert.True(true);
        }

        /// <summary>
        ///     Check that mandatory constraints are enforced
        /// </summary>
        [Theory(DisplayName = "Incorrect property types should result in an exception")]
        [Trait("Category", "Unit")]
        [Trait("Provider", "VSP")]
        [InlineData("TestFS")]
        public async void ViolateTypeConstraints(string providerTag)
        {
            var randomContents = GenerateRandomPassphrase(builder =>
            {
                builder.SetRequiredLength(64);
                builder.SetBase64Encoding(true);
            });
            var createCommand = new CreateCollectionCommand("Constrained Collection", providerTag, "")
            {
                Constraints = GenerateConstraintList().Where(c => c.ConstraintType == ConstraintType.AllowableType).ToList()
            };
            var collection = await _collectionManager.CreateCollectionAsync(createCommand);
            var formFile = CreateTestFormFile("test", randomContents);
            var properties = new Dictionary<string, object>
            {
                {"MustBeBool", "Invalid property type"},
                {"MustBeNumber", "Invalid property type"},
                {"MustBeString", 12345},
                {"MustBeDate", DateTime.Now}
            };
            await Assert.ThrowsAsync<IItemManager.ItemManagerException>(async () =>
            {
                await _itemManager.AddItemToCollectionAsync(collection, properties, formFile);
            });
        }

        [Theory(DisplayName = "Must be able to modify and add to the constraint group for a given collection (with existing constraints)")]
        [Trait("Category", "Unit")]
        [Trait("Provider", "VSP")]
        [InlineData("TestFS")]
        public async void AddAndUpdateConstraintsExistingConstraints(string providerTag)
        {
            var constraintList = GenerateConstraintList().ToList();
            var createCommand = new CreateCollectionCommand("Constrained Collection", providerTag, "")
            {
                Constraints = GenerateConstraintList().ToList()
            };
            var collection = await _collectionManager.CreateCollectionAsync(createCommand);
            var collectionId = collection.Id!.Value;
            Assert.NotNull(collection.ConstraintGroup);
            Assert.Equal(constraintList.Count, collection.ConstraintGroup.Constraints!.Count);
            var constraintUpdateCommands = new List<CreateOrUpdateConstraintCommand>
            {
                // change the source property of a constraint
                new CreateOrUpdateConstraintCommand
                {
                    Name = "Mandatory Title Constraint",
                    ConstraintType = ConstraintType.Mandatory,
                    SourceProperty = "DocumentTitle"
                },
                // change the overall type of a constraint
                new CreateOrUpdateConstraintCommand
                {
                    Name = "Document Type Constraint",
                    ConstraintType = ConstraintType.AllowableType,
                    SourceProperty = "Document Type",
                    ValueType = ConstraintValidTypes.String
                },
                new CreateOrUpdateConstraintCommand
                {
                    Name = "Sample Additional Constraint",
                    ConstraintType = ConstraintType.AllowableType,
                    SourceProperty = "AdditionalConstraint",
                    ValueType = ConstraintValidTypes.Number
                }
            };
            collection = await _collectionManager.UpdateCollectionConstraints(collectionId, constraintUpdateCommands);
            Constraint updatedConstraint;
            Assert.NotNull(collection.ConstraintGroup);
            updatedConstraint = collection.ConstraintGroup.Constraints!.First(c => c.Name == "Mandatory Title Constraint");
            Assert.Equal("DocumentTitle", updatedConstraint.SourceProperty);
            updatedConstraint = collection.ConstraintGroup.Constraints!.First(c => c.Name == "Document Type Constraint");
            Assert.Equal(ConstraintValidTypes.String, updatedConstraint.ValueType);
            Assert.Contains(collection.ConstraintGroup.Constraints, c => c.Name == "Sample Additional Constraint");
        }

        [Theory(DisplayName =
            "Must be able to modify and add to the constraint group for a given collection (with no existing constraints)")]
        [Trait("Category", "Unit")]
        [Trait("Provider", "VSP")]
        [InlineData("TestFS")]
        public async void AddAndUpdateConstraintsNoExistingConstraints(string providerTag)
        {
            var createCommand = new CreateCollectionCommand("Constrained Collection", providerTag, "");
            var collection = await _collectionManager.CreateCollectionAsync(createCommand);
            var collectionId = collection.Id!.Value;
            var constraintUpdateCommands = new List<CreateOrUpdateConstraintCommand>
            {
                // change the source property of a constraint
                new CreateOrUpdateConstraintCommand
                {
                    Name = "Mandatory Title Constraint",
                    ConstraintType = ConstraintType.Mandatory,
                    SourceProperty = "DocumentTitle"
                },
                // change the overall type of a constraint
                new CreateOrUpdateConstraintCommand
                {
                    Name = "Document Type Constraint",
                    ConstraintType = ConstraintType.AllowableType,
                    SourceProperty = "Document Type",
                    ValueType = ConstraintValidTypes.String
                },
                new CreateOrUpdateConstraintCommand
                {
                    Name = "Sample Additional Constraint",
                    ConstraintType = ConstraintType.AllowableType,
                    SourceProperty = "AdditionalConstraint",
                    ValueType = ConstraintValidTypes.Number
                }
            };
            collection = await _collectionManager.UpdateCollectionConstraints(collectionId, constraintUpdateCommands);
            Constraint updatedConstraint;
            Assert.NotNull(collection.ConstraintGroup);
            updatedConstraint = collection.ConstraintGroup.Constraints!.First(c => c.Name == "Mandatory Title Constraint");
            Assert.Equal("DocumentTitle", updatedConstraint.SourceProperty);
            updatedConstraint = collection.ConstraintGroup.Constraints!.First(c => c.Name == "Document Type Constraint");
            Assert.Equal(ConstraintValidTypes.String, updatedConstraint.ValueType);
            Assert.Contains(collection.ConstraintGroup.Constraints, c => c.Name == "Sample Additional Constraint");
        }
    }
}