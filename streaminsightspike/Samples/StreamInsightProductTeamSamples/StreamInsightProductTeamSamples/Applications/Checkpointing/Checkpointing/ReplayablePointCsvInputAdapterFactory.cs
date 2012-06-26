//*********************************************************
//
// Copyright Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of
// the License at
//
// http://www.apache.org/licenses/LICENSE-2.0 
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES
// OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES
// OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache 2 License for the specific language
// governing permissions and limitations under the License.
//
//*********************************************************

namespace StreamInsight.Samples.Checkpointing
{
    using System;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    /// <summary>
    /// A factory for ReplayablePointCsvAdapters
    /// </summary>
    public class ReplayablePointCsvInputAdapterFactory : IHighWaterMarkTypedInputAdapterFactory<ReplayablePointCsvInputAdapterConfig>
    {
        /// <summary>
        /// Create a ReplayablePointCsvInputAdapter with replay.
        /// </summary>
        /// <typeparam name="TPayload">Ignored: this always produced XYPayloads</typeparam>
        /// <param name="configInfo">The config info.</param>
        /// <param name="eventShape">The event shape; must be Point.</param>
        /// <param name="highWaterMark">The system-provided high-water mark to replay from.</param>
        /// <returns>The new adapter.</returns>
        public InputAdapterBase Create<TPayload>(ReplayablePointCsvInputAdapterConfig configInfo, EventShape eventShape, DateTimeOffset highWaterMark)
        {
            // We only support points.
            if (eventShape != EventShape.Point)
            {
                throw new ArgumentException("Got event shape = " + eventShape + "; only Point is supported.");
            }

            return new ReplayablePointCsvInputAdapter(configInfo.File, configInfo.Interval, highWaterMark);
        }

        /// <summary>
        /// Create a ReplayablePointCsvInputAdapter without replay: start at the beginning.
        /// </summary>
        /// <typeparam name="TPayload">Ignored: this always produced XYPayloads</typeparam>
        /// <param name="configInfo">The config info.</param>
        /// <param name="eventShape">The event shape; must be Point.</param>
        /// <returns>The new adapter.</returns>
        public InputAdapterBase Create<TPayload>(ReplayablePointCsvInputAdapterConfig configInfo, EventShape eventShape)
        {
            // We only support points.
            if (eventShape != EventShape.Point)
            {
                throw new ArgumentException("Got event shape = " + eventShape + "; only Point is supported.");
            }

            return new ReplayablePointCsvInputAdapter(configInfo.File, configInfo.Interval);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // NOP
        }
    }
}
