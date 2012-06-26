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

namespace StreamInsight.Samples.Adapters.DataGenerator
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    /// <summary>
    /// Factory to instantiate a data generator input adapter.
    /// </summary>
    public sealed class GeneratorFactory : ITypedInputAdapterFactory<GeneratorConfig>,
                                           ITypedDeclareAdvanceTimeProperties<GeneratorConfig>
    {
        /// <summary>
        /// Returns an instance of a data generator input adapter.
        /// </summary>
        /// <typeparam name="TPayload">Type of the payload.</typeparam>
        /// <param name="configInfo">Configuration passed from the query binding.</param>
        /// <param name="eventShape">Event shape requested from the query binding.</param>
        /// <returns>An instance of a data generator input adapter.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By Design")]
        public InputAdapterBase Create<TPayload>(GeneratorConfig configInfo, EventShape eventShape)
        {
            switch (eventShape)
            {
                case EventShape.Edge:
                    return new GeneratorAdapterEdge(configInfo);

                case EventShape.Interval:
                    return new GeneratorAdapterInterval(configInfo);

                case EventShape.Point:
                    return new GeneratorAdapterPoint(configInfo);

                default:
                    throw new ArgumentException("Unknown event shape");
            }
        }

        /// <summary>
        /// Dispose method.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Specifies the CTI behavior of this adapter. This method will be called whenever
        /// a new adapter instance is needed, with the respective configuration and event type.
        /// </summary>
        /// <typeparam name="TPayload">Type of the payload.</typeparam>
        /// <param name="configInfo">Configuration passed from the query binding.</param>
        /// <param name="eventShape">Event shape requested from the query binding.</param>
        /// <returns>An instance of AdapterAdvanceTimeSettings.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By Design")]
        public AdapterAdvanceTimeSettings DeclareAdvanceTimeProperties<TPayload>(GeneratorConfig configInfo, EventShape eventShape)
        {
            // Inject a CTI one tick _after_ (negative delay) each event and
            // drop violating events. This ensures maximum liveliness, since
            // each event is immediately committed.  Note that this will drop
            // duplicate events (events collocated in time) when enqueued by
            // the input adapter.
            return new AdapterAdvanceTimeSettings(
                new AdvanceTimeGenerationSettings(configInfo.CtiFrequency, TimeSpan.FromTicks(-1)),
                AdvanceTimePolicy.Drop);
        }
    }
}
