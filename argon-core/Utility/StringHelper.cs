#region

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace JCS.Argon.Utility
{
    public static class StringHelper
    {
        public static string CollapseStringList(List<string> source)
        {
            var sb = new StringBuilder();
            foreach (var s in source) sb.Append($"{s}{Environment.NewLine}");

            return sb.ToString();
        }
    }
}