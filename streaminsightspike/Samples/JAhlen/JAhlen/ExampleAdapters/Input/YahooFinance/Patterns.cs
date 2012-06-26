// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvantIQ.ExampleAdapters.Input.YahooFinance
{
    /// <summary>
    /// Class for interfacing with Yahoo Finance
    /// </summary>
    public class Patterns
    {
        public string URL { get; set; }
        public string MatchPattern { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="instrument">Yahoo instrument name</param>
        public Patterns(string symbol)
        {
            URL = "http://finance.yahoo.com/q?s=" + symbol;

            // Add escape character to avoid messing up the RegEx
            string escSymbol = symbol.Replace("^", @"\^");
            MatchPattern = @"\<span id=""yfs_l10_" + escSymbol + 
                @"""\>(.+)\</span\>";
        }
    }
}
