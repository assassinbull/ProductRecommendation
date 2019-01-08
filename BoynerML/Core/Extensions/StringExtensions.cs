using System;
using System.Globalization;
using System.Threading;

namespace BoynerML.Core.Extensions
{
    public static class StringExtensions
    {
        public static double ToLocaleDouble(this string str)
        {
            var result = double.Parse(str, CultureInfo.InvariantCulture);

            return result;
        }

        public static string PreprocessTag(this string str)
        {
            var punctuations = new string[] { "-", "\\", "'" };

            foreach (var punctuation in punctuations)
            {
                str = str.Replace(punctuation, "");
            }

            return str;
        }
    }
}