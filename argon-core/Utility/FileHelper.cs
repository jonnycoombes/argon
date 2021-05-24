using System.IO;

namespace JCS.Argon.Utility
{
    /// <summary>
    /// Static helpers for file & directoy I/O go here
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// Simple utility that creates a new, random temporary directory
        /// </summary>
        /// <returns></returns>
        public static DirectoryInfo CreateTempDirectory()
        {
            var directoryName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            return Directory.CreateDirectory(directoryName);
        }
    }
}