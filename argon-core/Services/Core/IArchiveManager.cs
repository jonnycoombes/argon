using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.Soap.Opentext;

namespace JCS.Argon.Services.Core
{
    /// <summary>
    ///     Interface definitions for the archive manager, responsible for retrieval of documents (singular and bulk) from potentially multiple
    ///     underlying Content Server instances.
    /// </summary>
    public interface IArchiveManager
    {
        /// <summary>
        ///     Downloads the content of a single archive file, based on an archive tag and a path to the content
        /// </summary>
        /// <param name="tag">The archive tag - links to the <see cref="ArchiveStorageBinding.Tag" /> property</param>
        /// <param name="path">The path to the element to be downloaded</param>
        /// <returns></returns>
        /// <exception cref="ArchiveManagerException">Thrown in the event of a fault</exception>
        public Task<(Version, Stream)> DownloadArchivedDocument(string tag, string path);

        /// <summary>
        ///     Downloads a JSON representation of the meta-data associated with an existing archive object.  (Either a document or a folder).
        /// </summary>
        /// <param name="tag">The archive tag</param>
        /// <param name="path">The path to the archived item</param>
        /// <returns></returns>
        /// <exception cref="ArchiveManagerException">Thrown in the event of a fault</exception>
        public Task<JsonDocument> DownloadArchivedMetadata(string tag, string path);
    }
}