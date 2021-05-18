using System;
using JCS.Argon.Model.Exceptions;

namespace JCS.Argon.Services.Core
{
    /// <summary>
    ///     Exception type thrown by any implementors of <see cref="IArchiveManager" />
    /// </summary>
    public class ArchiveManagerException : ResponseAwareException
    {
        public ArchiveManagerException(int? statusHint, string? message) : base(statusHint, message)
        {
        }

        public ArchiveManagerException(int? statusHint, string? message, Exception? inner) : base(statusHint, message, inner)
        {
        }
    }
}