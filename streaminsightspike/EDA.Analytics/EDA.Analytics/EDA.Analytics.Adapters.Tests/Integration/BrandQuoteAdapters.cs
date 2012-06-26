using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using EDA.Analytics.MassTransitAdapter;
using MassTransit;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;
using NUnit.Framework;

namespace EDA.Analytics.Adapters.Tests.Integration
{
    [TestFixture]
    public class BrandQuoteAdapters
    {

        [Test]
        public void ShouldPassPointEventThrough()
        {
            //Given
            MassTransitMessage recievedMessage = null;
            ManualResetEvent signal = new ManualResetEvent(false);
            MassTransitMessage messageTosend = new MassTransitMessage()
                                                   {
                                                       Premium = 123.56m,
                                                       Brand = "brand A",
                                                       EnquiryId = Guid.NewGuid(),
                                                       ProviderQuoteId = Guid.NewGuid(),
                                                       SourceQuoteEngine = "QuoteEngine-Test"
                                                   };

            var producer = SetupMassTransitProducer();
            var consumer = SetupMassTransitConsumer();
            consumer.SubscribeHandler<MassTransitMessage>(
                (m) =>
                    {
                        recievedMessage = m;
                        signal.Set();
                    }
                );

            var query = SetupStreamInsight();
            query.Start();

            //When
            //Publish a masstransit message
            producer.Publish<MassTransitMessage>(messageTosend);
            signal.WaitOne(1000);
            query.Stop();

            //Then
            //Check we get a message out the other side
            Assert.That(recievedMessage, Is.Not.Null);
            Assert.That(recievedMessage.EnquiryId == messageTosend.EnquiryId);

        }

        private IServiceBus SetupMassTransitProducer()
        {
            var receiveFromAddress = new Uri("rabbitmq://localhost/StreamInsightTestSource");
            return ServiceBusFactory.New(sbc =>
                                             {
                                                 sbc.UseRabbitMqRouting();
                                                 sbc.ReceiveFrom(receiveFromAddress);
                                             });
        }

        private IServiceBus SetupMassTransitConsumer()
        {
            var receiveFromAddress = new Uri("rabbitmq://localhost/StreamInsightTestSink");
            return ServiceBusFactory.New(sbc =>
            {
                sbc.UseRabbitMqRouting();
                sbc.ReceiveFrom(receiveFromAddress);
            });
        }

        private Query SetupStreamInsight()
        {
            using (var server = Server.Create("HomeStreamInsight"))
            {
                var application = server.CreateApplication("Application");

                // Define a simple query
                CepStream<BrandQuote> input = CepStream<BrandQuote>.Create("Input");
                var streamDefinition = from e in input
                                       //where e.Premium % 10 == 0
                                       select e;

                // Bind query
                var inputConfig = new BrandQuoteInputConfig()
                                      {
                                         ListenToMassTransitOn= new Uri("rabbitmq://localhost/StreamInsightInputAdapter")
                                      };
                var outputConfig = new BrandQuoteOutputConfig()
                {
                                         SendOnMassTransitFrom = new Uri("rabbitmq://localhost/StreamInsightOutputAdapter")
                                      };
               
            return CreateQuery(application, streamDefinition, inputConfig, outputConfig); 
            }
        }

        private static Query CreateQuery(Application application, CepStream<BrandQuote> streamDefinition, BrandQuoteInputConfig inputConfig, BrandQuoteOutputConfig outputConfig)
        {
            var template = application.CreateQueryTemplate("QueryTemplate", null, streamDefinition);
            var binder = new QueryBinder(template);
            var inputAdapter = application.CreateInputAdapter<BrandQuoteInputAdapterFactory>("InputAdapter", null);
            binder.BindProducer("Input", inputAdapter, inputConfig, EventShape.Point);
            var outputAdapter = application.CreateOutputAdapter<BrandQuoteOutputAdapterFactory>("OutputAdapter", null);
            binder.AddConsumer("Output", outputAdapter, outputConfig, EventShape.Point, StreamEventOrder.FullyOrdered);
            var query = application.CreateQuery("Query", null, binder);
            return query;
        }
    }   
}
