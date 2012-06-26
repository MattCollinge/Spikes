// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;
using AdvantIQ.StockInsight.InputAdapters;
using AdvantIQ.StockInsight.OutputAdapters;

namespace AdvantIQ.StockInsight
{
    /// <summary>
    /// Main class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// SET THIS TO THE NAME OF YOUR STREAMINSIGHT INSTANCE, OR BLANK TO AUTODETECT
        /// </summary>
        private const string InstanceName = "Default";

        /// <summary>
        /// Starting point.
        /// </summary>
        public static void Main()
        {
            var instanceName = !string.IsNullOrEmpty(InstanceName) ? InstanceName : StreamInsightSetupInfo.EnumerateInstances()[0];
            Server server;

            try
            {
                server = Server.Create(instanceName);
            }
            catch
            {
                Console.WriteLine("Could not create StreamInsight instance. Please open Program.cs and check InstanceName.");
                return;
            }
            var application = server.CreateApplication("StockInsight");

            // Create a tracer to output information on the console.
            TraceListener tracer = new ConsoleTraceListener();

            // Determine path for historical data
            var dataPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + "\\HistoricalData\\";

            // Configuration for input
            var ericSEKConfig = new StockQuoteInputConfig
            {
                ID = "ERIC-SEK",
                Filename = dataPath + "eric_b_sek_2009.csv",
                ColumnNames = new string[] { "Open", "High", "Low", "Close", "Volume", "Adj Close" },
                StartDate = new DateTime(2009, 01, 01),
                Interval = 0
            };

            // Configuration for input
            var ericUSDConfig = new StockQuoteInputConfig
            {
                ID = "ERIC-USD",
                Filename = dataPath + "eric_b_usd_2009.csv",
                ColumnNames = new string[] { "Open", "High", "Low", "Close", "Volume", "Adj Close" },
                StartDate = new DateTime(2009, 01, 01),
                Interval = 0
            };

            // Configuration for input
            var nokiaUSDConfig = new StockQuoteInputConfig
            {
                ID = "NOKIA-USD",
                Filename = dataPath + "nokia_2009.csv",
                ColumnNames = new string[] { "Open", "High", "Low", "Close", "Volume", "Adj Close" },
                StartDate = new DateTime(2009, 01, 01),
                Interval = 0
            };

            // Configuration for input
            var USDSEKConfig = new StockQuoteInputConfig
            {
                ID = "USD-SEK",
                Filename = dataPath + "USD_SEK_ExchangeRate_2009.csv",
                ColumnNames = new string[] { "Value" },
                StartDate = new DateTime(2009, 01, 01),
                Interval = 0
            };

            // Instantiate input adapters
            var ericSEKStream = CepStream<StockQuote>.Create("ericSEKStream", typeof(StockQuoteInputFactory), ericSEKConfig, EventShape.Point);
            var ericUSDStream = CepStream<StockQuote>.Create("ericUSDStream", typeof(StockQuoteInputFactory), ericUSDConfig, EventShape.Point);
            var nokiaUSDStream = CepStream<StockQuote>.Create("nokiaUSDStream", typeof(StockQuoteInputFactory), nokiaUSDConfig, EventShape.Point);
            var USDSEKStream = CepStream<StockQuote>.Create("USDSEKStream", typeof(StockQuoteInputFactory), USDSEKConfig, EventShape.Point);

            // Run examples
            filterExample(ericUSDStream, application);
            crossJoinProjectionExample(ericSEKStream, ericUSDStream, USDSEKStream, application);
            avgExample(ericUSDStream, application);
            groupApplyExample(ericUSDStream, application);
            bigLooserExample(ericUSDStream, application);
            userFilterExample(ericUSDStream, application);
            standardDeviationExample(ericUSDStream, application);

            application.Delete();
            server.Dispose();
        }

        /// <summary>
        /// Binds a stream to output adapter and runs the query
        /// </summary>
        /// <param name="cepStream"></param>
        private static void runQuery(CepStream<StockQuote> cepStream, Application application)
        {
            // Configure output adapter
            var outputConfig = new StockQuoteOutputConfig();

            // Create query and bind to the output adapter
            var query = cepStream.ToQuery(application, Guid.NewGuid().ToString(), "description", typeof(StockQuoteOutputFactory), outputConfig, EventShape.Point, StreamEventOrder.ChainOrdered);

            // Start query
            query.Start();

            // Wait until query change state
            DiagnosticView diagnosticView;
            do
            {
                Thread.Sleep(100);
                diagnosticView = query.Application.Server.GetDiagnosticView(query.Name);
            } while ((string)diagnosticView[DiagnosticViewProperty.QueryState] == "Running");

            // Stop query
            query.Stop();
        }

        /// <summary>
        /// Example of filter function
        /// </summary>
        /// <param name="cepStream"></param>
        /// <param name="outputConfig"></param>
        /// <param name="adapterStopSignal"></param>
        private static void filterExample(CepStream<StockQuote> cepStream, Application application)
        {
            // Return only "Close" values using a where-clause
            var filteredCepStream = from e in cepStream
                                    where e.FieldID == "Close"
                                    select e;

            runQuery(filteredCepStream, application);
        }

