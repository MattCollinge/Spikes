//*********************************************************
//
//  Copyright (c) Microsoft. All rights reserved.
//  This code is licensed under the Apache 2.0 License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OR
//  CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
//  INCLUDING, WITHOUT LIMITATION, ANY IMPLIED WARRANTIES
//  OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

namespace StreamInsight.Samples.Adapters.OutputTracer
{
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    /// <summary>
    /// Factory to instantiate a tracer output adapter.    
    /// </summary>
    public sealed class TracerFactory : IOutputAdapterFactory<TracerConfig>
    {
        /// <summary>
        /// Returns an instance of a tracer output adapter.
        /// </summary>
        /// <param name="configInfo">Configuration passed from the query binding.</param>
        /// <param name="eventShape">Event shape requested from the query binding.</param>
        /// <param name="cepEventType">Event type produced by the bound query template.</param>
        /// <returns>An instance of a tracer output adapter.</returns>
        public OutputAdapterBase Create(TracerConfig configInfo, EventShape eventShape, CepEventType cepEventType)
        {
            OutputAdapterBase adapter = default(OutputAdapterBase);
            switch (eventShape)
            {
                case EventShape.Point:
                    adapter = new TracerPointOutputAdapter(configInfo, cepEventType);
                    break;

                case EventShape.Interval:
                    adapter = new TracerIntervalOutputAdapter(configInfo, cepEventType);
                    break;

                case EventShape.Edge:
                    adapter = new TracerEdgeOutputAdapter(configInfo, cepEventType);
                    break;
            }

            return adapter;
        }

        /// <summary>
        /// Dispose method.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
