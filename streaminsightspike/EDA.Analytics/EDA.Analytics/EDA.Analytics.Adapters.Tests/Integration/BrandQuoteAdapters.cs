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
           var producer = SetupMassTransitProducer();
            SetupStreamInsight();

            //When
            //Publish a masstransit message

            //Then
            //Check we get a message out the other side
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
                CepStream<EventType> input = CepStream<EventType>.Create("Input");
                var streamDefinition = from e in input
                                       where e.X % 10 == 0
                                       select e;

                // Bind query
                var inputAddress = new Uri("http://localhost:8080/InputAdapter");
                var outputAddress = new Uri("http://localhost:8080/OutputAdapter");
                Query query = CreateQuery(application, streamDefinition, inputAddress, outputAddress);

                query.Start();

                //Send Message

               
                query.Stop();
            }
        }

        private static Query CreateQuery(Application application, CepStream<EventType> streamDefinition, Uri inputAddress, Uri outputAddress)
        {
            var template = application.CreateQueryTemplate("QueryTemplate", null, streamDefinition);
            var binder = new QueryBinder(template);
            var inputAdapter = application.CreateInputAdapter<BrandQuoteInputAdapterFactory>("InputAdapter", null);
            binder.BindProducer("Input", inputAdapter, inputAddress, EventShape.Point);
            var outputAdapter = application.CreateOutputAdapter<BrandQuoteOutputAdapterFactory>("OutputAdapter", null);
            binder.AddConsumer("Output", outputAdapter, outputAddress, EventShape.Point, StreamEventOrder.FullyOrdered);
            var query = application.CreateQuery("Query", null, binder);
            return query;
        }

        private sealed class EventType
        {
            public int X { get; set; }
        }

    }

    public class BrandQuoteOutputAdapter
    {
    }
}
