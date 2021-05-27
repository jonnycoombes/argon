using System;
using System.Linq;
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
        ///     This endpoint can be used to retrieve either single or multiple documents from the archive identified by the "tag"
        /// parameter.  The value of this parameter should correspond to a configured archive provider hosted by the Argon endpoint.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="tag">A tag value which must relate to a configured archived provider</param>
        /// <param name="path">The path to the item to be retrieved.  This path should be relative to the Enterprise Volume, and should
        /// utilise the '/' character as a path delimeter</param>
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
            var impersonationUser = ResolveImpersonationUser();
            if (!meta)
            {
                var archiveType = IArchiveManager.ArchiveType.ZipArchive;
                if (archive.Equals("pdf"))
                {
                    archiveType = IArchiveManager.ArchiveType.PdfArchive;
                }

                var response =
                    await _archiveManager.DownloadArchivedDocument(tag, Uri.UnescapeDataString(path), archiveType, impersonationUser);
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

        /// <summary>
        ///     Function which will attempt to extract an impersonation user from an inbound request/claimset
        /// </summary>
        /// <returns>A nullable string.  If non-null, the value is passed through to underlying services as an impersonation identity</returns>
        private string? ResolveImpersonationUser()
        {
            return null;
        }
    }
}