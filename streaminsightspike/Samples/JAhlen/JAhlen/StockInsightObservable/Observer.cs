// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Threading;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Extensibility;

namespace AdvantIQ.StockInsight
{
    public class Observer : IObserver<StockQuote>
    {
        public void OnCompleted()
        {
            // Prevent failure if OnCompleted() is called several times by encapsulating it in a try-catch block
            try
            {
                // Retrieve wait handle and set signal
                var adapterStopSignal = EventWaitHandle.OpenExisting("StockInsightSignal");
                adapterStopSignal.Set();
            }
            catch
            {
            }
        }

        public void OnError(Exception error)
        {
            throw error;
        }

        public void OnNext(StockQuote value)
        {
            Console.WriteLine("Output: " +
                value.TimeStamp + " " +
                value.StockID + " " +
                value.FieldID + " " +
                value.Value.ToString("f2"));
        }
    }
}
