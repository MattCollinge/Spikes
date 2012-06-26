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
    /// <summary>
    /// StreamInsight extention to calculate standard deviations.
    /// </summary>
    public class StandardDeviationUDA : CepAggregate<StockQuote, double>
    {
        /// <summary>
        /// Calculation.
        /// </summary>
        /// <param name="stockQuotes">Payloads</param>
        /// <returns></returns>
        public override double GenerateOutput(IEnumerable<StockQuote> stockQuotes)
        {
            // Calculate mean and count
            var sum = 0.0;
            var count = 0;
            foreach (var q in stockQuotes)
            {
                sum += q.Value;
                count++;
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

            return stddev;
        }
    }
}
