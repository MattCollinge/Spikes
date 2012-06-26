// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;
using Microsoft.ComplexEventProcessing.Extensibility;

namespace AdvantIQ.StockInsight
{
    public class BollingerBandsUDO : CepOperator<StockQuote, BollingerBandsPayload>
    {
        public const double K = 2.0;

        public override IEnumerable<BollingerBandsPayload> GenerateOutput(IEnumerable<StockQuote> stockQuotes)
        {
            try
            {
                // Calculate mean and count
                var sum = 0.0;
                var count = 0;
                var last = 0.0;
                foreach (var q in stockQuotes)
                {
                    sum += q.Value;
                    count++;
                    last = q.Value;
                }
                var mean = sum / count;

                // Add deviation squares
                sum = 0.0;
                foreach (var q in stockQuotes)
                {
                    sum += (q.Value - mean) * (q.Value - mean);
                }

                // Calculate standard deviation
                var stddev = Math.Sqrt(sum / count);

                // Calculate results
                var bb = new BollingerBandsPayload();
                bb.MidBand = mean;
                bb.UpperBand = mean + K * stddev;
                bb.LowerBand = mean - K * stddev;
                bb.BandWidth = (bb.UpperBand - bb.LowerBand) / bb.MidBand;
                bb.PercentB = (last - bb.LowerBand) / (bb.UpperBand - bb.LowerBand);
                bb.LastValue = last;

                return new BollingerBandsPayload[] { bb };
            }
            catch
            {
                // In case of failure, return no events
                return new BollingerBandsPayload[0];
            }
        }
    }
}
