using System.Collections.Generic;
using JCS.Argon.Model.Commands;
using Xunit;

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
        ///     Generates a sample property bag
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, object> GeneratePropertyBag()
        {
            return new Dictionary<string, object>();
        }

        /// <summary>
        ///     Generates a constraint list which can be used in order to setup a collection
        /// </summary>
        /// <returns></returns>
        private static List<CreateOrUpdateConstraintCommand> GenerateConstraintList()
        {
            return new List<CreateOrUpdateConstraintCommand>();
        }
    }
}