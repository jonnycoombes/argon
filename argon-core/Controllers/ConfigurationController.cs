using System.Net;
using System.Threading.Tasks;
using JCS.Argon.Utility;
using JCS.Argon.Model.Responses;
using JCS.Argon.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;
using static JCS.Neon.Glow.Helpers.General.ReflectionHelpers;

namespace JCS.Argon.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class ConfigurationController : BaseApiController
    {
        /// <summary>
        /// Static logger
        /// </summary>
        private static ILogger _log = Log.ForContext<ConfigurationController>();

        /// <summary>
        /// The current <see cref="ICollectionManager"/>
        /// </summary>
        protected ICollectionManager _collectionManager;

        public ConfigurationController(ICollectionManager collectionManager) : base()
        {
            LogMethodCall(_log);
            _collectionManager = collectionManager;
        }

        /// <summary>
        /// Retrieves the current configuration for the Content Service Layer
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Call this method in order to retrieve information about the current CSL configuration.  In particular,
        /// this method can be used to retrieve information about the currently configured VSP providers.
        /// </remarks>
        /// <response code="200">Successful</response>
        /// <response code="500">Internal server error - check the logs</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ConfigurationResponse> Get()
        {
            LogMethodCall(_log);
            return new ConfigurationResponse
            {
                HostName = Dns.GetHostName(),
                Endpoint = HttpHelper.BuildEndpointFromContext(HttpContext).Replace("/Configuration", ""),
                Version = GetApplicationAssemblyVersion(),
                SchemaVersion = GetApplicationAssemblyVersion(),
                Bindings = _collectionManager.GetStorageBindings(),
                Metrics = new Metrics
                {
                    TotalCollections = await _collectionManager.CountCollectionsAsync(),
                    TotalItems = await _collectionManager.CountTotalItemsAsync(),
                    TotalVersions = await _collectionManager.CountTotalVersionsAsync()
                }
            };
        }
    }
}