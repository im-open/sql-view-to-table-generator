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
            => text.Split(new [] { "\r\n", "\n" }, StringSplitOptions.None).Index();

        public static (int index, object value)[] Index(this IEnumerable<object> values)
             => values
                .Select((v, i) => (index: i, value: v))
                .ToArray();

        public static FileDifferences StringLineDifference(string previousString, string newString)
        {
            var previousSplit = previousString.LineIndex();
            var newSplit = newString.LineIndex();

            var lineDifferences = previousSplit
                .Join(newSplit,
                    p => p.index,
                    n => n.index,
                    (p, n) => new
                    {
                        index = p.index,
                        p = p.value,
                        n = n.value,
                    })
                .Where(f => !f.p.Equals(f.n))
                .ToDictionary(line => line.index, line => new LineDifferences(line.p.ToString(), line.n.ToString()));

            var hasDifferentLineCounts = previousSplit.Length != newSplit.Length;

            return new FileDifferences
            {
                HasDifferentLineCounts = hasDifferentLineCounts,
                LineDifferences = lineDifferences
            };
        }

        public class FileDifferences
        {
            public bool HasDifferentLineCounts { get; set; }
            public IDictionary<int, LineDifferences> LineDifferences { get; set; }

            public bool HasLineDifferences => this.LineDifferences?.Any() == true || this.HasDifferentLineCounts;
        }

        public class LineDifferences
        {
            public LineDifferences(string fileOneLine, string fileTwoLine)
            {
                FileOneLine = fileOneLine;
                FileTwoLine = fileTwoLine;
            }

            public string FileOneLine { get; set; }
            public string FileTwoLine { get; set; }
        }
    }
}
