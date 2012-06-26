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
using Microsoft.ComplexEventProcessing.Diagnostics;
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
            // Create the server and application
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

            // Configure output adapter
            var outputConfig = new StockQuoteOutputConfig();

            // Instantiate semaphor for stop signal
            var adapterStopSignal = new EventWaitHandle(false,
                EventResetMode.AutoReset, outputConfig.AdapterStopSignal);

            // Add input and output adapter factories to the server
            var inputAdapter = application.CreateInputAdapter<StockQuoteInputFactory>("StockQuoteInput", "Description...");
            var outputAdapter = application.CreateOutputAdapter<StockQuoteOutputFactory>("StockQuoteOutput", "Description...");

            // Create queries
            createFilterExampleQuery(application, ericUSDConfig, outputConfig, inputAdapter, outputAdapter);
            createCrossJoinExampleQuery(application, ericUSDConfig, ericSEKConfig, USDSEKConfig, outputConfig, inputAdapter, outputAdapter);
            createAvgExampleQuery(application, ericUSDConfig, outputConfig, inputAdapter, outputAdapter);
            createGroupApplyExampleQuery(application, ericUSDConfig, outputConfig, inputAdapter, outputAdapter);
            createBigLooserExampleQuery(application, ericUSDConfig, outputConfig, inputAdapter, outputAdapter);
            createUserFilterExampleQuery(application, ericUSDConfig, outputConfig, inputAdapter, outputAdapter);
            createStandardDeviationExampleQuery(application, ericUSDConfig, outputConfig, inputAdapter, outputAdapter);

            // Execute the queries (one at a time)
            // It would also be possible to execute them in parallell
            foreach (var query in application.Queries.Values)
            {
                Console.WriteLine("\r\nQuery: " + query.Name);

                // Start
                query.Start();

                // Wait until output adapter signals that it is finished
                DiagnosticView diagnosticView;
                do
                {
                    Thread.Sleep(100);
                    diagnosticView = query.Application.Server.GetDiagnosticView(query.Name);
                } while ((string)diagnosticView["QueryState"] == "Running");

                // Stop
                query.Stop();
            }

            // Release resources
            application.Delete();
            server.Dispose();
            //application.Dispose();
        }

        /// <summary>
        /// Create a simple filtering example
        /// </summary>
        /// <param name="application"></param>
        /// <param name="inputConfig"></param>
        /// <param name="outputConfig"></param>
        /// <param name="inputAdapter"></param>
        /// <param name="outputAdapter"></param>
        /// <returns></returns>
        private static Query createFilterExampleQuery(Application application, StockQuoteInputConfig inputConfig, StockQuoteOutputConfig outputConfig, InputAdapter inputAdapter, OutputAdapter outputAdapter)
        {
            var input = CepStream<StockQuote>.Create("input");
            var filteredCepStream = from e in input
                                    where e.FieldID == "Close"
                                    select e;
            
            var queryTemplate = application.CreateQueryTemplate("filterExampleTemplate", "Description...", filteredCepStream);

            var queryBinder = new QueryBinder(queryTemplate);
            queryBinder.BindProducer<StockQuote>("input", inputAdapter, inputConfig, EventShape.Point);
            queryBinder.AddConsumer<StockQuote>("output", outputAdapter, outputConfig, EventShape.Point, StreamEventOrder.ChainOrdered);

            var query = application.CreateQuery("filterExampleQuery", "Description...", queryBinder);
            return query;
        }

        /// <summary>
        /// Compare USD and SEK stock prices using given exchange rate. Example of a cross join.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="ericUSDConfig"></param>
        /// <param name="ericSEKConfig"></param>
        /// <param name="USDSEKConfig"></param>
        /// <param name="outputConfig"></param>
        /// <param name="inputAdapter"></param>
        /// <param name="outputAdapter"></param>
        /// <returns></returns>
        private static Query createCrossJoinExampleQuery(Application application, StockQuoteInputConfig ericUSDConfig, StockQuoteInputConfig ericSEKConfig, StockQuoteInputConfig USDSEKConfig, StockQuoteOutputConfig outputConfig, InputAdapter inputAdapter, OutputAdapter outputAdapter)
        {
            var ericUSDStream = CepStream<StockQuote>.Create("ericUSDStream");
            var ericSEKStream = CepStream<StockQuote>.Create("ericSEKStream");
            var USDSEKStream = CepStream<StockQuote>.Create("USDSEKStream");
            var ericRecalcCepStream = from eUSD in ericUSDStream
                                      from eXch in USDSEKStream // Cross join
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
            var queryTemplate = application.CreateQueryTemplate("ericCompareTemplate", "Description...", ericCompareCepStream);

            var queryBinder = new QueryBinder(queryTemplate);
            queryBinder.BindProducer<StockQuote>("ericUSDStream", inputAdapter, ericUSDConfig, EventShape.Point);
            queryBinder.BindProducer<StockQuote>("ericSEKStream", inputAdapter, ericSEKConfig, EventShape.Point);
            queryBinder.BindProducer<StockQuote>("USDSEKStream", inputAdapter, USDSEKConfig, EventShape.Point);
            queryBinder.AddConsumer<StockQuote>("output", outputAdapter, outputConfig, EventShape.Point, StreamEventOrder.ChainOrdered);

            var query = application.CreateQuery("ericCompareQuery", "Description...", queryBinder);
            return query;
        }

        /// <summary>
        /// Example of using a window aggregation function
        /// </summary>
        /// <param name="application"></param>
        /// <param name="inputConfig"></param>
        /// <param name="outputConfig"></param>
        /// <param name="inputAdapter"></param>
        /// <param name="outputAdapter"></param>
        /// <returns></returns>
        private static Query createAvgExampleQuery(Application application, StockQuoteInputConfig inputConfig, StockQuoteOutputConfig outputConfig, InputAdapter inputAdapter, OutputAdapter outputAdapter)
        {
            var input = CepStream<StockQuote>.Create("input");
            var avgCepStream = from w in input.Where(e => e.FieldID == "Close")
                                              .HoppingWindow(TimeSpan.FromDays(7), TimeSpan.FromDays(1), HoppingWindowOutputPolicy.ClipToWindowEnd)
                               select new StockQuote()
                               {
                                   StockID = "ERIC",
                                   FieldID = "7-day avg",
                                   Value = w.Avg(e => e.Value)
                               };
            var queryTemplate = application.CreateQueryTemplate("avgExampleTemplate", "Description...", avgCepStream);

            var queryBinder = new QueryBinder(queryTemplate);
            queryBinder.BindProducer<StockQuote>("input", inputAdapter, inputConfig, EventShape.Point);
            queryBinder.AddConsumer<StockQuote>("output", outputAdapter, outputConfig, EventShape.Point, StreamEventOrder.ChainOrdered);

            var query = application.CreateQuery("avgExampleQuery", "Description...", queryBinder);
            return query;
        }

        /// <summary>
        /// Example of a grouping and calculation of averages for the groups
        /// </summary>
        /// <param name="application"></param>
        /// <param name="inputConfig"></param>
        /// <param name="outputConfig"></param>
        /// <param name="inputAdapter"></param>
        /// <param name="outputAdapter"></param>
        /// <returns></returns>
        private static Query createGroupApplyExampleQuery(Application application, StockQuoteInputConfig inputConfig, StockQuoteOutputConfig outputConfig, InputAdapter inputAdapter, OutputAdapter outputAdapter)
        {
            var input = CepStream<StockQuote>.Create("input");
            var ericUSDGroupCepStream = from e in input
                                        group e by e.FieldID into eGroup
                                        from w in eGroup.HoppingWindow(TimeSpan.FromDays(7), TimeSpan.FromDays(1), HoppingWindowOutputPolicy.ClipToWindowEnd)
                                        select new StockQuote()
                                        {
                                            StockID = "ERIC 7-day avg",
                                            FieldID = eGroup.Key,
                                            Value = w.Avg(e => e.Value)
                                        };
            var queryTemplate = application.CreateQueryTemplate("groupApplyExampleTemplate", "Description...", ericUSDGroupCepStream);

            var queryBinder = new QueryBinder(queryTemplate);
            queryBinder.BindProducer<StockQuote>("input", inputAdapter, inputConfig, EventShape.Point);
            queryBinder.AddConsumer<StockQuote>("output", outputAdapter, outputConfig, EventShape.Point, StreamEventOrder.ChainOrdered);

            var query = application.CreateQuery("groupApplyExampleQuery", "Description...", queryBinder);
            return query;
        }

        /// <summary>
        /// Example of detecting when stock price falls more than 10% in 7 days
        /// </summary>
        /// <param name="application"></param>
        /// <param name="inputConfig"></param>
        /// <param name="outputConfig"></param>
        /// <param name="inputAdapter"></param>
        /// <param name="outputAdapter"></param>
        /// <returns></returns>
        private static Query createBigLooserExampleQuery(Application application, StockQuoteInputConfig inputConfig, StockQuoteOutputConfig outputConfig, InputAdapter inputAdapter, OutputAdapter outputAdapter)
        {
            var input = CepStream<StockQuote>.Create("input");
            var bigLooserCepStream = (from e1 in input
                                      from e2 in input.ShiftEventTime(e => e.StartTime.AddDays(7))
                                      where e1.FieldID == "Close" && e2.FieldID == "Close"
                                      select new StockQuote()
                                      {
                                          StockID = "ERIC > 10% drop",
                                          FieldID = "Close",
                                          Value = (e1.Value - e2.Value) / e2.Value * 100
                                      }).Where(e => e.Value < -10);
            var queryTemplate = application.CreateQueryTemplate("bigLooserExampleTemplate", "Description...", bigLooserCepStream);

            var queryBinder = new QueryBinder(queryTemplate);
            queryBinder.BindProducer<StockQuote>("input", inputAdapter, inputConfig, EventShape.Point);
            queryBinder.AddConsumer<StockQuote>("output", outputAdapter, outputConfig, EventShape.Point, StreamEventOrder.ChainOrdered);

            var query = application.CreateQuery("bigLooserExampleQuery", "Description...", queryBinder);
            return query;
        }

        /// <summary>
        /// Example of using a user-defined filter
        /// </summary>
        /// <param name="application"></param>
        /// <param name="inputConfig"></param>
        /// <param name="outputConfig"></param>
        /// <param name="inputAdapter"></param>
        /// <param name="outputAdapter"></param>
        /// <returns></returns>
        private static Query createUserFilterExampleQuery(Application application, StockQuoteInputConfig inputConfig, StockQuoteOutputConfig outputConfig, InputAdapter inputAdapter, OutputAdapter outputAdapter)
        {
            var input = CepStream<StockQuote>.Create("input");
            var filteredCepStream = from e in input
                                    where UserDefinedFilter(e.FieldID)
                                    select e;
            var queryTemplate = application.CreateQueryTemplate("userFilterExampleTemplate", "Description...", filteredCepStream);

            var queryBinder = new QueryBinder(queryTemplate);
            queryBinder.BindProducer<StockQuote>("input", inputAdapter, inputConfig, EventShape.Point);
            queryBinder.AddConsumer<StockQuote>("output", outputAdapter, outputConfig, EventShape.Point, StreamEventOrder.ChainOrdered);

            var query = application.CreateQuery("userFilterExampleQuery", "Description...", queryBinder);
            return query;
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
        /// Example of using a user-defined aggregate
        /// </summary>
        /// <param name="application"></param>
        /// <param name="inputConfig"></param>
        /// <param name="outputConfig"></param>
        /// <param name="inputAdapter"></param>
        /// <param name="outputAdapter"></param>
        /// <returns></returns>
        private static Query createStandardDeviationExampleQuery(Application application, StockQuoteInputConfig inputConfig, StockQuoteOutputConfig outputConfig, InputAdapter inputAdapter, OutputAdapter outputAdapter)
        {
            var input = CepStream<StockQuote>.Create("input");
            var stddevCepStream = from w in input.Where(e => e.FieldID == "Close")
                                                             .HoppingWindow(TimeSpan.FromDays(7), TimeSpan.FromDays(1), HoppingWindowOutputPolicy.ClipToWindowEnd)
                                  select new StockQuote()
                                  {
                                      StockID = "ERIC",
                                      FieldID = "7-day Stddev",
                                      Value = w.StandardDeviation()
                                  };
            var queryTemplate = application.CreateQueryTemplate("standardDeviationExampleTemplate", "Description...", stddevCepStream);

            var queryBinder = new QueryBinder(queryTemplate);
            queryBinder.BindProducer<StockQuote>("input", inputAdapter, inputConfig, EventShape.Point);
            queryBinder.AddConsumer<StockQuote>("output", outputAdapter, outputConfig, EventShape.Point, StreamEventOrder.ChainOrdered);

            var query = application.CreateQuery("standardDeviationExampleQuery", "Description...", queryBinder);
            return query;
        }
    }
}
