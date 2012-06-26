// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvantIQ.StockInsight
{
    /// <summary>
    /// Payload class for StockGraphObserver
    /// </summary>
    [Serializable]
    public class StockSignal
    {
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Currently unused
        /// </summary>
        public string StockID { get; set; }

        /// <summary>
        /// Stock value
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// True for buy. False for sell.
        /// </summary>
        public bool BuySignal { get; set; }
    }
}
