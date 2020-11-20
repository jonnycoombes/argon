using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace JCS.Argon.Utility
{
    public static class StringUtilities
    {
        public static string CollapseStringList(List<string> source)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var s in source)
            {
                sb.Append($"{s}{Environment.NewLine}");
            }
            return sb.ToString();
        }
    }
}