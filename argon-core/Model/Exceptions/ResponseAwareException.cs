using System;

namespace JCS.Argon.Model.Exceptions
{
    /// <summary>
    /// Base class for response exceptions.
    /// </summary>
    public abstract class ResponseAwareException : Exception
    {
        /// <summary>
        /// Default constructor that takes a response code hint
        /// </summary>
        /// <param name="statusHint"></param>
        /// <param name="message"></param>
        protected ResponseAwareException(int? statusHint, string? message)
            : base(message)
        {
            ResponseCodeHint = statusHint;
        }

        /// <summary>
        /// Allows the wrapping of an exception
        /// </summary>
        /// <param name="statusHint"></param>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        protected ResponseAwareException(int? statusHint, string? message, Exception? inner)
            : base(message, inner)
        {
            ResponseCodeHint = statusHint;
        }

        /// <summary>
        /// "Suggested" response code if bubbled up via the HTTP layer
        /// </summary>
        public int? ResponseCodeHint { get; set; }
    }
}