        /// <summary>
        /// Example of cross join and projections
        /// </summary>
        /// <param name="ericSEKStream"></param>
        /// <param name="ericUSDStream"></param>
        /// <param name="USDSEKStream"></param>
        private static void crossJoinProjectionExample(CepStream<StockQuote> ericSEKStream, CepStream<StockQuote> ericUSDStream, CepStream<StockQuote> USDSEKStream, Application application)
        {
            var ericRecalcCepStream = from eUSD in ericUSDStream
                                      from eXch in USDSEKStream
                                      where eUSD.FieldID == "Close"
                                      select new StockQuote()
                                      {
                                          StockID = "ERIC-Recalc",
                                          FieldID = "Close",
                                          Value = eUSD.Value * eXch.Value // Convert ERIC USD quote to SEK
                                      };

            var ericCompareCepStream = from eRecalc in ericRecalcCepStream
                                       from eSEK in ericSEKStream
                                       where eSEK.FieldID == "Close"
                                       select new StockQuote()
                                       {
                                           StockID = "ERIC-Compare",
                                           FieldID = "Diff",
                                           Value = eSEK.Value - eRecalc.Value
                                       };

            runQuery(ericCompareCepStream, application);
        }

        /// <summary>
        /// Example of window function
        /// </summary>
        /// <param name="cepStream"></param>
        /// <param name="outputConfig"></param>
        /// <param name="adapterStopSignal"></param>
        private static void avgExample(CepStream<StockQuote> cepStream, Application application)
        {
            var avgCepStream = from w in cepStream.Where(e => e.FieldID == "Close")
                                                             .HoppingWindow(TimeSpan.FromDays(7), TimeSpan.FromDays(1), HoppingWindowOutputPolicy.ClipToWindowEnd)
                               select new StockQuote()
                               {
                                   StockID = "ERIC",
                                   FieldID = "7-day avg",
                                   Value = w.Avg(e => e.Value)
                               };

            runQuery(avgCepStream, application);
        }

        /// <summary>
        /// Example of a grouping and average calculation on the groups
        /// </summary>
        /// <param name="ericUSDStream"></param>
        /// <param name="outputConfig"></param>
        /// <param name="adapterStopSignal"></param>
        private static void groupApplyExample(CepStream<StockQuote> ericUSDStream, Application application)
        {
            var ericUSDGroupCepStream = from e in ericUSDStream
                                        group e by e.FieldID into eGroup
                                        from w in eGroup.HoppingWindow(TimeSpan.FromDays(7), TimeSpan.FromDays(1), HoppingWindowOutputPolicy.ClipToWindowEnd)
                                        select new StockQuote()
                                        {
                                            StockID = "ERIC 7-day avg",
                                            FieldID = eGroup.Key,
                                            Value = w.Avg(e => e.Value)
                                        };

            runQuery(ericUSDGroupCepStream, application);
        }

        /// <summary>
        /// Example of detecting price drops larger than 10% in 7 days
        /// </summary>
        /// <param name="ericUSDStream"></param>
        /// <param name="outputConfig"></param>
        /// <param name="adapterStopSignal"></param>
        private static void bigLooserExample(CepStream<StockQuote> cepStream, Application application)
        {
            var bigLooserCepStream = (from e1 in cepStream
                                      from e2 in cepStream.ShiftEventTime(e => e.StartTime.AddDays(7))
                                      where e1.FieldID == "Close" && e2.FieldID == "Close"
                                      select new StockQuote()
                                      {
                                          StockID = "ERIC > 10% drop",
                                          FieldID = "Close",
                                          Value = (e1.Value - e2.Value) / e2.Value * 100
                                      }).Where(e => e.Value < -10);


            runQuery(bigLooserCepStream, application);
        }

        /// <summary>
        /// Example of utilising a user defined function for filtering
        /// </summary>
        /// <param name="ericUSDStream"></param>
        /// <param name="outputConfig"></param>
        /// <param name="adapterStopSignal"></param>
        private static void userFilterExample(CepStream<StockQuote> cepStream, Application application)
        {
            var filteredCepStream = from e in cepStream
                                    where UserDefinedFilter(e.FieldID)
                                    select e;

            runQuery(filteredCepStream, application);
        }

        /// <summary>
        /// User defined filter. Notice that it must be declared as public.
        /// </summary>
        /// <param name="fieldID"></param>
        /// <returns></returns>
        public static bool UserDefinedFilter(string fieldID)
        {
            List<string> validFieldIDs = new List<string>(new string[] { "Open", "Close" });
            return validFieldIDs.Contains(fieldID);
        }

        /// <summary>
        /// Example of using a custom aggregation function for standard deviation calculation.
        /// </summary>
        /// <param name="ericUSDStream"></param>
        /// <param name="outputConfig"></param>
        /// <param name="adapterStopSignal"></param>
        private static void standardDeviationExample(CepStream<StockQuote> cepStream, Application application)
        {
            var stddevCepStream = from w in cepStream.Where(e => e.FieldID == "Close")
                                                             .HoppingWindow(TimeSpan.FromDays(7), TimeSpan.FromDays(1), HoppingWindowOutputPolicy.ClipToWindowEnd)
                                  select new StockQuote()
                                  {
                                      StockID = "ERIC",
                                      FieldID = "7-day Stddev",
                                      Value = w.StandardDeviation()
                                  };

            runQuery(stddevCepStream, application);
        }
    }
}
