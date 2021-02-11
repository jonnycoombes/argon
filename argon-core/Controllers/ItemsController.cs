#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    ///     Controller for handling item-related requests to the API
    /// </summary>
    [ApiController]
    [Route("/api/v1/Collections")]
    public class ItemsController : BaseApiController
    {
        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<ItemsController>();

        /// <summary>
        ///     The <see cref="CollectionManager" /> instance - DI'd
        /// </summary>
        protected readonly ICollectionManager _collectionManager;

        /// <summary>
        ///     The <see cref="ItemManager" /> instance - DI'd
        /// </summary>
        protected readonly IItemManager _itemManager;

        public ItemsController(ICollectionManager collectionManager, IItemManager itemManager)
        {
            LogMethodCall(_log);
            _collectionManager = collectionManager;
            _itemManager = itemManager;
        }

        /// <summary>
        ///     Retrieves a list of items for a given collection.
        /// </summary>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/v1/Collections/{collectionId}/Items")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<List<Item>> ReadItems(Guid collectionId)
        {
            LogMethodCall(_log);
            var collection = await _collectionManager.GetCollectionAsync(collectionId);
            var items = await _itemManager.GetItemsForCollectionAsync(collection);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return items;
        }

        /// <summary>
        ///     Reads the meta-data (including <see cref="PropertyGroup" />) for a given item
        /// </summary>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <param name="itemId">The unique identifier for the item</param>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/v1/Collections/{collectionId}/Items/{itemId}/Properties")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Item> ReadItemMeta(Guid collectionId, Guid itemId)
        {
            LogMethodCall(_log);
            var collection = await _collectionManager.GetCollectionAsync(collectionId);
            var items = await _itemManager.GetItemForCollectionAsync(collection, itemId);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return items;
        }

        /// <summary>
        ///     Reads the content for a specific collection item. By default, this will return the content of the latest version
        /// </summary>
        /// <remarks>
        ///     This method will return the latest version of the item by default.
        /// </remarks>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <param name="itemId">The unique identifier for the item</param>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/v1/Collections/{collectionId}/Items/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReadItemContent(Guid collectionId, Guid itemId)
        {
            LogMethodCall(_log);
            var collection = await _collectionManager.GetCollectionAsync(collectionId);
            var item = await _itemManager.GetItemForCollectionAsync(collection, itemId);
            var latestVersion = await _itemManager.GetCurrentItemVersionAsync(collection, item);
            var stream = await _itemManager.GetStreamForVersionAsync(collection, item, latestVersion);
            return new FileStreamResult(stream, latestVersion.MIMEType)
            {
                FileDownloadName = latestVersion.Name
            };
        }

        /// <summary>
        ///     Reads the contents of a specific version
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="itemId"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/v1/Collections/{collectionId}/Items/{itemId}/Versions/{versionId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReadVersionContent(Guid collectionId, Guid itemId, Guid versionId)
        {
            LogMethodCall(_log);
            var collection = await _collectionManager.GetCollectionAsync(collectionId);
            var item = await _itemManager.GetItemForCollectionAsync(collection, itemId);
            var version = await _itemManager.GetItemVersionAsync(collection, item, versionId);
            var stream = await _itemManager.GetStreamForVersionAsync(collection, item, version);
            return new FileStreamResult(stream, version.MIMEType)
            {
                FileDownloadName = version.Name
            };
        }

        /// <summary>
        ///     Performs an upload of new content to a given collection.  This will create the initial version for the content
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="file"></param>
        /// <remarks>
        ///     This is a multi-part file upload request, with the addition that the "Headers" object may also be
        ///     used in order to specify a map of meta-data values to be assigned against the content.  Where possible,
        ///     automatic type-detection is performed on the meta-data pairs.
        /// </remarks>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <response code="201">Successful creation of new content</response>
        /// <response code="400">
        ///     Bad request. May be for a number of reasons, such as uniqueness constraints being violated. Details given
        ///     in response payload.
        /// </response>
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
            LogMethodCall(_log);
            var properties = ExtractPropertiesFromRequest();
            var collection = await _collectionManager.GetCollectionAsync(collectionId);
            var item = await _itemManager.AddItemToCollectionAsync(collection, properties, file);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return item;
        }

        /// <summary>
        ///     Will add a new version to a given item of content
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="itemId">The </param>
        /// <param name="file"></param>
        /// <remarks>
        ///     This is a multi-part file upload request, with the addition that the "Headers" object may also be
        ///     used in order to specify a map of meta-data values to be assigned against the content.  Where possible,
        ///     automatic type-detection is performed on the meta-data pairs.
        /// </remarks>
        /// <returns></returns>
        /// <response code="201">Successful creation of new content</response>
        /// <response code="400">
        ///     Bad request. May be for a number of reasons, such as uniqueness constraints being violated. Details given
        ///     in response payload.
        /// </response>
        /// <response code="500">Internal server error - check the response payload</response>
        [HttpPost]
        [Route("/api/v1/Collections/{collectionId}/Items/{itemId}/Versions")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Item> CreateItemVersion(Guid collectionId, Guid itemId, [FromForm(Name = "Content")] IFormFile file)
        {
            LogMethodCall(_log);
            var properties = ExtractPropertiesFromRequest();
            var collection = await _collectionManager.GetCollectionAsync(collectionId);
            var item = await _itemManager.GetItemForCollectionAsync(collection, itemId);
            var revisedItem = await _itemManager.AddItemVersionToCollectionAsync(collection, item, properties, file);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return revisedItem;
        }

        /// <summary>
        ///     Will attempt the deletion of an existing item from a given collection
        /// </summary>
        /// <param name="collectionId">The identifier for the parent collection</param>
        /// <param name="itemId">The identifier for the item to delete</param>
        /// <returns></returns>
        /// <response code="200">Successful deletion of the item</response>
        /// <response code="500">Internal server error - check the response payload</response>
        /// [HttpDelete]
        [HttpDelete]
        [Route("/api/v1/Collections/{collectionId}/Items/{itemId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task DeleteItem(Guid collectionId, Guid itemId)
        {
            LogMethodCall(_log);
            var collection = await _collectionManager.GetCollectionAsync(collectionId);
            await _itemManager.DeleteItemFromCollection(collection, itemId);
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
        }

        /// <summary>
        ///     Retrieves the content of a specific item version
        /// </summary>
        /// <param name="collectionId">The unique identifier for the collection</param>
        /// <param name="itemId">The unique identifier for the item</param>
        /// <param name="versionId">The unique identifier for the item version</param>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/v1/Collections/{collectionId}/Item/{itemId}/Versions/{versionId}/Properties")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ItemVersion> ReadItemVersionMeta(Guid collectionId, Guid itemId, Guid versionId)
        {
            LogMethodCall(_log);
            var collection = await _collectionManager.GetCollectionAsync(collectionId);
            var item = await _itemManager.GetItemForCollectionAsync(collection, itemId);
            return await _itemManager.GetItemVersionAsync(collection, item, versionId);
        }
    }
}