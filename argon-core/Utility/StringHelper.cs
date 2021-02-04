#region

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace JCS.Argon.Utility
{
    public static class StringHelper
    {
        /// <summary>
        ///     Simple utility for concatenating a list of strings - should be replaced by LINQ aggregate function
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string CollapseStringList(IEnumerable<string> source)
        {
            var sb = new StringBuilder();
            foreach (var s in source) sb.Append($"{s}{Environment.NewLine}");
            return sb.ToString();
        }
    }
}