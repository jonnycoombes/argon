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

        private class NullAssertionException : Exception
        {
            public NullAssertionException(string message) : base(message)
            {
            }
        }
    }
}