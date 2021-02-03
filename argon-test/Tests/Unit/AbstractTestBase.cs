#region

using System;
using System.Data.Common;
using System.IO;
using System.Net.Http;
using System.Text;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.Core;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

namespace JCS.Argon.Tests.Tests.Unit
{
    /// <summary>
    ///     Base class for unit and service tests.  Basically contains methods for mocking services as required,
    ///     along with establishing DB contexts etc...this base class assumes and will use a separate series of
    ///     application settings found within the appsettings.Test.json file
    /// </summary>
    public abstract class AbstractTestBase : IDisposable
    {
        /// <summary>
        ///     Static logger instance
        /// </summary>
        private static ILogger _log;

        /// <summary>
        ///     The <see cref="ICollectionManager" /> instance for testing
        /// </summary>
        protected ICollectionManager _collectionManager;

        /// <summary>
        ///     The current configuration instance to be used during testing
        /// </summary>
        private IConfiguration _configuration;

        /// <summary>
        ///     The <see cref="IConstraintGroupManager" /> instance for testing
        /// </summary>
        private IConstraintGroupManager _constraintGroupManager;

        /// <summary>
        ///     The current sql context options
        /// </summary>
        private DbContextOptions<SqlDbContext> _contextOptions;

        /// <summary>
        ///     The <see cref="IDbCache" /> instance for testing
        /// </summary>
        private IDbCache _dbCache;

        /// <summary>
        ///     The db context to use within the tests
        /// </summary>
        private SqlDbContext _dbContext;

        /// <summary>
        ///     The <see cref="IItemManager" /> instance for testing
        /// </summary>
        protected IItemManager _itemManager;

        /// <summary>
        ///     The mocked/test api configuration options
        /// </summary>
        private IOptionsMonitor<ApiOptions> _options;

        /// <summary>
        ///     The <see cref="IPropertyGroupManager" /> intance for testing
        /// </summary>
        private IPropertyGroupManager _propertyGroupManager;

        /// <summary>
        ///     The mock service provider
        /// </summary>
        private IServiceProvider _serviceProvider;

        /// <summary>
        ///     The <see cref="IVirtualStorageManager" /> instance for testing
        /// </summary>
        protected IVirtualStorageManager _virtualStorageManager;

        /// <summary>
        ///     Constructor which just sets up a bunch of things
        /// </summary>
        protected AbstractTestBase()
        {
            MockConfiguration();
            ConfigureLogging();
            CreateContextOptions();
            MigrateDatabase();
            MockServices();
        }

        public void Dispose()
        {
            LogMethodCall(_log);
            if (_dbContext == null) return;
            LogInformation(_log, "Tearing down current test database");
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            LogInformation(_log, "Test database blatted");
            var vfsBinding = _options.CurrentValue.VirtualStorageOptions
                .Bindings.Find(b => b.ProviderType == "nativeFileSystem");
        }

        /// <summary>
        ///     Stands up the options for the db context
        /// </summary>
        private void CreateContextOptions()
        {
            LogMethodCall(_log);
#if __SQLSERVER_TESTS__
            _contextOptions = new DbContextOptionsBuilder<SqlDbContext>()
                            .UseSqlServer(_configuration.GetConnectionString("DefaultConnection"),
                                sqlServerOptionsAction: sqlOptions => { })
                            .EnableDetailedErrors().Options;
#else
            _contextOptions = new DbContextOptionsBuilder<SqlDbContext>()
                .UseSqlite(CreateInMemoryDatabase())
                .EnableSensitiveDataLogging()
                .Options;
#endif
        }

        /// <summary>
        ///     Creates an in-memory database for use within a given series of tests
        /// </summary>
        /// <returns>Connection to a new SQLite in-memory database</returns>
        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        /// <summary>
        ///     Migrates the database - i.e. creates all the necessary schema structures
        /// </summary>
        private void MigrateDatabase()
        {
            LogMethodCall(_log);
            LogInformation(_log, "Ensuring that we have a fresh database to test against");
            new SqlDbContext(_contextOptions).Database.Migrate();
            LogInformation(_log, "Database stand-up complete");
        }

        /// <summary>
        ///     Mock any services/providers that are required within the tests
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
            _dbCache = new DbCache(_options, _serviceProvider);
            _serviceProvider.GetService(typeof(IDbCache))
                .Returns(_dbCache);
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
        ///     Configures logging for use during tests (so that full logging may be used in test cases)
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
        ///     Stands up the <see cref="IConfiguration" /> instance to be used during testing
        /// </summary>
        private void MockConfiguration()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", true, true)
                .AddEnvironmentVariables()
                .Build();
            var options = new ApiOptions();
            _configuration.GetSection(ApiOptions.ConfigurationSection).Bind(options);
            _options = Substitute.For<IOptionsMonitor<ApiOptions>>();
            _options.CurrentValue.Returns(options);
        }

        /// <summary>
        ///     Helper function for creating a test instance of <see cref="IFormFile" />
        /// </summary>
        /// <param name="name">The name to set against the form file</param>
        /// <param name="content">The string content</param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        protected static IFormFile CreateTestFormFile(string name, string content, string contentType = "text/plain")
        {
            LogMethodCall(_log);
            var bytes = Encoding.UTF8.GetBytes(content);
            var file = new FormFile(
                new MemoryStream(bytes),
                0,
                bytes.Length,
                "Content",
                name
            );
            return file;
        }
    }
}