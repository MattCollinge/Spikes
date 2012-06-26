//*********************************************************
//
// Copyright 2010 Microsoft Corporation
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

namespace StreamInsight.Samples.PatternDetector
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;
    using StreamInsight.Samples.Adapters.SimpleTextFileReader;
    using StreamInsight.Samples.Adapters.SimpleTextFileWriter;
    using StreamInsight.Samples.UserExtensions.Afa;

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
                // Define the runtime configuration for input adapter
                var stockTicksInputConf = new TextFileReaderConfig
                {
                    InputFileName = @"..\..\..\StockTicks.csv",
                    Delimiter = ',',
                    CtiFrequency = 9,
                    CultureName = "en-US",
                    InputFieldOrders = new Collection<string>() { "StockSymbol", "StockPrice", "PriceChange" }
                };

                // Define the runtime configuration for the output adapter.
                // Specify an empty file name, which will just dump the output
                // events on the console.
                var outputConf = new TextFileWriterConfig
                {
                    OutputFileName = string.Empty,
                    Delimiter = '\t'
                };

                try
                {
                    // Create application in the server. The application will serve 
                    // as a container for actual CEP objects and queries.
                    Console.WriteLine("Creating CEP Application");
                    Application application = server.CreateApplication("PatternDetectorSample");

                    // Create an input stream directly from the adapter,
                    var stockStream = CepStream<StockTick>.Create("stockTicksInput", typeof(TextFileReaderFactory), stockTicksInputConf, EventShape.Point);

                    // Execute pattern detection over the stream
                    var patternResult = from w in stockStream.TumblingWindow(TimeSpan.FromHours(1), HoppingWindowOutputPolicy.ClipToWindowEnd)
                                        select w.AfaOperator<StockTick, RegisterType>(typeof(AfaEqualDownticksUpticks).AssemblyQualifiedName.ToString());

                    // Alternate example of using a count window for pattern detection:
                    // var patternResult = from w in stockStream.CountByStartTimeWindow(4, CountWindowOutputPolicy.PointAlignToWindowEnd)
                    //                     select w.AfaOperator<StockTick, RegisterType>(typeof(AfaEqualDownticksUpticks).AssemblyQualifiedName.ToString());

                    // Turn this logic into a query, outputting to the tracer output adapter.
                    var query = patternResult.ToQuery(
                        application,
                        "EqualDownticksUpticksQuery",
                        "Detecting equal number of downticks and upticks in a stock quote stream",
                        typeof(TextFileWriterFactory),
                        outputConf,
                        EventShape.Point,
                        StreamEventOrder.FullyOrdered);

                    // Start the query
                    Console.WriteLine("Starting query");
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
                    RetrieveDiagnostics(server.GetDiagnosticView(new Uri("cep:/Server/Application/PatternDetectorSample/Query/EqualDownticksUpticksQuery")), Console.Out);

                    query.Stop();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.ReadLine();
                }
            }

            Console.WriteLine("\nPress enter to exit application");
            Console.ReadLine();
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
