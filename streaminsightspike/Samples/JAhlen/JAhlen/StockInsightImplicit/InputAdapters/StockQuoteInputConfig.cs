// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvantIQ.StockInsight.InputAdapters
{
    /// <summary>
    /// Configuration for input adapter
    /// </summary>
    public class StockQuoteInputConfig
    {
        public string ID { get; set; }
        public string Filename { get; set; }
        public string[] ColumnNames { get; set; }
        public DateTime StartDate { get; set; }
        public int Interval { get; set; }
    }
}
