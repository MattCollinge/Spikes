// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2011. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace AdvantIQ.ExampleAdapters.Input.Facebook
{
    public class FacebookFactory : ITypedInputAdapterFactory<FacebookConfig>
    {
        public InputAdapterBase Create<TPayload>(FacebookConfig config, EventShape eventShape)
        {
            // Only support the point event model
            if (eventShape == EventShape.Point)
                return new FacebookInput(config);
            else
                return default(InputAdapterBase);
        }

        public void Dispose()
        {
        }
    }
}
