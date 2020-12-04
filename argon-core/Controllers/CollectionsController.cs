using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.Core;
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
            _log.LogInformation("Creating new instance");
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
            _log.LogDebug("ReadCollections called");
            var t = await _collectionManager.ListCollectionsAsync();
            return t;
        }

        /// <summary>
        /// Call this method in order to create a new collection
        /// </summary>
        /// <remarks>
        /// A collection is a top-level container for a series of items (documents).  Each collection may have a
        /// series of constraints configured against it, which loosely define how meta-data properties associated
        /// with items are handled.
        /// </remarks>
        /// <param name="cmd">Contains the information relating to the new collection</param>
        /// <returns></returns>
        /// <response code="201">Successful creation.</response>
        /// <response code="400">Bad request. May be for a number of reasons, such as uniqueness constraints being violated. Details given
        /// in response payload.</response>
        /// <response code="500">Internal server error - check the response payload</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Collection> CreateCollection([FromBody]CreateCollectionCommand cmd)
        {
            _log.LogDebug("CreateCollection called");
            var collection = await _collectionManager.CreateCollectionAsync(cmd);
            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            _log.LogDebug($"New collection successfully created {@collection}", collection);
            return collection;
            
        }

        /// <summary>
        /// Call this method in order to retrieve the details for an individual collection
        /// </summary>
        /// <remarks>
        /// If the specified collection exists, then this method will retrieve the details for it,
        /// including some top-level metrics including its length and overall size.
        /// </remarks>
        /// <param name="collectionId"></param>
        /// <returns></returns>
        [HttpGet("{collectionId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Collection> ReadCollection(Guid collectionId)
        {
            _log.LogDebug("ReadCollection called");
            var collection = await _collectionManager.ReadCollectionAsync(collectionId);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return collection;
        }

        /// <summary>
        /// Gets the constraint group for a given collection, if it exists
        /// </summary>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <returns></returns>
        [HttpGet("/api/v1/Collections/{collectionId}/Constraints")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ConstraintGroup?> ReadCollectionConstraints(Guid collectionId)
        {
            _log.LogDebug("ReadCollectionConstraints called");
            var collection = await _collectionManager.ReadCollectionAsync(collectionId);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return collection.ConstraintGroup;
        }

        /// <summary>
        /// Gets a specific constraint for a given collection
        /// </summary>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <param name="constraintId">The unique identifier for the constraint</param>
        /// <returns></returns>
        [HttpGet("/api/v1/Collections/{collectionId}/Constraints/{constraintId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Constraint?> ReadCollectionConstraints(Guid collectionId, Guid constraintId)
        {
            _log.LogDebug("ReadCollectionConstraints called");
            var collection = await _collectionManager.ReadCollectionAsync(collectionId);
            var constraintGroup = collection.ConstraintGroup;
            if (constraintGroup == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                return null;
            }
            else
            {
                var constraint = constraintGroup.Constraints.Find(c => c.Id.Equals(constraintId));
                if (constraint == null)
                {
                    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
                    return null;
                }
                else
                {
                    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
                    return constraint;
                }
            }
        }

        /// <summary>
        /// Updates the constraints associated with a given collection.  
        /// </summary>
        /// <remarks>
        /// The same format of command is used to create
        /// and update constraints.  If a constraint referenced by name already exists, then it is overwritten with the
        /// new details.  If no such constraint exists, then a new constraint is created and added to the collection.
        /// Constraints take effect on the next operation against the collection.
        /// </remarks>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <param name="cmds">A list of create/update commands</param>
        /// <returns></returns>
        [HttpPost("/api/v1/Collections/{collectionId}/Constraints")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ConstraintGroup?> UpdateCollectionConstraints(Guid collectionId, [FromBody]List<CreateOrUpdateConstraintCommand> cmds)
        {
            _log.LogDebug("ReadCollectionConstraints called");
            var collection = await _collectionManager.ReadCollectionAsync(collectionId);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return collection.ConstraintGroup;
        }

        /// <summary>
        /// Allows for the update of an individual collection
        /// </summary>
        /// <param name="collectionId">The unique identifier associated with the collection to update</param>
        /// <param name="cmd">A <see cref="PatchCollectionCommand"/> instance containing the updates to be made</param>
        /// <returns></returns>
        [HttpPatch("{collectionId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Collection> UpdateCollection(Guid collectionId, [FromBody] PatchCollectionCommand cmd)
        {
            _log.LogDebug("UpdateCollection called");
            var collection = await _collectionManager.UpdateCollectionAsync(collectionId, cmd);
            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            return collection;
        }
    }
}