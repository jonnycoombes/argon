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
                    AllowableValues = new[] {"Value 1", "Value 2", "Value 3", "Value 4"}
                },
                new CreateOrUpdateConstraintCommand
                {
                    Name = "DateTime Value Type Constraint",
                    ConstraintType = ConstraintType.AllowableTypeAndValues,
                    SourceProperty = "AllowableNumericValues",
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
            var creationCmd = new CreateCollectionCommand("Constrained Collection", providerTag, "")
            {
                Constraints = GenerateConstraintList().Where(c => c.ConstraintType == ConstraintType.Mandatory).ToList()
            };
            var collection = await _collectionManager.CreateCollectionAsync(creationCmd);
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
            var creationCmd = new CreateCollectionCommand("Constrained Collection", providerTag, "")
            {
                Constraints = GenerateConstraintList().Where(c => c.ConstraintType == ConstraintType.Mandatory).ToList()
            };
            var collection = await _collectionManager.CreateCollectionAsync(creationCmd);
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
            var creationCmd = new CreateCollectionCommand("Constrained Collection", providerTag, "")
            {
                Constraints = GenerateConstraintList().Where(c => c.ConstraintType == ConstraintType.AllowableType).ToList()
            };
            var collection = await _collectionManager.CreateCollectionAsync(creationCmd);
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
            var creationCmd = new CreateCollectionCommand("Constrained Collection", providerTag, "")
            {
                Constraints = GenerateConstraintList().Where(c => c.ConstraintType == ConstraintType.AllowableType).ToList()
            };
            var collection = await _collectionManager.CreateCollectionAsync(creationCmd);
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
    }
}