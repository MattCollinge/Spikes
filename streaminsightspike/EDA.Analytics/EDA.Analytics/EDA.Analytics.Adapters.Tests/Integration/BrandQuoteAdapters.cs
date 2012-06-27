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
using StreamInsight.Samples.Adapters.Wcf;

namespace EDA.Analytics.Adapters.Tests.Integration
{
    [TestFixture]
    public class BrandQuoteAdapters
    {
        ManualResetEvent signal = new ManualResetEvent(false);
          

        [Test]
        public void ShouldPassPointEventThrough()
        {
            //Given
            MassTransitStreamInsightEvent recievedMessage = null;
            MassTransitDomainEvent domainEventTosend = new MassTransitDomainEvent()
                                                   {
                                                       Premium = 123.56m,
                                                       Brand = "brand A",
                                                       EnquiryId = Guid.NewGuid(),
                                                       ProviderQuoteId = Guid.NewGuid(),
                                                       SourceQuoteEngine = "QuoteEngine-Test"
                                                   };

            var producer = SetupMassTransitProducer();
            var consumer = SetupMassTransitConsumer();
            var unsubConsumer = consumer.SubscribeHandler<MassTransitStreamInsightEvent>(
                (m) =>
                {
                    recievedMessage = m;
                    signal.Set();
                }
                );

           new Thread(() => SetupStreamInsight()).Start(); //SetupStreamInsightWCF(); //
           // query.Start();
           //signal.WaitOne(2000);
           
            //When
            //Publish a masstransit domainEvent
            producer.Publish<MassTransitDomainEvent>(domainEventTosend);
            signal.WaitOne(20000);
            //query.Stop();

            //Then
            //Check we get a domainEvent out the other side
            Assert.That(recievedMessage, Is.Not.Null);
            Assert.That(recievedMessage.EnquiryId == domainEventTosend.EnquiryId);
            producer.Dispose();
            unsubConsumer();
            consumer.Dispose();
            signal.Set();
            signal.Dispose();
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

        private void SetupStreamInsight()
        {
            using (var server = Server.Create("HomeStreamInsight"))
            {
                var application = server.CreateApplication("Application");

                // Define a simple query
                CepStream<BrandQuote> input = CepStream<BrandQuote>.Create("Input");
                var streamDefinition = from e in input 
                                       select e;
                //where e.Premium % 10 == 0
                                       
                // Bind query
                var inputConfig = new BrandQuoteInputConfig()
                                      {
                                         ListenToMassTransitOn= new Uri("rabbitmq://localhost/StreamInsightInputAdapter")
                                      };
                var outputConfig = new BrandQuoteOutputConfig()
                {
                                         SendOnMassTransitFrom = new Uri("rabbitmq://localhost/StreamInsightOutputAdapter")
                                      };
               
            Query query = CreateQuery(application, streamDefinition, inputConfig, outputConfig);
           
               
            query.Start();
                
                signal.WaitOne();

                query.Stop();
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

        /// <summary>
        /// Creates and binds a query with a single input stream.
        /// </summary>
        private static Query CreateQueryWCF(Application application, CepStream<EventType> streamDefinition, Uri inputAddress, Uri outputAddress)
        {
            QueryTemplate template = application.CreateQueryTemplate("QueryTemplate", null, streamDefinition);
            QueryBinder binder = new QueryBinder(template);
            InputAdapter inputAdapter = application.CreateInputAdapter<WcfInputAdapterFactory>("InputAdapter", null);
            binder.BindProducer("Input", inputAdapter, inputAddress, EventShape.Point);
            OutputAdapter outputAdapter = application.CreateOutputAdapter<WcfOutputAdapterFactory>("OutputAdapter", null);
            binder.AddConsumer("Output", outputAdapter, outputAddress, EventShape.Point, StreamEventOrder.FullyOrdered);
            Query query = application.CreateQuery("Query", null, binder);
            return query;
        }
    
        private sealed class EventType
        {
            public int X { get; set; }
        }

        private void SetupStreamInsightWCF()
        {

            using (Server server = Server.Create("Default"))
            {
                Application application = server.CreateApplication("Application");

                // Define a simple query
                CepStream<EventType> input = CepStream<EventType>.Create("Input");
                var streamDefinition = from e in input
                                       where e.X % 10 == 0
                                       select e;

                // Bind query
                Uri inputAddress = new Uri("http://localhost:8080/InputAdapter");
                Uri outputAddress = new Uri("http://localhost:8080/OutputAdapter");
                Query query = CreateQueryWCF(application, streamDefinition, inputAddress, outputAddress);

               // return query;

                query.Start();
                Console.WriteLine("Query started.");
                Console.WriteLine("Press enter to stop query.");

               
                Console.ReadLine();

                query.Stop();
                Console.WriteLine("Query stopped.");
            
            }
        }


    }   
}
