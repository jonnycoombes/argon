using System.Collections.Generic;
using System.Threading.Tasks;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.Core;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class CollectionsController : BaseApiController
    {

        /// <summary>
        /// An instance of <see cref="ICollectionManager"/>, used for most of the interactions
        /// with collections
        /// </summary>
        protected readonly ICollectionManager _collectionManager;
        
        /// <summary>
        /// Default constructor.  Parameters are DI'd at runtime
        /// </summary>
        /// <param name="log">A logger for logging</param>
        /// <param name="collectionManager">An instance of <see cref="ICollectionManager"/></param>
        public CollectionsController(ILogger<CollectionsController> log, ICollectionManager collectionManager) : base(log)
        {
            Log.LogInformation("Creating new instance");
            _collectionManager = collectionManager;
        }

        /// <summary>
        /// Call this method in order to retrieve a list of all current collections
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">Successful</response>
        /// <response code="500">Internal server error - check the response payload</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<List<Collection>> ReadCollections()
        {
            return await _collectionManager.ListCollections();
        }

        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Collection> CreateCollection([FromBody]CreateCollectionCommand cmd)
        {
            return await _collectionManager.CreateCollection(cmd);
        }
    }
}