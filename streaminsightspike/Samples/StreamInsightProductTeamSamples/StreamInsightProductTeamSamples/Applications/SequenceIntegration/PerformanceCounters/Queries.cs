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
    using System.Linq;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;

    static class Queries
    {
        /// <summary>
        /// Measures the processor utilization per core on the local machine averaged over hopping windows.
        /// </summary>
        /// <param name="application">Application hosting the query.</param>
        /// <returns>IObservable event sink for the StreamInsight query.</returns>
        public static dynamic ProcessorUtilizationPerCore(Application application)
        {
            // Construct a stream per core using LINQ to Objects and LINQ to StreamInsight.
            var streams = from id in Enumerable.Range(0, Environment.ProcessorCount)
                          select from s in InputData.CreateProcessorTimeInput(application, id)
                                 select new { ProcessorID = id, Value = s.Value };

            // Use LINQ to Objects Aggregate to union streams for all cores.
            var input = streams.Aggregate(CepStream.Union);

            // Measure processor utilization per core over 10-second windows that advance
            // in 1-second increments.
            var query = from s in input
                        group s by s.ProcessorID into g
                        from win in g.HoppingWindow(
                            TimeSpan.FromSeconds(10),
                            TimeSpan.FromSeconds(1),
                            HoppingWindowOutputPolicy.ClipToWindowEnd)
                        select new { Avg = win.Avg(s => s.Value), ProcessorID = g.Key };

            return query.ToObservable();
        }

        /// <summary>
        /// Produces output whenever the processor utilization exceeds a threshold consistently over some duration. Both the threshold
	    /// and duration are configurable in real-time via the UI.
        /// </summary>
        /// <param name="application">Application hosting the query.</param>
        /// <param name="configurationSource">Real-time configuration data from UI.</param>
        /// <returns>IObservable event sink for the StreamInsight query.</returns>
        public static dynamic ProcessorUtilizationSustainedThreshold(
            Application application,
            IObservable<ThresholdConfiguration> configurationSource)
        {
            const string dataStreamName = "dataStream";

            var dataStream = InputData.CreateProcessorTimeInput(application, streamName: dataStreamName);

            // first, smooth the processor time to avoid false negatives (when we dip below threshold)
            var smoothed = from win in dataStream.HoppingWindow(
                               TimeSpan.FromSeconds(2.0),
                               TimeSpan.FromSeconds(0.25),
                               HoppingWindowOutputPolicy.ClipToWindowEnd)
                           select new { Value = win.Avg(s => s.Value) };

            // drive advance time settings from the data stream to the configuration stream
            var ats = new AdvanceTimeSettings(
                null,
                new AdvanceTimeImportSettings(dataStreamName),
                AdvanceTimePolicy.Adjust);

            var configurationStream = configurationSource.ToPointStream(
                application,
                c => PointEvent.CreateInsert(DateTime.UtcNow, c),
                ats,
                "configurationStream");

            // treat the configuration stream as a signal: the start of each event is the end
            // of the previous event
            configurationStream = configurationStream
                .AlterEventDuration(e => TimeSpan.MaxValue)
                .ClipEventDuration(configurationStream, (s, e) => true);

            // join data and configuration streams
            var joined = from d in dataStream
                         from c in configurationStream
                         select new { d.Value, c.Duration, c.Threshold };

            // use alter lifetime to apply the requested duration
            joined = joined.AlterEventDuration(e => e.Payload.Duration);

            // detect when the threshold is sustained for the requested duration
            var query = from win in joined.SnapshotWindow(SnapshotWindowOutputPolicy.Clip)
                        select new { Min = win.Min(s => s.Value), Threshold = win.Avg(s => s.Threshold) } into a
                        where a.Min > a.Threshold
                        select a;

            return from p in query.ToPointObservable()
                   where p.EventKind == EventKind.Insert
                   select new { p.StartTime, p.Payload.Min };
        }
    }
}
