using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Unicorn.Web.Core.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveHtml(this string html)
        {
            return Regex.Replace(html, @"<[^>]*>", String.Empty);
        }
    }
}
