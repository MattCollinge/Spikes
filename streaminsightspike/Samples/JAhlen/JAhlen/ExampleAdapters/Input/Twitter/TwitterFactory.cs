// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace AdvantIQ.ExampleAdapters.Input.Twitter
{
    /// <summary>
    /// Factory for instantiating of input adapter
    /// </summary>
    public class TwitterFactory : ITypedInputAdapterFactory<TwitterConfig>
    {
        public InputAdapterBase Create<TPayload>(TwitterConfig config, EventShape eventShape)
        {
            // Only support the point event model
            if (eventShape == EventShape.Point)
                return new TwitterInput(config);
            else
                return default(InputAdapterBase);
        }

        public void Dispose()
        {
        }
    }
}
