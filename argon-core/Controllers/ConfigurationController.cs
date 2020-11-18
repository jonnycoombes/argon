using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Responses;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigurationController : BaseApiController
    {
        protected readonly IVSPFactory _vspFactory;
        
        public ConfigurationController(ILogger<ConfigurationController> log, IVSPFactory vspFactory) : base(log)
        {
            Log.LogDebug("Creating new instance");
            _vspFactory = vspFactory;
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
        public ConfigurationResponse Get()
        {
            return new ConfigurationResponse
            {
                Bindings = _vspFactory.GetConfigurations().ToList()
            };
        }
    }
}