using System;

namespace JCS.Argon.Tests.Unit
{
    /// <summary>
    /// Base class for unit and service tests.  Basically contains methods for mocking services as required,
    /// along with establishing DB contexts etc...this base class assumes and will use a separate series of
    /// application settings found within the appsettings.Test.json file
    /// </summary>
    public class TestBase : IDisposable
    {
        public void Dispose()
        {
        }
    }
}