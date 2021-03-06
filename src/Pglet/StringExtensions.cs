using System;

namespace Pglet
{
    internal static class StringExtensions
    {
        public static string Camelize(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}
