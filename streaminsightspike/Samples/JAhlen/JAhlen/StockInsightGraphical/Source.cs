// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvantIQ.StockInsight.InputAdapters;

namespace AdvantIQ.StockInsight
{
    public class Source
    {
        public StockQuoteInputConfig Config { get; set; }
        public string Name { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double GridValueSize { get; set; }
    }
}
