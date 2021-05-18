using System;
using System.Text.Json;
using System.Threading.Tasks;
using JCS.Argon.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

namespace JCS.Argon.Controllers
{
    /// <summary>
    ///     Controller for handling any "archive" requests
    /// </summary>
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class ArchiveController : BaseApiController
    {
        /// <summary>
        ///     Static logger for this controller
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<CollectionsController>();

        /// <summary>
        ///     An instance of <see cref="IArchiveManager" />
        /// </summary>
        private readonly IArchiveManager _archiveManager;

        /// <summary>
        ///     Default constructor - must have an instance of <see cref="IArchiveManager" />
        /// </summary>
        /// <param name="archiveManager">A DI-injected instance of <see cref="IArchiveManager" /></param>
        public ArchiveController(IArchiveManager archiveManager)
        {
            LogMethodCall(_log);
            _archiveManager = archiveManager;
        }

        /// <summary>
        ///     This operation can be used in order to retrieve a single document item from the archive specified by the "tag" parameter
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="tag">The tag for the archive provider</param>
        /// <param name="path">The path to the item to be retrieved</param>
        /// <param name="meta">Whether or not the meta-data should be retrieved</param>
        /// <returns></returns>
        /// <response code="200">Successful</response>
        /// <response code="400">An invalid tag or path has been supplied</response>
        /// <response code="400">The specified item is actually a non-document</response>
        /// <response code="404">The specified item couldn't be found in the archive</response>
        /// <response code="500">Internal server error - check the response payload</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/v1/[controller]/{tag}/{*path}")]
        public async Task<IActionResult> DownloadArchivedDocument(string tag, string path, [FromQuery]
            bool meta = false)
        {
            LogMethodCall(_log);
            if (!meta)
            {
                var response = await _archiveManager.DownloadArchivedDocument(tag, Uri.UnescapeDataString(path));
                return new FileStreamResult(response.Item2, response.Item1.MimeType)
                {
                    FileDownloadName = response.Item1.Name
                };
            }
            else
            {
                var response = await _archiveManager.DownloadArchivedMetadata(tag, Uri.UnescapeDataString(path));
                return new JsonResult(JsonDocument.Parse(response));
            }
        }
    }
}