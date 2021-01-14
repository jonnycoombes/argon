using System;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace JCS.Argon.Tests.Unit
{
    /// <summary>
    /// Base class for unit and service tests.  Basically contains methods for mocking services as required,
    /// along with establishing DB contexts etc...this base class assumes and will use a separate series of
    /// application settings found within the appsettings.Test.json file
    /// </summary>
    public abstract class AbstractTestBase : IDisposable
    {
        /// <summary>
        /// The current configuration instance to be used during testing
        /// </summary>
        protected IConfiguration _configuration;

        /// <summary>
        /// Static logger instance
        /// </summary>
        protected static ILogger _log;

        /// <summary>
        /// Constructor which just sets up a bunch of things 
        /// </summary>
        protected AbstractTestBase()
        {
            
            ConfigureLogging();
        }
        
        private void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .Enrich.WithMachineName()
                .CreateLogger();
            _log = Log.ForContext<AbstractTestBase>();
        }
        
        public void Dispose()
        {
        }
    }
}