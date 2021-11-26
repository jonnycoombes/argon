#region

using System;

#endregion

namespace JCS.Argon.Utility
{
    public static class ObjectExtensions
    {
        public static void AssertNotNull(this object obj, object? target, string message)
        {
            if (target == null) throw new NullAssertionException(message);
        }

        /// <summary>
        /// 
        /// </summary>
        private class NullAssertionException : Exception
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="message"></param>
            public NullAssertionException(string message) : base(message)
            {
            }
        }
    }
}