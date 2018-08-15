using System;
using System.Text;
using System.Text.RegularExpressions;

namespace app.common.extensions
{
    public static class StringExtensions  
    {
        // Reference: https://andrewlock.net/customising-asp-net-core-identity-ef-core-naming-conventions-for-postgresql/
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
    }
}