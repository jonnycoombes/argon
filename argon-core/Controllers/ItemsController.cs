using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using JCS.Argon.Model.Schema;
using JCS.Argon.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Version = JCS.Argon.Model.Schema.Version;

namespace JCS.Argon.Controllers
{
    [Route("/api/v1/Collections")]
    public class ItemsController : BaseApiController
    {
        /// <summary>
        /// The <see cref="CollectionManager"/> instance - DI'd 
        /// </summary>
        protected readonly ICollectionManager _collectionManager;

        /// <summary>
        /// The <see cref="ItemManager"/> instance - DI'd
        /// </summary>
        protected readonly IItemManager _itemManager;
        
        public ItemsController(ILogger<ItemsController> log, ICollectionManager collectionManager, IItemManager itemManager) : base(log)
        {
            _log.LogInformation("Creating new instance");
            _collectionManager = collectionManager;
            _itemManager = itemManager;
        }

        /// <summary>
        /// Retrieves a list of items for a given collection.
        /// </summary>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet]
        [Route("/api/v1/Collections/{collectionId}/Items")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<List<Item>> ReadItems(Guid collectionId)
        {
            _log.LogDebug($"Reading collection items for collection with id: {collectionId}");
            var collection = await _collectionManager.ReadCollectionAsync(collectionId);
            var items = await _itemManager.GetItemsForCollectionAsync(collection);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return items;
        }

        /// <summary>
        /// Reads the meta-data (including <see cref="PropertyGroup"/>) for a given item
        /// </summary>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <param name="itemId">The unique identifier for the item</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet]
        [Route("/api/v1/Collections/{collectionId}/Items/{itemId}/Properties")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Item> ReadItemMeta(Guid collectionId, Guid itemId)
        {
            _log.LogDebug($"Reading collection item for collection with id: {collectionId}, item id: {itemId}");
            var collection = await _collectionManager.ReadCollectionAsync(collectionId);
            var items = await _itemManager.GetItemForCollectionAsync(collection, itemId);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return items;
        }

        /// <summary>
        /// Reads the content for a specific collection item. By default, this will return the content of the latest version
        /// </summary>
        /// <remarks>
        /// This method will return the latest version of the item by default.  
        /// </remarks>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <param name="itemId">The unique identifier for the item</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet]
        [Route("/api/v1/Collections/{collectionId}/Items/{itemId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Item> ReadItemContent(Guid collectionId, Guid itemId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs an upload of new content to a given collection.  This will create the initial version for the content
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="file"></param>
        /// <remarks>
        /// This is a multi-part file upload request, with the addition that the "Headers" object may also be
        /// used in order to specify a map of meta-data values to be assigned against the content.  Where possible,
        /// automatic type-detection is performed on the meta-data pairs.
        /// </remarks>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <response code="201">Successful creation of new content</response>
        /// <response code="400">Bad request. May be for a number of reasons, such as uniqueness constraints being violated. Details given
        /// in response payload.</response>
        /// <response code="500">Internal server error - check the response payload</response>
        [HttpPost]
        [Route("/api/v1/Collections/{collectionId}/Items")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Item> CreateItemContent(Guid collectionId, [FromForm(Name = "Content")] IFormFile file)
        {
            Dictionary<string, object>? properties= null;
            if (Request.Form.ContainsKey("Properties"))
            {
                properties = JsonSerializer.Deserialize<Dictionary<string, object>>(Request.Form["Properties"]);
            }
            var collection = await _collectionManager.ReadCollectionAsync(collectionId);
            var item = await _itemManager.AddItemToCollectionAsync(collection, properties, file); 
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return item;
        }
        
        /// <summary>
        /// Will add a new version to a given item of content 
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="itemId">The </param>
        /// <param name="file"></param>
        /// <remarks>
        ///This is a multi-part file upload request, with the addition that the "Headers" object may also be
        /// used in order to specify a map of meta-data values to be assigned against the content.  Where possible,
        /// automatic type-detection is performed on the meta-data pairs.
        /// </remarks>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <response code="201">Successful creation of new content</response>
        /// <response code="400">Bad request. May be for a number of reasons, such as uniqueness constraints being violated. Details given
        /// in response payload.</response>
        /// <response code="500">Internal server error - check the response payload</response>
        [HttpPost]
        [Route("/api/v1/Collections/{collectionId}/Items/{itemId}/Versions")]
        [Consumes("multipart/form")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<List<Version>> CreateItemVersion(Guid collectionId, Guid itemId, [FromForm(Name = "version")] IFormFile file)
        {
            Dictionary<string, object>? properties = null;
            if (Request.Form.ContainsKey("Properties"))
            {
                properties = JsonSerializer.Deserialize<Dictionary<string, object>>(Request.Form["Properties"]);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the content of a specific item version
        /// </summary>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <param name="itemId">The unique identifier for the item</param>
        /// <param name="versionId">The unique identifier for the item version</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet]
        [Route("/api/v1/Collections/{collectionId}/Item/{itemId}/Versions/{versionId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Item> ReadItemVersionMeta(Guid collectionId, Guid itemId, Guid versionId)
        {
            throw new NotImplementedException();
        }
    }
}