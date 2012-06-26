using System;
using System.Globalization;
using System.Threading;
using EDA.Analytics.Adapters;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace EDA.Analytics.Adapters
{
    public class BrandQuoteOutputAdapter : TypedPointOutputAdapter<BrandQuote>
    {
        public readonly static IFormatProvider QuoteFormatProvider = CultureInfo.InvariantCulture.NumberFormat;
        private PointEvent<BrandQuote> pendingEvent;
        private BrandQuoteOutputConfig _config;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Configuration for this adapter</param>
        public BrandQuoteOutputAdapter(BrandQuoteOutputConfig config)
        {
            _config = config;
            //Patterns tmp = new Patterns(config.StockSymbol);
            //screenScraper = new ScreenScraper(tmp.URL,
            //    config.Timeout, tmp.MatchPattern);
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
            //var currEvent = default(PointEvent<BrandQuote>);

            //EnqueueCtiEvent(DateTimeOffset.Now);
            //try
            //{
            //    // Loop until stop signal
            //    while (AdapterState != AdapterState.Stopping)
            //    {
            //        if (pendingEvent != null)
            //        {
            //            currEvent = pendingEvent;
            //            pendingEvent = null;
            //        }
            //        else
            //        {
            //            try
            //            {
            //                // Read from MassTransit
            //                //var str = screenScraper.Scrape();
            //                //var value = double.Parse(str, QuoteFormatProvider);

            //                // Produce INSERT event
            //                currEvent = CreateInsertEvent();
            //                currEvent.StartTime = DateTimeOffset.Now;
            //                currEvent.Payload = new BrandQuote() { Brand = massTransitMessage.Brand,
            //                    EnquiryId = massTransitMessage.EnquiryId, 
            //                    Premium = massTransitMessage.Premium,
            //                    ProviderQuoteId = massTransitMessage.ProviderQuoteId,
            //                    SourceQuoteEngine = massTransitMessage.SourceQuoteEngine
            //                };
            //                pendingEvent = null;
            //                //PrintEvent(currEvent);
            //                Enqueue(ref currEvent);

            //                // Also send an CTI event
            //                EnqueueCtiEvent(DateTimeOffset.Now.AddTicks(2));

            //            }
            //            catch
            //            {
            //                // Error handling should go here
            //            }
            //            Thread.Sleep(_config.Interval);
            //        }
            //    }

            //    if (pendingEvent != null)
            //    {
            //        currEvent = pendingEvent;
            //        pendingEvent = null;
            //    }

            //    PrepareToStop(currEvent);
            //    Stopped();
            //}
            //catch (AdapterException e)
            //{
            //    Console.WriteLine("BrandQuoteInputAdapter.ProduceEvents - " + e.Message + e.StackTrace);
            //}
        }

        private void PrepareToStop(PointEvent<BrandQuote> currEvent)
        {
            //EnqueueCtiEvent(DateTime.Now);
            if (currEvent != null)
            {
                // Do this to avoid memory leaks
                ReleaseEvent(ref currEvent);
            }
        }

        private void PrepareToResume(PointEvent<BrandQuote> currEvent)
        {
            pendingEvent = currEvent;
        }

       
    }

  
}
