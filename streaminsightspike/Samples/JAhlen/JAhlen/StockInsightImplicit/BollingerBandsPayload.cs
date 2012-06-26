// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvantIQ.StockInsight
{
    public class BollingerBandsPayload
    {
        public double UpperBand { get; set; }
        public double MidBand { get; set; }
        public double LowerBand { get; set; }
        public double PercentB { get; set; }
        public double BandWidth { get; set; }
        public double LastValue { get; set; }
    }
}
