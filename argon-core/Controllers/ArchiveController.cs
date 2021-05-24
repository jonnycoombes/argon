﻿using System;
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
        ///     This operation can be used in order to retrieve either single or multiple documents from the archive specified by the "tag" parameter
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="tag">The tag for the archive provider</param>
        /// <param name="path">The path to the item to be retrieved</param>
        /// <param name="meta">Whether or not the meta-data should be retrieved. Default value is <i>false</i></param>
        /// <param name="archive">
        ///     The type of archive to be generated during bulk downloads.  Can be either "zip" or "pdf". Note that this
        ///     parameter only makes sense if the path provided corresponds to a container (e.g. folder) location within the archive.
        /// </param>
        /// <returns></returns>
        /// <response code="200">Successful</response>
        /// <response code="400">An invalid tag or path has been supplied</response>
        /// <response code="404">The specified item couldn't be found in the archive</response>
        /// <response code="500">Internal server error - check the response payload</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/v1/[controller]/{tag}/{*path}")]
        public async Task<IActionResult> DownloadArchivedDocument(string tag, string path, [FromQuery]
            bool meta = false, [FromQuery]
            string archive = "zip")
        {
            LogMethodCall(_log);
            if (!meta)
            {
                var archiveType = IArchiveManager.ArchiveDownloadType.ZipArchive;
                if (archive.Equals("pdf"))
                {
                    archiveType = IArchiveManager.ArchiveDownloadType.PdfArchive;
                }

                var response = await _archiveManager.DownloadArchivedDocument(tag, Uri.UnescapeDataString(path), archiveType);
                return new FileStreamResult(response.Stream, response.MimeType)
                {
                    FileDownloadName = response.Filename
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