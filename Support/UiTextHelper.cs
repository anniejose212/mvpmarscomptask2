// FILE: UiTextHelper.cs
// PURPOSE: Utility for normalizing UI text and performing case-insensitive equality checks.

using System;
using System.Net;

namespace mars_nunit_json.Support
{
    public static class UiTextHelper
    {
       
        public static string Normalize(string s)
        {
            if (s == null)
                return "";

            return WebUtility.HtmlDecode(s).Trim();
        }

        
        public static bool EqNorm(string a, string b)
        {
            return string.Equals(Normalize(a), Normalize(b), StringComparison.OrdinalIgnoreCase);
        }
    }
}
