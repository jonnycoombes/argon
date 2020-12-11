using System.Net;
using System.Threading.Tasks;
using JCS.Argon.Utility;
using JCS.Argon.Model.Responses;
using JCS.Argon.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class ConfigurationController : BaseApiController
    {
        protected ICollectionManager _collectionManager;

        public ConfigurationController(ILogger<ConfigurationController> log, ICollectionManager collectionManager ) : base(log)
        {
            _log.LogDebug("Creating new instance");
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
            return new ConfigurationResponse
            {
                HostName = Dns.GetHostName(),
                Endpoint = HttpHelper.BuildEndpointFromContext(HttpContext).Replace("/Configuration", ""),
                Version = new AppVersion().ToString(),
                SchemaVersion = new AppVersion().ToStringSchema(),
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