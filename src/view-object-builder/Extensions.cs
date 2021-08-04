using System;
using System.Collections.Generic;
using System.Linq;

namespace viewObjectBuilder
{
    public static class Extensions
    {
        public static string PluralizeName(this string name)
        {
            var vowels = "aeiou".ToArray();
            var word = name.ToArray();
            var nameEnd = word[name.Length - 1].ToString().ToLower() == "y" &&
                          !vowels.Contains(word[name.Length - 2]) ?
                "ies" :
                $"{word[name.Length - 1].ToString()}s";

            return $"{name.Substring(0, name.Length - 1)}{nameEnd}";
        }

        public static (int index, object value)[] LineIndex(this string text)
            => text.Split(Environment.NewLine).Index();

        public static (int index, object value)[] Index(this IEnumerable<object> values)
             => values
                .Select((v, i) => (index: i, value: v))
                .ToArray();

        public static bool StringLineDifference(string previousString, string newString)
        {
            var previousSplit = previousString.LineIndex();
            var newSplit = newString.LineIndex();

            var hasDifferentLines = previousSplit
                .Join(newSplit,
                    p => p.index,
                    n => n.index,
                    (p, n) => new
                    {
                        p = p.value,
                        n = n.value,
                    })
                .Any(f => !f.p.Equals(f.n));

            var hasDifferentLineCounts = previousSplit.Length != newSplit.Length;
            return hasDifferentLines || hasDifferentLineCounts;
        }
    }
}