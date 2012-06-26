// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace AdvantIQ.ExampleAdapters.Input.YahooFinance
{
    /// <summary>
    /// Simple input adapter that reads stock quotes
    /// Future development:
    /// - Check that the stock market is open
    /// - Check date/time of the source
    /// </summary>
    public class YahooFinanceInput : TypedPointInputAdapter<StockQuote>
    {
        public readonly static IFormatProvider QuoteFormatProvider = CultureInfo.InvariantCulture.NumberFormat;
        private PointEvent<StockQuote> pendingEvent;
        private ScreenScraper screenScraper;
        private YahooFinanceConfig _config;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Configuration for this adapter</param>
        public YahooFinanceInput(YahooFinanceConfig config)
        {
            _config = config;
            Patterns tmp = new Patterns(config.StockSymbol);
            screenScraper = new ScreenScraper(tmp.URL,
                config.Timeout, tmp.MatchPattern);
        }

        public override void Start()
        {
            ProduceEvents();
        }

        public override void Resume()
        {
            ProduceEvents();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// Main loop
        /// </summary>
        private void ProduceEvents()
        {
            var currEvent = default(PointEvent<StockQuote>);

            EnqueueCtiEvent(DateTimeOffset.Now);
            try
            {
                // Loop until stop signal
                while (AdapterState != AdapterState.Stopping)
                {
                    if (pendingEvent != null)
                    {
                        currEvent = pendingEvent;
                        pendingEvent = null;
                    }
                    else
                    {
                        try
                        {
                            // Read from source
                            var str = screenScraper.Scrape();
                            var value = double.Parse(str, QuoteFormatProvider);

                            // Produce INSERT event
                            currEvent = CreateInsertEvent();
                            currEvent.StartTime = DateTimeOffset.Now;
                            currEvent.Payload = new StockQuote { StockID = _config.StockName, Value = value };
                            pendingEvent = null;
                            //PrintEvent(currEvent);
                            Enqueue(ref currEvent);

                            // Also send an CTI event
                            EnqueueCtiEvent(DateTimeOffset.Now.AddTicks(2));

                        }
                        catch
                        {
                            // Error handling should go here
                        }
                        Thread.Sleep(_config.Interval);
                    }
                }

                if (pendingEvent != null)
                {
                    currEvent = pendingEvent;
                    pendingEvent = null;
                }

                PrepareToStop(currEvent);
                Stopped();
            }
            catch (AdapterException e)
            {
                Console.WriteLine("AdvantIQ.ExampleAdapters.Input.YahooFinanceTypedPointInput.ProduceEvents - " + e.Message + e.StackTrace);
            }
        }

        private void PrepareToStop(PointEvent<StockQuote> currEvent)
        {
            //EnqueueCtiEvent(DateTime.Now);
            if (currEvent != null)
            {
                // Do this to avoid memory leaks
                ReleaseEvent(ref currEvent);
            }
        }

        private void PrepareToResume(PointEvent<StockQuote> currEvent)
        {
            pendingEvent = currEvent;
        }

        /// <summary>
        /// Debugging function
        /// </summary>
        /// <param name="evt"></param>
        private void PrintEvent(PointEvent<StockQuote> evt)
        {
            Console.WriteLine("Input: " + evt.EventKind + " " +
                evt.StartTime + " " + evt.Payload.StockID + " " +
                evt.Payload.Value);
        }
    }
}
