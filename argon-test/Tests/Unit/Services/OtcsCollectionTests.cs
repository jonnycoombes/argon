#region

using JCS.Argon.Services.Core;
using JCS.Argon.Tests.Unit;
using Xunit;

#endregion

namespace JCS.Argon.Tests.Tests.Unit.Services
{
    /// <summary>
    ///     Test suite for <see cref="ICollectionManager" /> against an external OTCS provider.  The configuration and
    ///     connection properties associated with OTCS connection are taken from the current appsettings.json file(s)
    /// </summary>
    [Collection("Units")]
    [Trait("Category", "Unit")]
    [Trait("Provider", "OTCS")]
    public class OtcsCollectionTests : AbstractTestBase
    {
    }
}