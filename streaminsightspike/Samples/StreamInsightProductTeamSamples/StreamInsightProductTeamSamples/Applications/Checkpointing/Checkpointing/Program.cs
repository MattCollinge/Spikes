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
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.ServiceModel;
    using System.Threading;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;
    using Microsoft.ComplexEventProcessing.ManagementService;
    
    /// <summary>
    /// This sample illustrates how to create a resilient application using checkpointing, replayable input adapters, 
    /// and deduplicating output adapters.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// This is the path of the metadata file to create. It is relative to the output path specified on
        /// the command line.
        /// </summary>
        private const string MetadataFileName = "metadata.sdf";

        /// <summary>
        /// This is the path used for checkpoint logs. It is relative to the output path spacified on the
        /// command line.
        /// </summary>
        private const string LogSubdirectoryName = "logs";

        /// <summary>
        /// Our main routine. Reads in command line arguments indicating the name of the instance and app,
        /// as well as where to read and write data; starts up an instance w/ checkpointing enabled; and
        /// (re)starts a set of queries on that instance.
        /// </summary>
        /// <param name="args">
        /// The command line arguments. We read:
        /// - The name of the instance to use.
        /// - The name of the app to use.
        /// - The input CSV file to read.
        /// - A path to write data under.
        /// </param>
        private static void Main(string[] args)
        {
            string instanceName = ConfigurationManager.AppSettings["instanceName"];
            string appName = ConfigurationManager.AppSettings["appName"];
            string targetPath = ConfigurationManager.AppSettings["targetPath"];
            string dataFile = ConfigurationManager.AppSettings["dataFile"];
            TimeSpan eventDelay = TimeSpan.Parse(ConfigurationManager.AppSettings["eventDelay"]);

            // Create the target path if needed.
            try
            {
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Failed to create output path.", e);
            }

            // Set up the metadata configuration to use SQL CE. This is required to use checkpointing.
            var metaConfig = new SqlCeMetadataProviderConfiguration 
            { 
                DataSource = Path.Combine(targetPath, MetadataFileName), 
                CreateDataSourceIfMissing = true 
            };

            // Set up resiliency. This needs a location to place the log files.
            var resConfig = new CheckpointConfiguration 
            { 
                LogPath = Path.Combine(targetPath, LogSubdirectoryName), 
                CreateLogPathIfMissing = true
            };

            try
            {
                // Create the server.
                using (Server server = Server.Create(instanceName, metaConfig, resConfig))
                {
                    // Create the host (primarily so that we can use the debugger.)
                    //using (ServiceHost host = CreateWebService(server))
                    {
                        // Create an AppManager to manage our application.
                        using (AppManager appMgr = new AppManager(server, appName))
                        {
                            // A dictionary of the queries we care about. These will be started or restarted as
                            // needed by the Start() routine.
                            var queries = new Dictionary<string, QueryCreator> 
                            {
                                { "passthrough", (app, name, desc) => CreatePassthroughQuery(app, name, desc, dataFile, targetPath, eventDelay) },
                                { "aggregation", (app, name, desc) => CreateAggregationQuery(app, name, desc, dataFile, targetPath, eventDelay) },
                                { "buckets", (app, name, desc) => CreateBucketQuery(app, name, desc, dataFile, targetPath, eventDelay) },
                            };

                            foreach (var name in queries.Keys)
                            {
                                if (!appMgr.ContainsQuery(name))
                                {
                                    Util.Log("Main", "Adding query '" + name + "'");
                                    appMgr.RunQuery(name, name, queries[name]);
                                }
                                else
                                {
                                    Util.Log("Main", "Query '" + name + "' already in app.");
                                }
                            }

                            appMgr.BeginCheckpointing();

                            // Stay alive until the user tells us to stop.
                            Console.WriteLine("*** Press <enter> to end. ***");

                            Console.ReadLine();

                            appMgr.StopCheckpointing();
                        }
                    }

                    Console.WriteLine("*** Disposing server. ***");
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine(); 
            }

            Console.WriteLine("*** Exiting. ***");
        }

        /// <summary>
        /// Create a web service for the server.
        /// </summary>
        /// <param name="server">The server to create a web service for.</param>
        /// <returns>The new ServiceHost.</returns>
        private static ServiceHost CreateWebService(Server server)
        {
            var host = new ServiceHost(server.CreateManagementService());

            // create a binding for the service
            WSHttpBinding binding = new WSHttpBinding(SecurityMode.Message);
            host.AddServiceEndpoint(
                typeof(IManagementService),
                binding, 
                "http://localhost:8090/");
            host.Open();
            return host;
        }

        /// <summary>
        /// Create a simple passthrough query.
        /// </summary>
        /// <param name="app">The application that will host the query.</param>
        /// <param name="queryName">The name of the query.</param>
        /// <param name="queryDescription">The description of the query.</param>
        /// <param name="inputFile">The file to read input from.</param>
        /// <param name="outputPath">The path under which to put our output. This will go to a file of the form [app].[query].out</param>
        /// <param name="eventFrequency">How often the input adapter should produce events. Zero will produce them as fast as possible.</param>
        /// <returns>The new query.</returns>
        private static Query CreatePassthroughQuery(Application app, string queryName, string queryDescription, string inputFile, string outputPath, TimeSpan eventFrequency)
        {
            // Create a configuration for our input adapter. 
            var inputConfig = new ReplayablePointCsvInputAdapterConfig(inputFile, eventFrequency);

            // Declare the input stream.
            var inputStream = CepStream<XYPayload>.Create(
                queryName + ":input",
                typeof(ReplayablePointCsvInputAdapterFactory),
                inputConfig,
                EventShape.Point,
                null);

            // Just a passthrough template.
            var q = from e in inputStream select e;

            // Create the query. This declares which output adapter to use, and also configures
            // the query for resiliency.
            Query rval = q.ToQuery(
                app,
                queryName,
                queryDescription,
                typeof(DedupingPointCsvOuputAdapterFactory),
                new DedupingPointCsvOutputAdapterConfig(Path.Combine(outputPath, GetOutputFileName(app.ShortName, queryName))),
                EventShape.Point,
                StreamEventOrder.FullyOrdered,
                true); // <== *** This says that the query is resilient and enables it for checkpointing. ***           

            return rval;
        }

        /// <summary>
        /// Create a simple aggregate query.
        /// </summary>
        /// <param name="app">The application that will host the query.</param>
        /// <param name="queryName">The name of the query.</param>
        /// <param name="queryDescription">The description of the query.</param>
        /// <param name="inputFile">The file to read input from.</param>
        /// <param name="outputPath">The path under which to put our output. This will go to a file of the form [app].[query].out</param>
        /// <param name="eventFrequency">How often the input adapter should produce events. Zero will produce them as fast as possible.</param>
        /// <returns>The new query.</returns>
        private static Query CreateAggregationQuery(Application app, string queryName, string queryDescription, string inputFile, string outputPath, TimeSpan eventFrequency)
        {
            // Create a configuration for our input adapter. 
            var inputConfig = new ReplayablePointCsvInputAdapterConfig(inputFile, eventFrequency);

            // Declare the input stream.
            var inputStream = CepStream<XYPayload>.Create(
                queryName + ":input",
                typeof(ReplayablePointCsvInputAdapterFactory),
                inputConfig,
                EventShape.Point,
                null);

            // Do some simple aggregation.
            var q = from window in inputStream.HoppingWindow(TimeSpan.FromHours(1), TimeSpan.FromMinutes(1))
                    select new { AvgX = window.Avg(e => e.X), AvgY = window.Avg(e => e.Y) };

            // Create the query. This declares which output adapter to use, and also configures
            // the query for resiliency.
            Query rval = q.ToQuery(
                app,
                queryName,
                queryDescription,
                typeof(DedupingPointCsvOuputAdapterFactory),
                new DedupingPointCsvOutputAdapterConfig(Path.Combine(outputPath, GetOutputFileName(app.ShortName, queryName)), new string[] { "AvgX", "AvgY" }),
                EventShape.Point,
                StreamEventOrder.FullyOrdered,
                true); // <== *** This says that the query is resilient and enables it for checkpointing. ***
            
            return rval;
        }

        /// <summary>
        /// Bucket things based on position and take counts.
        /// </summary>
        /// <param name="app">The application that will host the query.</param>
        /// <param name="queryName">The name of the query.</param>
        /// <param name="queryDescription">The description of the query.</param>
        /// <param name="inputFile">The file to read input from.</param>
        /// <param name="outputPath">The path under which to put our output. This will go to a file of the form [app].[query].out</param>
        /// <param name="eventFrequency">How often the input adapter should produce events. Zero will produce them as fast as possible.</param>
        /// <returns>The new query.</returns>
        private static Query CreateBucketQuery(Application app, string queryName, string queryDescription, string inputFile, string outputPath, TimeSpan eventFrequency)
        {
            // Create a configuration for our input adapter. 
            var inputConfig = new ReplayablePointCsvInputAdapterConfig(inputFile, eventFrequency);

            // Declare the input stream.
            var inputStream = CepStream<XYPayload>.Create(
                queryName + ":input",
                typeof(ReplayablePointCsvInputAdapterFactory),
                inputConfig,
                EventShape.Point,
                null);

            // Do some aggregation by leading digit of x and y coordinate.
            var q = from e in inputStream
                    group e by new { XBucket = (int)(e.X * 10), YBucket = (int)(e.Y * 10) } into xygroups
                    from window in xygroups.TumblingWindow(TimeSpan.FromDays(1))
                    select new
                    {
                        XBucket = xygroups.Key.XBucket,
                        YBucket = xygroups.Key.YBucket,
                        Count = window.Count(),
                        AvgX = window.Avg(e => e.X),
                        AvgY = window.Avg(e => e.Y)
                    };

            // Create the query. This declares which output adapter to use, and also configures
            // the query for resiliency.
            Query rval = q.ToQuery(
                app,
                queryName,
                queryDescription,
                typeof(DedupingPointCsvOuputAdapterFactory),
                new DedupingPointCsvOutputAdapterConfig(Path.Combine(outputPath, GetOutputFileName(app.ShortName, queryName)), new string[] { "XBucket", "YBucket", "AvgX", "AvgY", "Count" }),
                EventShape.Point,
                StreamEventOrder.FullyOrdered,
                true); // <== *** This says that the query is resilient and enables it for checkpointing. ***

            return rval;
        }

        /// <summary>
        /// Get a filename for the output.
        /// </summary>
        /// <param name="appName">The short name of the application.</param>
        /// <param name="queryName">The name of the query.</param>
        /// <returns>The filename.</returns>
        private static string GetOutputFileName(string appName, string queryName)
        {
            return Path.Combine(appName + "." + queryName + ".out");
        }
    }
}
