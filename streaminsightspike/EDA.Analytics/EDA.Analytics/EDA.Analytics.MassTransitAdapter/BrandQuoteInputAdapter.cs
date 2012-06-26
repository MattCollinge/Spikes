using System;
using System.Globalization;
using System.Threading;
using EDA.Analytics.Adapters;
using MassTransit;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace EDA.Analytics.Adapters
{
    public class BrandQuoteInputAdapter : TypedPointInputAdapter<BrandQuote>
    {
        public readonly static IFormatProvider QuoteFormatProvider = CultureInfo.InvariantCulture.NumberFormat;
        private PointEvent<BrandQuote> _pendingEvent;
        private readonly BrandQuoteInputConfig _config;
        private readonly IServiceBus _bus;
        private UnsubscribeAction _usubscribeAction = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Configuration for this adapter</param>
        public BrandQuoteInputAdapter(BrandQuoteInputConfig config)
        {
            _config = config;
        
            _bus = ServiceBusFactory.New(sbc =>
            {
                sbc.UseRabbitMqRouting();
                sbc.ReceiveFrom(_config.ListenToMassTransitOn);
            });

             }


        public override void Start()
        {
           _usubscribeAction = _bus.SubscribeHandler<MassTransitMessage>((m) => ProduceEvents(m));
        }

        public override void Resume()
        {
            _usubscribeAction = _bus.SubscribeHandler<MassTransitMessage> ((m) => ProduceEvents(m));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// Main loop
        /// </summary>
        private void ProduceEvents(MassTransitMessage message)
        {
            var currEvent = default(PointEvent<BrandQuote>);

            EnqueueCtiEvent(DateTimeOffset.Now);
            try
            {
                //  until stop signal
                if (AdapterState != AdapterState.Stopping)
                {
                    if (_pendingEvent != null)
                    {
                        currEvent = _pendingEvent;
                        _pendingEvent = null;
                    }
                    else
                    {
                        try
                        {
                            // Read from MassTransit

                            // Produce INSERT event
                            currEvent = CreateInsertEvent();
                            currEvent.StartTime = DateTimeOffset.Now;
                            currEvent.Payload = new BrandQuote()
                                                    {
                                                        Brand = message.Brand,
                                                        EnquiryId = message.EnquiryId,
                                                        Premium = message.Premium,
                                                        ProviderQuoteId = message.ProviderQuoteId,
                                                        SourceQuoteEngine = message.SourceQuoteEngine
                                                    };
                            _pendingEvent = null;
                            //PrintEvent(currEvent);
                            Enqueue(ref currEvent);

                            // Also send an CTI event
                            EnqueueCtiEvent(DateTimeOffset.Now.AddTicks(2));

                        }
                        finally
                        {

                        }
                    }
                    PrepareToStop(currEvent);
                    Stopped();

                }

                if (_pendingEvent != null)
                {
                    currEvent = _pendingEvent;
                    _pendingEvent = null;
                }
            }
            catch (AdapterException e)
            {
                Console.WriteLine("BrandQuoteInputAdapter.ProduceEvents - " + e.Message + e.StackTrace);
            }
        }

        private void PrepareToStop(PointEvent<BrandQuote> currEvent)
        {
            if (_usubscribeAction != null) _usubscribeAction();

            if (currEvent != null)
            {
                // Do this to avoid memory leaks
                ReleaseEvent(ref currEvent);
            }
        }

        private void PrepareToResume(PointEvent<BrandQuote> currEvent)
        {
            _pendingEvent = currEvent;
        }

       
    }

    public class MassTransitMessage
    {
        public string Brand;
        public Guid EnquiryId;
        public decimal Premium;
        public Guid ProviderQuoteId;
        public string SourceQuoteEngine;
    }

    public struct BrandQuote
    {
        public decimal Premium;
        public string Brand;
        public string SourceQuoteEngine;
        public Guid EnquiryId;
        public Guid ProviderQuoteId;
    }
}
