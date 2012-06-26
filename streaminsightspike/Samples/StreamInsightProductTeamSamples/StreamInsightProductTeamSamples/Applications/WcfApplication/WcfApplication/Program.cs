// *********************************************************
//
//  Copyright (c) Microsoft. All rights reserved.
//  This code is licensed under the Apache 2.0 License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OR
//  CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
//  INCLUDING, WITHOUT LIMITATION, ANY IMPLIED WARRANTIES
//  OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
// *********************************************************

namespace StreamInsight.Samples.WcfApplication
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;
    using StreamInsight.Samples.Adapters.Wcf;

    /// <summary>
    /// Sample application leveraging the WCF sample adapters.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Main entry point for the sample program.
        /// </summary>
        private static void Main()
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
                Query query = CreateQuery(application, streamDefinition, inputAddress, outputAddress);

                query.Start();
                Console.WriteLine("Query started.");
                Console.WriteLine("Press enter to stop query.");

                // Produce and consume events via the service contract channels.
                new Thread(() => ProduceEvents(inputAddress)).Start();
                new Thread(() => ConsumeEvents(outputAddress)).Start();

                Console.ReadLine();

                query.Stop();
                Console.WriteLine("Query stopped.");
            }
        }

        /// <summary>
        /// Enqueues events with increasing 'X' values and with CTIs after every insert.
        /// </summary>
        /// <param name="address">Input adapter service address.</param>
        private static void ProduceEvents(Uri address)
        {
            ClientPointInputAdapter clientAdapter = new ClientPointInputAdapter(address);
            int x = 0;
            DateTimeOffset startTime = DateTimeOffset.UtcNow;

            while (clientAdapter.TryEnqueueCtiEvent(startTime) && 
                clientAdapter.TryEnqueueInsertEvent(startTime, new Dictionary<string, object> { { "X", x } }))
            {
                startTime += TimeSpan.FromTicks(1);
                x++;
            }
        }

        /// <summary>
        /// Consumes events until the output adapter reports there are none remaining.
        /// </summary>
        /// <param name="address">Output adapter service address.</param>
        private static void ConsumeEvents(Uri address)
        {
            ClientPointOutputAdapter clientAdapter = new ClientPointOutputAdapter(address);

            WcfPointEvent result;

            while (clientAdapter.TryDequeueEvent(out result))
            {
                if (null != result.Payload)
                {
                    Console.WriteLine(result.StartTime);
                    foreach (KeyValuePair<string, object> pair in result.Payload)
                    {
                        Console.WriteLine("   {0} = {1}", pair.Key, pair.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Creates and binds a query with a single input stream.
        /// </summary>
        private static Query CreateQuery(Application application, CepStream<EventType> streamDefinition, Uri inputAddress, Uri outputAddress)
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

        /// <summary>
        /// Event type for simple sample query.
        /// </summary>
        private sealed class EventType
        {
            public int X { get; set; }
        }
    }
}
