// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Extensibility;
using Microsoft.ComplexEventProcessing.Linq;

namespace AdvantIQ.StockInsight
{
    public class Program
    {
        /// <summary>
        /// SET THIS TO THE NAME OF YOUR STREAMINSIGHT INSTANCE, OR BLANK TO AUTODETECT
        /// </summary>
        private const string InstanceName = "Default";

        static void Main(string[] args)
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

            // Determine path for historical data
            var dataPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + "\\HistoricalData\\";

            var ericUSDObservable = Enumerable.GetStockObservable(dataPath + "eric_b_usd_2009.csv",
                                "ERIC-USD", 
                                new string[] { "Open", "High", "Low", "Close", "Volume", "Adj Close" }
                             );

            var ericSEKObservable = Enumerable.GetStockObservable(dataPath + "eric_b_sek_2009.csv",
                                "ERIC-SEK",
                                new string[] { "Open", "High", "Low", "Close", "Volume", "Adj Close" }
                             );

            var nokiaUSDObservable = Enumerable.GetStockObservable(dataPath + "nokia_2009.csv",
                                "NOKIA-USD",
                                new string[] { "Open", "High", "Low", "Close", "Volume", "Adj Close" }
                             );

            var USDSEKObservable = Enumerable.GetStockObservable(dataPath + "USD_SEK_ExchangeRate_2009.csv",
                                "USD-SEK",
                                new string[] { "Value" }
                             );

            // Run examples
            filterExample(ericUSDObservable, application);
            crossJoinProjectionExample(ericSEKObservable, ericUSDObservable, USDSEKObservable, application);
            avgExample(ericUSDObservable, application);
            groupApplyExample(ericUSDObservable, application);
            bigLooserExample(ericUSDObservable, application);
            userFilterExample(ericUSDObservable, application);
            standardDeviationExample(ericUSDObservable, application);

            application.Delete();
            server.Dispose();
        }

        private static void runQuery(CepStream<StockQuote> cepStream)
        {
            var observer = new Observer();

            // Create waithandle to signal when query is finished
            var adapterStopSignal = new EventWaitHandle(false, EventResetMode.AutoReset, "StockInsightSignal");

            // This starts the query
            var disposable = cepStream.ToObservable<StockQuote>().Subscribe(observer);

            // Wait for query to finish
            adapterStopSignal.WaitOne();

            // Write a blank line
            Console.WriteLine();

            // This line has been commented out because it hangs sometimes
            disposable.Dispose();
        }

        /// <summary>
        /// Example of filter function
        /// </summary>
        /// <param name="cepStream"></param>
        /// <param name="outputConfig"></param>
        /// <param name="adapterStopSignal"></param>
        private static void filterExample(IEnumerable<StockQuote> observable, Application application)
        {
            // Convert observable to CEP Stream for using in the query
            var cepStream = observable.ToPointStream(application, e => PointEvent<StockQuote>.CreateInsert(new DateTimeOffset(e.TimeStamp, TimeSpan.Zero), e), AdvanceTimeSettings.IncreasingStartTime);

            // Return only "Close" values using a where-clause
            var filteredCepStream = from e in cepStream
                                    where e.FieldID == "Close"
                                    select e;

            runQuery(filteredCepStream);
        }

        /// <summary>
        /// Example of cross join and projections
        /// </summary>
        /// <param name="ericSEKStream"></param>
        /// <param name="ericUSDStream"></param>
        /// <param name="USDSEKStream"></param>
        private static void crossJoinProjectionExample(IEnumerable<StockQuote> ericSEKObservable, IEnumerable<StockQuote> ericUSDObservable, IEnumerable<StockQuote> USDSEKObservable, Application application)
        {
            var ericUSDStream = ericUSDObservable.ToPointStream(application, e => PointEvent<StockQuote>.CreateInsert(new DateTimeOffset(e.TimeStamp, TimeSpan.Zero), e), AdvanceTimeSettings.IncreasingStartTime);
            var ericSEKStream = ericSEKObservable.ToPointStream(application, e => PointEvent<StockQuote>.CreateInsert(new DateTimeOffset(e.TimeStamp, TimeSpan.Zero), e), AdvanceTimeSettings.IncreasingStartTime);
            var USDSEKStream = USDSEKObservable.ToPointStream(application, e => PointEvent<StockQuote>.CreateInsert(new DateTimeOffset(e.TimeStamp, TimeSpan.Zero), e), AdvanceTimeSettings.IncreasingStartTime);

            var ericRecalcCepStream = from eUSD in ericUSDStream
                                      from eXch in USDSEKStream
                                      where eUSD.FieldID == "Close"
                                      select new StockQuote()
                                      {
                                          StockID = "ERIC-Recalc",
                                          FieldID = "Close",
                                          Value = eUSD.Value * eXch.Value, // Convert ERIC USD quote to SEK
                                          TimeStamp = eUSD.TimeStamp

                                      };

            var ericCompareCepStream = from eRecalc in ericRecalcCepStream
                                       from eSEK in ericSEKStream
                                       where eSEK.FieldID == "Close"
                                       select new StockQuote()
                                       {
                                           StockID = "ERIC-Compare",
                                           FieldID = "Diff",
                                           Value = eSEK.Value - eRecalc.Value,
                                           TimeStamp = eRecalc.TimeStamp
                                       };

            runQuery(ericCompareCepStream);
        }

