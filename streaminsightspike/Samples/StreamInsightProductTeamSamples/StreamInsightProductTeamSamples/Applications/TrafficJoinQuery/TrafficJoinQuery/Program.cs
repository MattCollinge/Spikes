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

namespace StreamInsight.Samples.TrafficJoinQuery
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;
    using StreamInsight.Samples.Adapters.SimpleTextFileReader;
    using StreamInsight.Samples.Adapters.SimpleTextFileWriter;

    /// <summary>
    /// Console application that demonstrates the explicit query binding API.
    /// </summary>
    internal class ExplicitBinding
    {
        /// <summary>
        /// Signal name for the handshake between the output adapter and this thread.
        /// </summary>
        private const string StopSignalName = "StopAdapter";

        /// <summary>
        /// Main program.
        /// </summary>
        internal static void Main()
        {
            Console.WriteLine("Creating CEP Server");

            // Creating an embedded server.
            //
            // In order to connect to an existing server instead of creating a
            // new one, Server.Connect() can be used, with the appropriate
            // endpoint exposed by the remote server.
            //
            // NOTE: replace "Default" with the instance name you provided
            // during StreamInsight setup.
            using (Server server = Server.Create("Default"))
            {
                try
                {
                    // Create application in the server. The application will serve 
                    // as a container for actual CEP objects and queries.
                    Console.WriteLine("Creating CEP Application");
                    Application application = server.CreateApplication("TrafficJoinSample");

                    // Create query logic as a query template
                    Console.WriteLine("Registering LINQ query template");
                    QueryTemplate queryTemplate = CreateQueryTemplate(application);

                    // Register adapter factories, so that the engine will be able 
                    // to instantiate adapters when the query starts.
                    Console.WriteLine("Registering Adapter Factories");
                    InputAdapter csvInputAdapter = application.CreateInputAdapter<TextFileReaderFactory>("CSV Input", "Reading tuples from a CSV file");
                    OutputAdapter csvOutputAdapter = application.CreateOutputAdapter<TextFileWriterFactory>("CSV Output", "Writing result events to a CSV file");

                    // bind query to event producers and consumers
                    QueryBinder queryBinder = BindQuery(csvInputAdapter, csvOutputAdapter, queryTemplate);

                    // Create bound query that can be run
                    Console.WriteLine("Registering bound query");
                    Query query = application.CreateQuery("TrafficSensorQuery", "Minute average count, filtered by location threshold", queryBinder);

                    Console.WriteLine("Start query");

                    // Start the query
                    query.Start();

                    // Wait for the query to be suspended - that is the state
                    // it will be in as soon as the output adapter stops due to
                    // the end of the stream.
                    DiagnosticView dv = server.GetDiagnosticView(query.Name);

                    while ((string)dv[DiagnosticViewProperty.QueryState] == "Running")
                    {
                        // Sleep for 1s and check again
                        Thread.Sleep(1000);
                        dv = server.GetDiagnosticView(query.Name);
                    }

                    // Retrieve some diagnostic information from the CEP server
                    // about the query.
                    Console.WriteLine(string.Empty);
                    RetrieveDiagnostics(server.GetDiagnosticView(new Uri("cep:/Server/EventManager")), Console.Out);
                    RetrieveDiagnostics(server.GetDiagnosticView(new Uri("cep:/Server/PlanManager")), Console.Out);
                    RetrieveDiagnostics(server.GetDiagnosticView(new Uri("cep:/Server/Application/TrafficJoinSample/Query/TrafficSensorQuery")), Console.Out);

                    query.Stop();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.ReadLine();
                }
            }

            Console.WriteLine("\npress enter to exit application");
            Console.ReadLine();
        }

        /// <summary>
        /// Binds the query template to the specified adapters.
        /// </summary>
        /// <param name="inputAdapter">Input adapter metadata object to bind to all query template inputs.</param>
        /// <param name="outputAdapter">Input adapter metadata object to bind to the query template output.</param>
        /// <param name="queryTemplate">Query template to bind to adapters.</param>
        /// <returns>The query binder.</returns>
        private static QueryBinder BindQuery(InputAdapter inputAdapter, OutputAdapter outputAdapter, QueryTemplate queryTemplate)
        {
            // Create a query binder, wrapping the query template.
            QueryBinder queryBinder = new QueryBinder(queryTemplate);

            // Define the runtime configuration for both input adapters.
            var sensorInputConf = new TextFileReaderConfig
            {
                InputFileName = @"..\..\..\TrafficSensors.csv",
                Delimiter = ',',
                CtiFrequency = 9,
                CultureName = "en-US",
                InputFieldOrders = new Collection<string>() { "SensorId", "AverageSpeed", "VehicularCount" }
            };
            var locationInputConf = new TextFileReaderConfig
            {
                InputFileName = @"..\..\..\TrafficSensorLocations.csv",
                Delimiter = ',',
                CtiFrequency = 100,
                CultureName = "en-US",
                InputFieldOrders = new Collection<string>() { "SensorId", "LocationId" }
            };

            // Define the runtime configuration for the output adapter.
            // Specify an empty file name, which will just dump the output
            // events on the console. Also pass an event handle name to 
            // synchronize the shutdown.
            var outputConf = new TextFileWriterConfig
            {
                OutputFileName = string.Empty,
                Delimiter = '\t'
            };

            // Bind input adapters to query template's input streams,
            // applying runtime configuration.
            // In this example, the given input file for sensor input 
            // contains interval events (each sensor reading has a start 
            // and end time), while the location input is represented by 
            // edge events (for each event, the end time is not known in 
            // advance).
            queryBinder.BindProducer("sensorInput", inputAdapter, sensorInputConf, EventShape.Interval);
            queryBinder.BindProducer("locationInput", inputAdapter, locationInputConf, EventShape.Edge);

            // Bind output adapter to query, applying runtime 
            // configuration.
            queryBinder.AddConsumer<TextFileWriterConfig>("queryresult", outputAdapter, outputConf, EventShape.Point, StreamEventOrder.FullyOrdered);
            return queryBinder;
        }

        /// <summary>
        /// Contains the query logic in form of a query template.
        /// </summary>
        /// <param name="application">Application to host the query template.</param>
        /// <returns>The new query template object.</returns>
        private static QueryTemplate CreateQueryTemplate(Application application)
        {
            // Create stream objects as basis for query template
            // specification. The specified names will be used when binding
            // the query template's inputs to event producers.
            CepStream<SensorReading> sensorStream = CepStream<SensorReading>.Create("sensorInput");
            CepStream<LocationData> locationStream = CepStream<LocationData>.Create("locationInput");

            // Extend duration of each sensor reading, so that they fall in
            // a one-minute sliding window. Group by sensor ID and calculate the 
            // average vehicular count per group within each window.
            // Include the grouping key in the aggregation result.
            var avgCount = from oneMinReading in sensorStream.AlterEventDuration(e => TimeSpan.FromMinutes(1))
                           group oneMinReading by oneMinReading.SensorId into oneGroup
                           from eventWindow in oneGroup.SnapshotWindow(SnapshotWindowOutputPolicy.Clip)
                           select new { avgCount = eventWindow.Avg(e => e.VehicularCount), SensorId = oneGroup.Key };

            // Join sensors and locations. Moreover, filter the count 
            // result by a threshold, which is looked up based on the
            // sensor location through a user-defined function.
            var joined = from averageEvent in avgCount
                         join locationData in locationStream
                         on averageEvent.SensorId equals locationData.SensorId
                         where averageEvent.avgCount > UserFunctions.LocationCountThreshold(locationData.LocationId)
                         select new
                         {
                             SensorId = locationData.SensorId,
                             LocationID = locationData.LocationId,
                             VehicularCount = averageEvent.avgCount
                         };

            return application.CreateQueryTemplate("SampleQueryTemplate", string.Empty, joined);
        }

        /// <summary>
        /// Takes a diagnostic view and dumps all its entries to the given 
        /// text writer.
        /// </summary>
        /// <param name="diagview">Diagnostic view to dump.</param>
        /// <param name="traceListener">Tracer to receive the diagnostic data.</param>
        private static void RetrieveDiagnostics(DiagnosticView diagview, System.IO.TextWriter traceListener)
        {
            // Display diagnostics for diagnostic view object
            traceListener.WriteLine("Diagnostic View for '" + diagview.ObjectName + "':");
            foreach (KeyValuePair<string, object> diagprop in diagview)
            {
                traceListener.WriteLine(" " + diagprop.Key + ": " + diagprop.Value);
            }
        }
    }
}
