using System;
using System.IO;
using System.Net.Http;
using System.Text;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.Core;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

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
        /// The mocked/test api configuration options
        /// </summary>
        protected IOptionsMonitor<ApiOptions> _options;

        /// <summary>
        /// The current sql context options
        /// </summary>
        protected DbContextOptions<SqlDbContext> _contextOptions;

        /// <summary>
        /// The db context to use within the tests
        /// </summary>
        protected SqlDbContext _dbContext;

        /// <summary>
        /// The mock service provider
        /// </summary>
        protected IServiceProvider _serviceProvider;

        /// <summary>
        /// Static logger instance
        /// </summary>
        protected static ILogger _log;

        /// <summary>
        /// The <see cref="ICollectionManager"/> instance for testing
        /// </summary>
        protected ICollectionManager _collectionManager;
        
        /// <summary>
        /// The <see cref="IItemManager"/> instance for testing
        /// </summary>
        protected IItemManager _itemManager;

        /// <summary>
        /// The <see cref="IPropertyGroupManager"/> intance for testing
        /// </summary>
        protected IPropertyGroupManager _propertyGroupManager;

        /// <summary>
        /// The <see cref="IConstraintGroupManager"/> instance for testing
        /// </summary>
        protected IConstraintGroupManager _constraintGroupManager;

        /// <summary>
        /// The <see cref="IVirtualStorageManager"/> instance for testing
        /// </summary>
        protected IVirtualStorageManager _virtualStorageManager;

        /// <summary>
        /// Constructor which just sets up a bunch of things 
        /// </summary>
        protected AbstractTestBase()
        {
            MockConfiguration();
            ConfigureLogging();
            CreateContextOptions();
            MigrateDatabase();
            MockServices();
        }

        /// <summary>
        /// Stands up the options for the db context
        /// </summary>
        private void CreateContextOptions()
        {
            LogMethodCall(_log);
            _contextOptions = new DbContextOptionsBuilder<SqlDbContext>()
                .UseSqlServer(_configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptionsAction: sqlOptions => { })
                .EnableDetailedErrors().Options;
        }

        protected void MigrateDatabase()
        {
            LogMethodCall(_log);
            LogInformation(_log, "Ensuring that we have a fresh database to test against");
            new SqlDbContext(_contextOptions).Database.Migrate();
            LogInformation(_log, "Database stand-up complete");
        }

        /// <summary>
        /// Mock any services/providers that are required within the tests
        /// </summary>
        private void MockServices()
        {
            LogMethodCall(_log);
            
            // sub in a service provider
            _serviceProvider = Substitute.For<IServiceProvider>();

            // instantiate a db context for use during the test
            _dbContext = new SqlDbContext(_contextOptions);
            _serviceProvider.GetService(typeof(SqlDbContext))
                .Returns(_dbContext);
            _serviceProvider.GetService<SqlDbContext>()
                .Returns(_dbContext);

            // instantiate instances of the various services - may as well create them here
            // rather than just checking construction within tests, because most constructors
            // don't actually do a great deal (largely because of late-binding methods used to
            // access specific dependent services in the base)
            _virtualStorageManager = new VirtualStorageManager(_serviceProvider, new HttpClient(), _options);
            _serviceProvider.GetService(typeof(IVirtualStorageManager))
                .Returns(_virtualStorageManager);
            
            // note that we have to substitute for the 'GetService' type as opposed to the 
            // generic variants, because they ain't a working with NSubstitute...
            _collectionManager = new CollectionManager(_options, _serviceProvider);
            _serviceProvider.GetService(typeof(ICollectionManager))
                .Returns(_collectionManager);
            _itemManager = new ItemManager(_options, _serviceProvider);
            _serviceProvider.GetService(typeof(IItemManager))
                .Returns(_itemManager);
            _constraintGroupManager = new ConstraintGroupManager(_options, _serviceProvider);
            _serviceProvider.GetService(typeof(IConstraintGroupManager))
                .Returns(_constraintGroupManager);
            _propertyGroupManager = new PropertyGroupManager(_options, _serviceProvider);
            _serviceProvider.GetService(typeof(IPropertyGroupManager))
                .Returns(_propertyGroupManager);

        }

        /// <summary>
        /// Configures logging for use during tests (so that full logging may be used in test cases)
        /// </summary>
        private void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .Enrich.WithMachineName()
                .CreateLogger();
            _log = Log.ForContext<AbstractTestBase>();
        }

        /// <summary>
        /// Stands up the <see cref="IConfiguration"/> instance to be used during testing
        /// </summary>
        private void MockConfiguration()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            var options = new ApiOptions();
            _configuration.GetSection(ApiOptions.ConfigurationSection).Bind(options);
            _options = Substitute.For<IOptionsMonitor<ApiOptions>>();
            _options.CurrentValue.Returns(options);
        }
        
        /// <summary>
        /// Helper function for creating a test instance of <see cref="IFormFile"/>
        /// </summary>
        /// <param name="name">The name to set against the form file</param>
        /// <param name="content">The string content</param>
        /// <returns></returns>
        protected IFormFile CreateTestFormFile(string name, string content, string contentType = "text/plain")
        {
            LogMethodCall(_log);
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            var file= new FormFile(
                baseStream: new MemoryStream(bytes),
                baseStreamOffset: 0,
                length: bytes.Length,
                name: "Content",
                fileName: name
            );
            return file;
        }
        
        public void Dispose()
        {
            LogMethodCall(_log);
            if (_dbContext != null)
            {
                LogInformation(_log, "Tearing down current test database");
                _dbContext.Database.EnsureDeleted();
                _dbContext.Dispose();
                LogInformation(_log, "Test database blatted");
            }
        }
    }
}