        /// <summary>
        /// Example of window function
        /// </summary>
        /// <param name="cepStream"></param>
        /// <param name="outputConfig"></param>
        /// <param name="adapterStopSignal"></param>
        private static void avgExample(IEnumerable<StockQuote> observable, Application application)
        {
            var cepStream = observable.ToPointStream(application, e => PointEvent<StockQuote>.CreateInsert(new DateTimeOffset(e.TimeStamp, TimeSpan.Zero), e), AdvanceTimeSettings.IncreasingStartTime);

            var avgCepStream = from w in cepStream.Where(e => e.FieldID == "Close")
                                                             .HoppingWindow(TimeSpan.FromDays(7), TimeSpan.FromDays(1), HoppingWindowOutputPolicy.ClipToWindowEnd)
                               select new StockQuote()
                               {
                                   StockID = "ERIC",
                                   FieldID = "7-day avg",
                                   Value = w.Avg(e => e.Value),
                                   TimeStamp = w.Min(e => e.TimeStamp)
                               };

            runQuery(avgCepStream);
        }

        /// <summary>
        /// Example of a grouping and average calculation on the groups
        /// </summary>
        /// <param name="ericUSDStream"></param>
        /// <param name="outputConfig"></param>
        /// <param name="adapterStopSignal"></param>
        private static void groupApplyExample(IEnumerable<StockQuote> observable, Application application)
        {
            var cepStream = observable.ToPointStream(application, e => PointEvent<StockQuote>.CreateInsert(new DateTimeOffset(e.TimeStamp, TimeSpan.Zero), e), AdvanceTimeSettings.IncreasingStartTime);

            var ericUSDGroupCepStream = from e in cepStream
                                        group e by e.FieldID into eGroup
                                        from w in eGroup.HoppingWindow(TimeSpan.FromDays(7), TimeSpan.FromDays(1), HoppingWindowOutputPolicy.ClipToWindowEnd)
                                        select new StockQuote()
                                        {
                                            StockID = "ERIC 7-day avg",
                                            FieldID = eGroup.Key,
                                            Value = w.Avg(e => e.Value),
                                            TimeStamp = w.Min(e => e.TimeStamp)
                                        };

            runQuery(ericUSDGroupCepStream);
        }

        /// <summary>
        /// Example of detecting price drops larger than 10% in 7 days
        /// </summary>
        /// <param name="ericUSDStream"></param>
        /// <param name="outputConfig"></param>
        /// <param name="adapterStopSignal"></param>
        private static void bigLooserExample(IEnumerable<StockQuote> observable, Application application)
        {
            var cepStream = observable.ToPointStream(application, e => PointEvent<StockQuote>.CreateInsert(new DateTimeOffset(e.TimeStamp, TimeSpan.Zero), e), AdvanceTimeSettings.IncreasingStartTime);

            var bigLooserCepStream = (from e1 in cepStream
                                      from e2 in cepStream.ShiftEventTime(e => e.StartTime.AddDays(7))
                                      where e1.FieldID == "Close" && e2.FieldID == "Close"
                                      select new StockQuote()
                                      {
                                          StockID = "ERIC > 10% drop",
                                          FieldID = "Close",
                                          Value = (e1.Value - e2.Value) / e2.Value * 100,
                                          TimeStamp = e1.TimeStamp
                                      }).Where(e => e.Value < -10);


            runQuery(bigLooserCepStream);
        }

        /// <summary>
        /// Example of utilising a user defined function for filtering
        /// </summary>
        /// <param name="ericUSDStream"></param>
        /// <param name="outputConfig"></param>
        /// <param name="adapterStopSignal"></param>
        private static void userFilterExample(IEnumerable<StockQuote> observable, Application application)
        {
            var cepStream = observable.ToPointStream(application, e => PointEvent<StockQuote>.CreateInsert(new DateTimeOffset(e.TimeStamp, TimeSpan.Zero), e), AdvanceTimeSettings.IncreasingStartTime);

            var filteredCepStream = from e in cepStream
                                    where UserDefinedFilter(e.FieldID)
                                    select e;

            runQuery(filteredCepStream);
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
        private static void standardDeviationExample(IEnumerable<StockQuote> observable, Application application)
        {
            var cepStream = observable.ToPointStream(application, e => PointEvent<StockQuote>.CreateInsert(new DateTimeOffset(e.TimeStamp, TimeSpan.Zero), e), AdvanceTimeSettings.IncreasingStartTime);

            var stddevCepStream = from w in cepStream.Where(e => e.FieldID == "Close")
                                                             .HoppingWindow(TimeSpan.FromDays(7), TimeSpan.FromDays(1), HoppingWindowOutputPolicy.ClipToWindowEnd)
                                  select new StockQuote()
                                  {
                                      StockID = "ERIC",
                                      FieldID = "7-day Stddev",
                                      Value = w.StandardDeviation(),
                                      TimeStamp = w.Min(e => e.TimeStamp)
                                  };

            runQuery(stddevCepStream);
        }
    }
}
