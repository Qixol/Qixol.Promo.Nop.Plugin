using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using global::Nop.Core.Html;

namespace Qixol.Nop.Promo.Core.Html
{
    public static class HtmlHelper
    {
        public static string ConvertHtmlToPlainText(string text,
            bool decode = false, bool replaceAnchorTags = false)
        {
            text = global::Nop.Core.Html.HtmlHelper.ConvertHtmlToPlainText(text, decode, replaceAnchorTags);

            text = ReplaceSpanTags(text);

            return text;
        }

        public static string ReplaceSpanTags(string text)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = Regex.Replace(text, @"<span\b[^>]+>([^<]*(?:(?!</span)<[^<]*)*)</span>", "$1", RegexOptions.IgnoreCase);
            return text;
        }
    }
}
