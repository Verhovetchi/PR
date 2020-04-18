using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MSharp.Framework.Services
{
    public class SearchKeywordExtractor
    {
        public static string[] Extract(string raw)
        {
            raw = raw.ToLowerOrEmpty();

            var exactMatches = new Regex("\"(.+?)\"").Matches(raw).OfType<Match>().Select(c => c.Value).ToArray();

            var broadMatches = raw.Remove(exactMatches).Split(' ').Trim().ToArray();

            return exactMatches.Concat(broadMatches).Select(c => c.Trim('\"')).Trim().Distinct().ToArray();
        }
    }
}
