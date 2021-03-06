#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

namespace JCS.Argon.Controllers
{
    /// <summary>
    ///     The main controller for <see cref="Collection" /> related operations
    /// </summary>
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class CollectionsController : BaseApiController
    {
        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<CollectionsController>();

        /// <summary>
        ///     An instance of <see cref="ICollectionManager" />, used for most of the interactions
        ///     with collections
        /// </summary>
        protected readonly ICollectionManager _collectionManager;

        /// <summary>
        ///     Default constructor.  Parameters are DI'd at runtime
        /// </summary>
        /// <param name="collectionManager">An instance of <see cref="ICollectionManager" /></param>
        public CollectionsController(ICollectionManager collectionManager)
        {
            LogMethodCall(_log);
            _collectionManager = collectionManager;
        }

        /// <summary>
        ///     Call this method in order to retrieve a list of all current collections
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
            LogMethodCall(_log);
            var t = await _collectionManager.ListCollectionsAsync();
            return t;
        }

        /// <summary>
        ///     Call this method in order to create a new collection
        /// </summary>
        /// <remarks>
        ///     A collection is a top-level container for a series of items (documents).  Each collection may have a
        ///     series of constraints configured against it, which loosely define how meta-data properties associated
        ///     with items are handled.
        /// </remarks>
        /// <param name="command">Contains the information relating to the new collection</param>
        /// <returns></returns>
        /// <response code="201">Successful creation.</response>
        /// <response code="400">
        ///     Bad request. May be for a number of reasons, such as uniqueness constraints being violated. Details given
        ///     in response payload.
        /// </response>
        /// <response code="500">Internal server error - check the response payload</response>
        /// <returns>An instance of <see cref="Collection" /></returns>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Collection> CreateCollection([FromBody] CreateCollectionCommand command)
        {
            LogMethodCall(_log);
            var collection = await _collectionManager.CreateCollectionAsync(command);
            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            return collection;
        }

        /// <summary>
        ///     Call this method in order to retrieve the details for an individual collection
        /// </summary>
        /// <remarks>
        ///     If the specified collection exists, then this method will retrieve the details for it,
        ///     including some top-level metrics including its length and overall size.
        /// </remarks>
        /// <param name="collectionId"></param>
        /// <response code="200">Successful read</response>
        /// <response code="404">The specified collection doesn't exist</response>
        /// <response code="500">Internal server error</response>
        /// <returns>A <see cref="Collection" /> reference</returns>
        [HttpGet("{collectionId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Collection> ReadCollection(Guid collectionId)
        {
            LogMethodCall(_log);
            var collection = await _collectionManager.GetCollectionAsync(collectionId);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return collection;
        }

        /// <summary>
        ///     Call this method in order to delete a collection.  Note that only empty collections may be deleted.  If the collection contains any
        ///     items, then a 400 response will be generated
        /// </summary>
        /// <param name="collectionId">The unique identifier for the collection to delete</param>
        /// <response code="200">Successful deletion</response>
        /// <response code="404">The specified collection doesn't exist</response>
        /// <response code="400">The specified collection cannot be deleted because it contains valid items</response>
        /// <response code="500">Internal server error</response>
        /// <returns></returns>
        [HttpDelete]
        [Route("/api/v1/Collections/{collectionId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task DeleteCollection(Guid collectionId)
        {
            LogMethodCall(_log);
            await _collectionManager.DeleteCollection(collectionId);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
        }

        /// <summary>
        ///     Gets the constraint group for a given collection, if it exists
        /// </summary>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <returns>The <see cref="ConstraintGroup" /> for the <see cref="Collection" /></returns>
        /// <response code="200">Successful read</response>
        /// <response code="404">The specified collection doesn't exist</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("/api/v1/Collections/{collectionId}/Constraints")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ConstraintGroup?> ReadCollectionConstraints(Guid collectionId)
        {
            LogMethodCall(_log);
            var collection = await _collectionManager.GetCollectionAsync(collectionId);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return collection.ConstraintGroup;
        }

        /// <summary>
        ///     Gets a specific constraint for a given collection
        /// </summary>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <param name="constraintId">The unique identifier for the constraint</param>
        /// <response code="200">Successful read</response>
        /// <response code="404">The specified collection doesn't exist</response>
        /// <response code="500">Internal server error</response>
        /// <returns></returns>
        [HttpGet("/api/v1/Collections/{collectionId}/Constraints/{constraintId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Constraint?> ReadCollectionConstraint(Guid collectionId, Guid constraintId)
        {
            LogMethodCall(_log);
            var collection = await _collectionManager.GetCollectionAsync(collectionId);
            var constraintGroup = collection.ConstraintGroup;
            if (constraintGroup == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                return null;
            }

            var constraint = constraintGroup.Constraints?.Find(c => c.Id.Equals(constraintId));
            if (constraint == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                return null;
            }

            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return constraint;
        }

        /// <summary>
        ///     Updates the constraints associated with a given collection.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The same format of command is used to create and update constraints.  If a constraint referenced by name already exists, then it is
        ///         overwritten with the new details.  If no such constraint exists, then a new constraint is created and added to the collection.
        ///     </para>
        ///     <para>
        ///         Constraints take effect on the next operation against the collection.
        ///     </para>
        /// </remarks>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <param name="commands">A list of create/update commands</param>
        /// <response code="200">Successful update</response>
        /// <response code="404">The specified collection doesn't exist</response>
        /// <response code="500">Internal server error</response>
        /// <returns></returns>
        [HttpPost("/api/v1/Collections/{collectionId}/Constraints")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Collection> UpdateCollectionConstraints(Guid collectionId,
            [FromBody] List<CreateOrUpdateConstraintCommand> commands)
        {
            LogMethodCall(_log);
            var collection = await _collectionManager.UpdateCollectionConstraints(collectionId, commands);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return collection;
        }

        /// <summary>
        ///     Allows for the update of an individual collection
        /// </summary>
        /// <param name="collectionId">The unique identifier associated with the collection to update</param>
        /// <param name="command">A <see cref="PatchCollectionCommand" /> instance containing the updates to be made</param>
        /// <response code="200">Successful update</response>
        /// <response code="404">The specified collection doesn't exist</response>
        /// <response code="500">Internal server error</response>
        /// <returns></returns>
        [HttpPatch("{collectionId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Collection> UpdateCollection(Guid collectionId, [FromBody] PatchCollectionCommand command)
        {
            LogMethodCall(_log);
            var collection = await _collectionManager.UpdateCollectionAsync(collectionId, command);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return collection;
        }
    }
}