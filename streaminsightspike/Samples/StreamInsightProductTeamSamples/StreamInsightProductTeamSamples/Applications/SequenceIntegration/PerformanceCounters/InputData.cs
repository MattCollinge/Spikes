// *********************************************************
//
//  Copyright (c) Microsoft. All rights reserved.
//  This code is licensed under the Apache 2.0 License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OR
//  CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
//  INCLUDING, WITHOUT LIMITATION, ANY IMPLIED WARRANTIES
//  OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
// *********************************************************

namespace StreamInsight.Samples.SequenceIntegration.PerformanceCounters
{
    using System;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;
    using System.Globalization;

    public static class InputData
    {
        public static CepStream<PerformanceCounterSample> CreateProcessorTimeInput(
            Application application,
            int? processorID = null,
            string streamName = null)
        {
            // Poll performance counter 4 times a second
            TimeSpan pollingInterval = TimeSpan.FromSeconds(0.25);

            // If no processorID is specified, take the total processor utilization
            string instanceName = processorID.HasValue
                ? processorID.Value.ToString(CultureInfo.InvariantCulture)
                : "_Total";

            return CreateInput(
                application,
                "Processor",
                "% Processor Time",
                instanceName,
                pollingInterval,
                streamName);
        }

        public static CepStream<PerformanceCounterSample> CreateInput(
            Application application, 
            string categoryName, 
            string counterName, 
            string instanceName, 
            TimeSpan pollingInterval,
            string streamName = null)
        {
            IObservable<PerformanceCounterSample> source = new PerformanceCounterObservable(categoryName, counterName, instanceName, pollingInterval);
            
            AdvanceTimeSettings advanceTimeSettings = AdvanceTimeSettings.IncreasingStartTime;

            return source.ToPointStream(application, s =>
                PointEvent.CreateInsert(s.StartTime, s),
                advanceTimeSettings,
                streamName);
        }
    }
}
