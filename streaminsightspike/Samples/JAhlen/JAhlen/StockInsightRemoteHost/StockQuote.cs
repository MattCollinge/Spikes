// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ComplexEventProcessing;

namespace AdvantIQ.StockInsight
{
    /// <summary>
    /// Payload class for stock quote events.
    /// </summary>
    public class StockQuote
    {
        /// <summary>
        /// Unique ID of stock or index
        /// </summary>
        public string StockID { get; set; }

        /// <summary>
        /// Field identifier
        /// </summary>
        public string FieldID { get; set; }

        /// <summary>
        /// Numerical value
        /// </summary>
        public double Value { get; set; }
    }
}
