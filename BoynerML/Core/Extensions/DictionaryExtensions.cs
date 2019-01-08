using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoynerML.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static void FillDictionary(this Dictionary<string, double> dict, string items, char delimiter)
        {
            if (!string.IsNullOrEmpty(items))
            {
                items = items.ToLowerInvariant();
                var split = items.Split(delimiter);
                foreach (var item in split)
                {
                    if (!string.IsNullOrEmpty(item.Trim()))
                        if (!dict.Any(x => x.Key == item))
                            dict.Add(item, 0);
                        else
                            dict[item]++;
                }
            }
        }

        public static void SetItemPresence(this Dictionary<string, double> dict, string items, char delimiter)
        {
            ResetDictionary(dict);
            if (!string.IsNullOrEmpty(items))
            {
                items = items.ToLowerInvariant();
                var split = items.Split(delimiter);
                foreach (var item in split)
                {
                    if (!string.IsNullOrEmpty(item.Trim()))
                        if (dict.Any(x => x.Key == item))
                            dict[item] = 1;
                }
            }
        }

        public static void SetItemScore(this Dictionary<string, double> dict, string items, char itemDelimiter, char scoreDelimiter)
        {
            ResetDictionary(dict);
            if (!string.IsNullOrEmpty(items))
            {
                items = items.PreprocessTag().ToLowerInvariant();
                var split = items.Split(itemDelimiter);
                foreach (var item in split)
                {
                    var itemKey = item.Trim();
                    double itemScore = 1;

                    var splitScore = item.Split(scoreDelimiter);
                    if (splitScore.Count() > 1)
                    {
                        itemKey = splitScore[0].Trim();
                        itemScore = splitScore[1].ToLocaleDouble();
                    }

                    if (!string.IsNullOrEmpty(itemKey))
                        if (dict.Any(x => x.Key == itemKey))
                            dict[itemKey] = itemScore;
                }
            }
        }

        public static string GetPresenceString(this Dictionary<string, double> dict)
        {
            var result = string.Empty;

            result = string.Join("','", dict.Values);
            if (!string.IsNullOrEmpty(result))
            {
                result = "'" + result + "'";
                result = result.Replace("'1'", GetBooleanString(true));
                result = result.Replace("'0'", GetBooleanString(false));
            }

            return result;
        }

        public static string GetFeatureHeadingsString(this Dictionary<string, double> dict, string featureSetPrefix)
        {
            var result = string.Empty;

            result = string.Join("," + featureSetPrefix + "", dict.Keys);
            if (!string.IsNullOrEmpty(result)) result = featureSetPrefix + result;

            return result;
        }

        public static void ResetDictionary(this Dictionary<string, double> dict)
        {
            foreach (var key in dict.Keys.ToList())
            {
                dict[key] = 0;
            }
        }

        private static string GetBooleanString(bool value)
        {
            var result = string.Empty;

            //result = value ? "y" : "n";
            result = value ? "1" : "0";

            return result;
        }
    }
}