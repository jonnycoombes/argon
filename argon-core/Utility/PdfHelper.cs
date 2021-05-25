using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Filespec;
using iText.Kernel.Utils;
using Microsoft.AspNetCore.StaticFiles;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

namespace JCS.Argon.Utility
{
    /// <summary>
    ///     Static class containing any PDF utilities used within Argon
    /// </summary>
    public static class PdfHelper
    {
        /// <summary>
        ///     Static logger for this class
        /// </summary>
        private static readonly ILogger _log = Log.ForContext(typeof(PdfHelper));

        /// <summary>
        ///     Given a list of files, this function will attempt to combine them into a single contiguous PDF file.  Each PDF file within the
        ///     specified list of files will be stitched together into a new document
        /// </summary>
        /// <param name="target">The location to store the finalised PDF document.</param>
        /// <param name="entries">A list of file paths</param>
        /// <returns>A stream onto the final PDF document</returns>
        public static bool CombineFilesToPdf(string target, IEnumerable<string> entries)
        {
            LogMethodCall(_log);

            var filenames = entries as string[] ?? entries.ToArray();

            if (filenames.Length == 0)
            {
                LogWarning(_log, "Asked to create a PDF archive with no input files");
                return false;
            }

            try
            {
                LogVerbose(_log, $"Creating combined PDF archive at \"{target}\" for maximum of {filenames.Length} entries");
                var contentProvider = new FileExtensionContentTypeProvider();
                var pdfFiles = filenames.Where(s => Path.GetExtension(s).Equals(".pdf", StringComparison.CurrentCultureIgnoreCase));
                var nonPdfFiles = filenames.Where(s => !Path.GetExtension(s).Equals(".pdf", StringComparison.CurrentCultureIgnoreCase));

                using var stream = new FileStream(target, FileMode.Create);
                var combined = new PdfDocument(new PdfWriter(stream));
                var merger = new PdfMerger(combined);
                foreach (var path in pdfFiles)
                {
                    LogVerbose(_log, $"Merging \"{path}\" into target Pdf");
                    var reader = new PdfReader(path);
                    reader.SetUnethicalReading(true);
                    var docToMerge = new PdfDocument(reader);
                    merger.Merge(docToMerge, 1, docToMerge.GetNumberOfPages());
                }

                foreach (var path in nonPdfFiles)
                {
                    var attachmentType = contentProvider.TryGetContentType(path, out var contentType)
                        ? new PdfName(contentType)
                        : new PdfName("application/octet");
                    LogVerbose(_log, $"Attaching Pdf inline attachment: \"{path}\"");
                    var fileSpec = PdfFileSpec.CreateEmbeddedFileSpec(combined, File.ReadAllBytes(path), Path.GetFileName(path), Path
                        .GetFileName(path), new PdfDictionary(), attachmentType);
                    combined.AddFileAttachment(Path.GetFileName(path), fileSpec);
                }

                combined.Close();
            }
            catch (Exception ex)
            {
                LogWarning(_log, $"Exception during PDF merge operation: \"{ex.Message}\"");
                return false;
            }

            return true;
        }
    }
}