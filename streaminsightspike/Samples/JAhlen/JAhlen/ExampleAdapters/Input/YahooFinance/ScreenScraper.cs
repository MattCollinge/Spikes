// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace AdvantIQ.ExampleAdapters.Input.YahooFinance
{
    /// <summary>
    /// Simple screen scraper that uses a regular expression
    /// to extract data.
    /// </summary>
    public class ScreenScraper
    {
        private string _url;
        private int _timeout;
        private Regex _regex;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="timeout">Timeout in ms</param>
        /// <param name="matchPattern">Regex pattern</param>
        public ScreenScraper(string url, int timeout, string matchPattern)
        {
            _url = url;
            _timeout = timeout;
            _regex = new Regex(matchPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Call web page and extract
        /// </summary>
        /// <returns>Extracted string</returns>
        public string Scrape()
        {
            var request = HttpWebRequest.Create(_url);
            request.Timeout = _timeout;
            var response = request.GetResponse();
            var sr = new StreamReader(response.GetResponseStream());
            var htmlSource = sr.ReadToEnd();
            response.Close();

            var match = _regex.Match(htmlSource);
            if (match != null)
            {
                return match.Groups[1].Value;
            }
            else
                return "";
        }
    }
}
