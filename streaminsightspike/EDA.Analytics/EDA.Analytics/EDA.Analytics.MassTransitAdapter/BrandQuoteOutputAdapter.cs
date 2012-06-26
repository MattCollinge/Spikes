using System;
using System.Globalization;
using System.Threading;
using EDA.Analytics.Adapters;
using MassTransit;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace EDA.Analytics.Adapters
{
    public class BrandQuoteOutputAdapter : TypedPointOutputAdapter<BrandQuote>
    {
        public readonly static IFormatProvider QuoteFormatProvider = CultureInfo.InvariantCulture.NumberFormat;
        private PointEvent<BrandQuote> currentEvent;
        private BrandQuoteOutputConfig _config;
        private IServiceBus _bus;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Configuration for this adapter</param>
        public BrandQuoteOutputAdapter(BrandQuoteOutputConfig config)
        {
            _config = config;
         
            _bus = ServiceBusFactory.New(sbc =>
            {
                sbc.UseRabbitMqRouting();
                sbc.ReceiveFrom(_config.SendOnMassTransitFrom);
            });
        }

        public override void Start()
        {
            ConsumeEvents();
        }

        public override void Resume()
        {
            ConsumeEvents();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public void ConsumeEvents()
        {
            PointEvent<BrandQuote> currentEvent = default(PointEvent<BrandQuote>);

            // if the engine is attempting to stop the adapter,  make one more attempt to dequeue the last event from the engine,
            // clean up state and exit worker thread
            if (AdapterState == AdapterState.Stopping)
            {
                this.PrepareToStop();
                this.Stopped();
                return;
            }

            // NOTE: at any point in time during execution of the code below, if the Adapter
            // state changes to Stopping, the engine will resume the adapter (i.e. call Resume())
            // just one more time, and the stopping condition will be trapped at the check above.

            // Dequeue the event
            // if the engine does not have any events, it puts the adapter in a Suspended state; or it
            // could also be putting the adapter in a Stopping state. If it is in the suspended state,
            // you can optionally invoke a method here for housekeeping. Signal to the engine that the
            // adapter is ready to be resumed, and exit the worker thread
            if (DequeueOperationResult.Empty == Dequeue(out currentEvent))
            {
                Ready();
                return;
            }

            try
            {

                //Create Message for MassTransit and publish
                var message = new MassTransitMessage()
                                  {
                                      Brand = currentEvent.Payload.Brand,
                                      EnquiryId = currentEvent.Payload.EnquiryId,
                                      ProviderQuoteId = currentEvent.Payload.ProviderQuoteId,
                                      Premium = currentEvent.Payload.Premium,
                                      SourceQuoteEngine = currentEvent.Payload.SourceQuoteEngine
                                  };
                _bus.Publish<MassTransitMessage>(message);
            }
            finally
            {
                ReleaseEvent(ref currentEvent);
            }
        }

        private void PrepareToStop()
        {
           //Dispose of MassTransit bus
            _bus.Dispose();
        }
    }

  
}
