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

namespace StreamInsight.Samples.ComposingQueries
{
    using System;
    using System.ServiceModel;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;
    using Microsoft.ComplexEventProcessing.ManagementService;
    using StreamInsight.Samples.Adapters.DataGenerator;
    using StreamInsight.Samples.Adapters.OutputTracer;

    /// <summary>
    /// Console Application.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main routine.
        /// </summary>
        internal static void Main()
        {
            // Creating an embedded server.
            //
            // NOTE: replace "Default" with the instance name you provided
            // during StreamInsight setup.
            using (Server server = Server.Create("Default"))
            {
                // Comment out if you want to create a service host and expose
                // the server's endpoint:

                //ServiceHost host = new ServiceHost(server.CreateManagementService());

                //host.AddServiceEndpoint(
                //    typeof(IManagementService),
                //    new WSHttpBinding(SecurityMode.Message),
                //    "http://localhost/MyStreamInsightServer");

                // To enable remote connection / debugging, you also need to uncomment the
                // lines "host.Open()" and "host.Close()".
                // In addition, the URI needs to be provisioned for the
                // account that runs this process (unless Administrator). To do this, open
                // an admin shell and execute the following command, using the respective
                // user credentials:
                // netsh http add urlacl url=http://+:80/MyStreamInsightServer user=<domain\username>

                //host.Open();

                // Configuration of the data generator input adapter.
                var generatorConfig = new GeneratorConfig()
                {
                    CtiFrequency = 1,
                    EventInterval = 500,
                    EventIntervalVariance = 450,
                    DeviceCount = 5,
                    MaxValue = 100
                };

                // Configuration of the tracer output adapter.
                var tracerConfig = new TracerConfig()
                {
                    DisplayCtiEvents = false,
                    SingleLine = true,
                    TraceName = "Deltas",
                    TracerKind = TracerKind.Console
                };

                try
                {
                    var myApp = server.CreateApplication("DeviceReadings");

                    // Create an input stream directly from the adapter,
                    // requesting point events from the simulator.
                    var inputstream = CepStream<GeneratedEvent>.Create("input", typeof(GeneratorFactory), generatorConfig, EventShape.Point);

                    // Compute the delta of consecutive values, using a count window
                    // and a user-defined aggregate.
                    // Use group & apply to compute this for each device separately.
                    var deltas = from e in inputstream
                                 group e by e.DeviceId into eachGroup
                                 from win in eachGroup.CountByStartTimeWindow(2, CountWindowOutputPolicy.PointAlignToWindowEnd)
                                 select new { ValueDelta = win.Delta(e => e.Value), SourceID = eachGroup.Key };

                    // Annotate the original values with the delta events by joining them.
                    // The aggregate over the count window produces a point event at
                    // the end of the window, which coincides with the second event in
                    // the window, so that they can be joined.
                    var annotatedValues = from left in inputstream
                                          join right in deltas
                                          on left.DeviceId equals right.SourceID
                                          select new { DeviceID = left.DeviceId, left.Value, right.ValueDelta };

                    // Turn this logic into a query, outputting to the tracer output adapter.
                    var primaryQuery = annotatedValues.ToQuery(
                        myApp,
                        "Deltas",
                        string.Empty,
                        typeof(TracerFactory),
                        tracerConfig,
                        EventShape.Point,
                        StreamEventOrder.FullyOrdered);

                    // Start the query now.
                    primaryQuery.Start();

                    // Append and start another query through Dynamic Query Composition,
                    // passing the primary query object.
                    var secondaryQuery = ComposeSecondaryQuery(primaryQuery);

                    // Wait for keystroke to end.
                    Console.ReadLine();

                    // Stop both queries.
                    primaryQuery.Stop();
                    secondaryQuery.Stop();

                    Console.WriteLine("Query stopped. Press enter to exit application.");
                    Console.ReadLine();

                    //host.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.ReadLine();
                }
            }
        }

        /// <summary>
        /// Appends a secondary query to the specified query and starts it.
        /// </summary>
        /// <param name="primaryQuery">Primary query to append to.</param>
        /// <returns>The secondary query.</returns>
        private static Query ComposeSecondaryQuery(Query primaryQuery)
        {
            // Create a stream based on the output of the primary query.
            var inputstream = primaryQuery.ToStream<AnnotatedValue>();

            // Specify the secondary query logic: Select only a specific sensor.
            var filtered = from e in inputstream
                           where e.DeviceID == "0"
                           select e;

            // Find the maximum of all sensor values within 5 second windows -
            // provided the window contains one or more events.
            var result = from win in filtered.TumblingWindow(TimeSpan.FromSeconds(5), HoppingWindowOutputPolicy.ClipToWindowEnd)
                         select new { Max = win.Max(e => e.ValueDelta) };

            // Display the result of this query in the console window, but with
            // a different tracer name.
            var tracerConfig = new TracerConfig()
                {
                    DisplayCtiEvents = false,
                    SingleLine = true,
                    TraceName = "MaxDeltas",
                    TracerKind = TracerKind.Console
                };

            // Turn this into a query and start it.
            // This call does not need an application object, because it is
            // already determined by the primary query.
            //
            // The secondary query will start feeding from the result
            // of the primary query, after the primary query has already
            // been started and producing events. The secondary query will
            // receive the last CTI emitted by the primary query, plus at least
            // those events produced by the primary query that overlapped with
            // this CTI.
            var query = result.ToQuery("OneSensorMaxDelta", string.Empty, typeof(TracerFactory), tracerConfig, EventShape.Interval, StreamEventOrder.FullyOrdered);

            query.Start();

            return query;
        }

        /// <summary>
        /// Output type of the primary query.
        /// Dynamic query composition requires explicit types at the query
        /// boundaries.
        /// </summary>
        public class AnnotatedValue
        {
            /// <summary>
            /// Gets or sets the device ID.
            /// </summary>
            public string DeviceID { get; set; }

            /// <summary>
            /// Gets or sets the current reading value of the device.
            /// </summary>
            public float Value { get; set; }

            /// <summary>
            /// Gets or sets the delta between the previous and the current value.
            /// </summary>
            public float ValueDelta { get; set; }
        }
    }
}
