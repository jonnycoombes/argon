using JCS.Argon.Services.Core;
using Xunit;

namespace JCS.Argon.Tests.Unit.Services
{
    /// <summary>
    ///     Test suite for <see cref="IItemManager" /> against an external OTCS provider.  The configuration and
    ///     connection properties associated with OTCS connection are taken from the current appsettings.json file(s)
    /// </summary>
    [Collection("Units")]
    [Trait("Category", "Unit")]
    [Trait("Provider", "OTCS")]
    public class OTCSItemTests : AbstractTestBase
    {
    }
}