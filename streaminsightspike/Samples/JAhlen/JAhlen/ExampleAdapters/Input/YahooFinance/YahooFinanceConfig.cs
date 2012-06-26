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
    /// Configuration for input adapter
    /// </summary>
    public class YahooFinanceConfig
    {
        public int Timeout { get; set; }
        public string StockSymbol { get; set; }
        public string StockName { get; set; }
        public int Interval { get; set; }
    }
}
