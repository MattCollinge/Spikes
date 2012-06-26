﻿// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace AdvantIQ.StockInsight.OutputAdapters
{
    /// <summary>
    /// Factory for instantiating of output adapter
    /// </summary>
    public class StockSignalOutputFactory : ITypedOutputAdapterFactory<StockSignalOutputConfig>
    {
        public OutputAdapterBase Create<TPayload>(StockSignalOutputConfig config, EventShape eventShape)
        {
            // Only support the point event model
            if (eventShape == EventShape.Point)
                return new StockSignalTypedPointOutput(config);
            else
                return default(OutputAdapterBase);
        }

        public void Dispose()
        {
        }
    }
}