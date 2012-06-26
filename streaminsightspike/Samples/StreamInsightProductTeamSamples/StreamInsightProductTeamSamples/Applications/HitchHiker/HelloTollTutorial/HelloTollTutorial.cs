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

namespace StreamInsight.Samples.HelloTollTutorial
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;
    using StreamInsight.Samples.Adapters.AsyncCsv;
    using StreamInsight.Samples.HelloToll;

    internal class HelloTollTutorial
    {
        internal static void Main()
        {
            using (var server = Server.Create("Default"))
            {
                // If you want to publish this server, or connect to an already existing server,
                // please see the product documentation "Publishing and Connecting to a StreamInsight Server"
                // to find out how to replace, or add to the above server creation code. You must publish
                // the server for the Event Flow debugger to be able to connect to the server.
                var application = server.CreateApplication("TutorialApp");

                // Define device configuration
                var inputConfig = new CsvInputConfig
                {
                    InputFileName = @"..\..\..\TollInput.txt",
                    Delimiter = new char[] { ',' },
                    BufferSize = 4096,
                    CtiFrequency = 1,
                    CultureName = "en-US",
                    Fields = new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" },
                    NonPayloadFieldCount = 2,
                    StartTimePos = 1,
                    EndTimePos = 2
                };

                var outputConfigForPassThroughQuery = new CsvOutputConfig
                {
                    // The adapter recognizes empty filename as a write to console
                    OutputFileName = string.Empty,
                    Delimiter = new string[] { "\t" },
                    CultureName = "en-US",
                    Fields = new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" }
                };

                var outputConfigForCountQuery = new CsvOutputConfig
                {
                    OutputFileName = string.Empty,
                    Delimiter = new string[] { "\t" },
                    CultureName = "en-US",
                    Fields = new List<string>() { "Count" }
                };

                // Define console trace listener
                TraceListener tracer = new ConsoleTraceListener();

                // set up advance time settings to enqueue CTIs - to deliver output in a timely fashion
                var advanceTimeGenerationSettings = new AdvanceTimeGenerationSettings(inputConfig.CtiFrequency, TimeSpan.Zero, true);
                var advanceTimeSettings = new AdvanceTimeSettings(advanceTimeGenerationSettings, null, AdvanceTimePolicy.Adjust);

                // Define input stream object, mapped to stream names
                // Instantiate an adapter from the input factory class
                var inputStream = CepStream<TollReading>.Create(
                                    "TollStream",
                                    typeof(CsvInputFactory),
                                    inputConfig,
                                    EventShape.Interval,
                                    advanceTimeSettings);


                var passthroughQuery = from e in inputStream select e;

                var countQuery = from w in inputStream.TumblingWindow(TimeSpan.FromMinutes(3), HoppingWindowOutputPolicy.ClipToWindowEnd)
                                    select new TollCount { Count = w.Count() };

                // Create a runnable query by binding the query template to the
                // input stream of interval events, and to an output stream of
                // fully ordered point events (through an output adapter instantiated
                // from the output factory class)
                Query query = countQuery.ToQuery(
                                    application,
                                    "HelloTollTutorial",
                                    "Hello Toll Query",
                                    typeof(CsvOutputFactory),
                                    outputConfigForCountQuery,
                                    EventShape.Interval,
                                    StreamEventOrder.FullyOrdered);

                query.Start();

                Console.WriteLine("*** Hit Return to see Query Diagnostics after this run ***");
                Console.WriteLine();
                Console.ReadLine();

                // Retrieve  diagnostic information from the CEP server about the query.
                // See the section "Connecting to and Publishing a Server" in the StreamInsight MSDN documentation on
                // how to make the debugger client connect to the server and retrieve these diagnostics
                Console.WriteLine(string.Empty);
                RetrieveDiagnostics(server.GetDiagnosticView(new Uri("cep:/Server/EventManager")), tracer);
                RetrieveDiagnostics(server.GetDiagnosticView(new Uri("cep:/Server/PlanManager")), tracer);
                RetrieveDiagnostics(server.GetDiagnosticView(new Uri("cep:/Server/Application/TutorialApp/Query/HelloTollTutorial")), tracer);

                DiagnosticSettings settings = new DiagnosticSettings(DiagnosticAspect.GenerateErrorReports, DiagnosticLevel.Always);
                server.SetDiagnosticSettings(new Uri("cep:/Server"), settings);

                tracer.WriteLine("Global Server Diagnostics");
                RetrieveDiagnostics(server.GetDiagnosticView(new Uri("cep:/Server/EventManager")), tracer);
                RetrieveDiagnostics(server.GetDiagnosticView(new Uri("cep:/Server/PlanManager")), tracer);
                RetrieveDiagnostics(server.GetDiagnosticView(new Uri("cep:/Server/Query")), tracer);
                tracer.WriteLine(string.Empty);
                tracer.WriteLine("Summary Query Diagnostics for");
                RetrieveDiagnostics(server.GetDiagnosticView(new Uri("cep:/Server/Application/TutorialApp/Query/HelloTollTutorial")), tracer);
                tracer.WriteLine(string.Empty);

                query.Stop();

                Console.WriteLine("*** Query Completed *** Hit Return to Exit");
                Console.WriteLine();
                Console.ReadLine();
            }

            return;
        }

        #region retrieve diagnostics
        private static void RetrieveDiagnostics(DiagnosticView diagview, TraceListener traceListener)
        {
            // Display diagnostics for diagnostic view object
            traceListener.WriteLine("Diagnostic View for '" + diagview.ObjectName + "':");
            foreach (KeyValuePair<string, object> diagprop in diagview)
            {
                traceListener.WriteLine(" " + diagprop.Key + ": " + diagprop.Value);
            }
        }
        #endregion
    }
}