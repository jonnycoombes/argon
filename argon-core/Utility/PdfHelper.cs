using Serilog;

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
        private static ILogger _log = Log.ForContext(typeof(PdfHelper));
    }
}