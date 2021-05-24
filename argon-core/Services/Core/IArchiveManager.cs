using System.IO;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;

namespace JCS.Argon.Services.Core
{
    /// <summary>
    ///     Interface definitions for the archive manager, responsible for retrieval of documents (singular and bulk) from potentially multiple
    ///     underlying Content Server instances.
    /// </summary>
    public interface IArchiveManager
    {
        /// <summary>
        ///     The type of content to be generated during a bulk download operation
        /// </summary>
        public enum ArchiveDownloadType
        {
            /// <summary>
            ///     All the items within a container should be extracted as a zip archive
            /// </summary>
            ZipArchive,

            /// <summary>
            ///     All the items (where possible) should be combined into a single PDF file, with optional attachments
            /// </summary>
            PdfArchive
        }

        /// <summary>
        ///     Downloads the content of a single archive file, based on an archive tag and a path to the content
        /// </summary>
        /// <param name="tag">The archive tag - links to the <see cref="ArchiveStorageBinding.Tag" /> property</param>
        /// <param name="path">The path to the element to be downloaded</param>
        /// <param name="archiveType">
        ///     The <see cref="ArchiveDownloadType" /> specified (defaults to
        ///     <see cref="ArchiveDownloadType.ZipArchive" />
        /// </param>
        /// <returns>A tuple, where the first element is the MIME type of the returned content, the second is a stream over the content</returns>
        /// <exception cref="ArchiveManagerException">Thrown in the event of a fault</exception>
        public Task<DownloadContentResult> DownloadArchivedDocument(string tag, string path,
            ArchiveDownloadType archiveType = ArchiveDownloadType.ZipArchive);

        /// <summary>
        ///     Downloads a JSON representation of the meta-data associated with an existing archive object.  (Either a document or a folder).
        /// </summary>
        /// <param name="tag">The archive tag</param>
        /// <param name="path">The path to the archived item</param>
        /// <returns></returns>
        /// <exception cref="ArchiveManagerException">Thrown in the event of a fault</exception>
        public Task<string> DownloadArchivedMetadata(string tag, string path);

        /// <summary>
        ///     Class used to represent the result of a download operation within the archive
        /// </summary>
        public class DownloadContentResult
        {
            /// <summary>
            ///     The <see cref="Stream" /> associated with the content
            /// </summary>
            public Stream Stream { get; set; }

            /// <summary>
            ///     The filename to be associated with the downloaded content
            /// </summary>
            public string Filename { get; set; }

            /// <summary>
            ///     The MIMEType associated with the content
            /// </summary>
            public string MimeType { get; set; }
        }
    }
}