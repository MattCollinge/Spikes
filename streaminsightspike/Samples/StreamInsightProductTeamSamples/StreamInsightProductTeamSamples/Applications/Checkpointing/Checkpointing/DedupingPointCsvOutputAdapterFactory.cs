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
    /// A factory for DedupingPointCsvOutputAdapters.
    /// </summary>
    public class DedupingPointCsvOuputAdapterFactory : IHighWaterMarkOutputAdapterFactory<DedupingPointCsvOutputAdapterConfig>
    {
        /// <summary>
        /// Create a DedupingPointCsvOutputAdapter set to deduplicate.
        /// </summary>
        /// <param name="configInfo">The configuration information; this includes the target file.</param>
        /// <param name="eventShape">This must be EventShape.Point.</param>
        /// <param name="cepEventType">The output event type.</param>
        /// <param name="highWaterMark">The high-water mark (partially) marking the place in the output where duplicates begin.</param>
        /// <param name="highWaterMarkEventOffset">The offset from the high-water mark marking the place in the output where duplicates begin.</param>
        /// <returns>The requested adapter.</returns>
        public OutputAdapterBase Create(DedupingPointCsvOutputAdapterConfig configInfo, EventShape eventShape, CepEventType cepEventType, DateTimeOffset highWaterMark, int highWaterMarkEventOffset)
        {
            if (eventShape != EventShape.Point)
            {
                throw new ArgumentException("Got event shape = " + eventShape + "; only Point is supported.");
            }

            return new DedupingPointCsvOutputAdapter(configInfo.File, configInfo.Fields, cepEventType, highWaterMark, highWaterMarkEventOffset);
        }

        /// <summary>
        /// Create a DedupingPointCsvOutputAdapter with a fresh output file.
        /// </summary>
        /// <param name="configInfo">The configuration.</param>
        /// <param name="eventShape">The event shape: must be Point.</param>
        /// <param name="cepEventType">The output type.</param>
        /// <returns>The new adapter.</returns>
        public OutputAdapterBase Create(DedupingPointCsvOutputAdapterConfig configInfo, EventShape eventShape, CepEventType cepEventType)
        {
            if (eventShape != EventShape.Point)
            {
                throw new ArgumentException("Got event shape = " + eventShape + "; only Point is supported.");
            }

            return new DedupingPointCsvOutputAdapter(configInfo.File, configInfo.Fields, cepEventType);
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing">Release managed state.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
