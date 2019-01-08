using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoynerML.Core.Extensions
{
    public static class ArrayExtensions
    {
        public static string[] TakeBestItems(this string[] array, int n)
        {
            var dict = new Dictionary<string, int>();
            for (int i = 0; i < array.Length; i++)
            {
                var weight = array.Length - i;
                if (!dict.Any(x => x.Key == array[i]))
                    dict.Add(array[i], weight);
                else
                    dict[array[i]] += weight;
            }

            var aggregateDict = new Dictionary<string, int>();
            var grp = dict.GroupBy(x => x.Key);
            foreach (var item in grp)
            {
                aggregateDict.Add(item.Key, item.Sum(x => x.Value));
            }
            return aggregateDict.OrderByDescending(x => x.Value).Select(x => x.Key).Take(n).ToArray();
        }
    }
}