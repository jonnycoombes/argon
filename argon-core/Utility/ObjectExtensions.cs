using System;

namespace JCS.Argon.Utility
{
    public static class ObjectExtensions
    {
        public static void AssertNotNull(this Object obj, object? target, string message)
        {
            if (target == null)
            {
                throw new NullAssertionException(message);
            }
        }

        public class NullAssertionException : Exception
        {
            public NullAssertionException(string message) : base(message)
            {
            }
        }
    }
}