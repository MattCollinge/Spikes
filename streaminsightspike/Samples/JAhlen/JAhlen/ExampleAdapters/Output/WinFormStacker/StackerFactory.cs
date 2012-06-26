// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace AdvantIQ.ExampleAdapters.Output.WinFormStacker
{
    public class StackerFactory : ITypedOutputAdapterFactory<StackerConfig>
    {
        public OutputAdapterBase Create<TPayload>(StackerConfig configInfo, EventShape eventShape)
        {
            if (eventShape == EventShape.Point)
                return new StackerPointOutput<TPayload>(configInfo);
            else if (eventShape == EventShape.Interval)
                return new StackerIntervalOutput<TPayload>(configInfo);
            else if (eventShape == EventShape.Edge)
                return new StackerEdgeOutput<TPayload>(configInfo);
            else
                return default(OutputAdapterBase);
        }

        public void Dispose()
        {
        }

    }
}
