// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace AdvantIQ.ExampleAdapters.Output.ConsoleSimple
{
    /// <summary>
    /// Factory for instantiating of output adapter
    /// </summary>
    public class ConsoleFactory : ITypedOutputAdapterFactory<ConsoleConfig>
    {
        public OutputAdapterBase Create<TPayload>(ConsoleConfig configInfo, EventShape eventShape)
        {
            if (eventShape == EventShape.Point)
                return new ConsolePointOutput<TPayload>(configInfo);
            else if (eventShape == EventShape.Interval)
                return new ConsoleIntervalOutput<TPayload>(configInfo);
            else if (eventShape == EventShape.Edge)
                return new ConsoleEdgeOutput<TPayload>(configInfo);
            else
                return default(OutputAdapterBase);
        }

        public void Dispose()
        {
        }

   }
}
