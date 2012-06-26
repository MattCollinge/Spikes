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
    /// Holds extension methods to allow aggregate functions to be used within StreamInsight LINQ queries
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Calculates the Standard Deviation
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        [CepUserDefinedAggregate(typeof(StandardDeviationUDA))]
        public static double StandardDeviation(this CepWindow<StockQuote> window)
        {
            // This method is actually never executed. Instead StreamInsight 
            // invokes the StandardDeviationUDA class.

            // Throw an error if method is executed.
            throw CepUtility.DoNotCall();
        }

        [CepUserDefinedOperator(typeof(BollingerBandsUDO))]
        public static BollingerBandsPayload BollingerBands(this CepWindow<StockQuote> window)
        {
            // This method is actually never executed. Instead StreamInsight 
            // invokes the BollingerBandsUDA class.

            // Throw an error if method is executed.
            throw CepUtility.DoNotCall();
        }
    }
}
