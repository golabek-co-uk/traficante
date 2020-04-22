using System.Linq;
using System.Text.RegularExpressions;

namespace Traficante.TSQL.Evaluator
{
    public class Operators
    {
        public bool Like(string content, string searchFor)
        {
            return
                new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\")
                              .Replace(searchFor, ch => @"\" + ch).Replace('_', '.')
                              .Replace("%", ".*") + @"\z", RegexOptions.Singleline).IsMatch(content);
        }

        public bool RLike(string content, string pattern)
        {
            return new Regex(pattern).IsMatch(content);
        }

        public int? LikeIndex(string content, string searchFor)
        {
            var regex = new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\")
                      .Replace(searchFor, ch => @"\" + ch)
                      .Replace('_', '.')
                      .Replace("%", ".*");

            if (regex.StartsWith(".*"))
                regex = $".*({regex.Substring(2)})";
            else
                regex = $"({regex})";

            var match = new Regex
                (@"\A" + regex + @"\z", RegexOptions.Singleline).Match(content);
            if (match != null && match.Success)
                return match.Groups[1].Index;
            return -1;
        }

        public bool Contains<T>(T value, T[] values)
        {
            return values.Contains(value);
        }
    }
}