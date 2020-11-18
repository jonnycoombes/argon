using System;
using JCS.Argon.Services.VSP;

namespace JCS.Argon.Model.Exceptions
{
    /// <summary>
    /// Exception that may be thrown during certain <see cref="IVSPFactory"/> operations
    /// </summary>
    public class InvalidVSPException : Exception
    {
        public InvalidVSPException() : base()
        {
        }
        
        public InvalidVSPException(string? message) : base(message)
        {
        }
    }
}