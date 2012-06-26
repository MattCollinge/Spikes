//*********************************************************
//
// Copyright Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of
// the License at
//
// http://www.apache.org/licenses/LICENSE-2.0 
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES
// OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES
// OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache 2 License for the specific language
// governing permissions and limitations under the License.
//
//*********************************************************

namespace StreamInsight.Samples.PatternDetector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;
    using StreamInsight.Samples.UserExtensions.Afa;

    /// <summary>
    /// This sample uses the StreamInsight UDSO framework to implement pattern detection. See the
    /// file README.txt for more details.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Our main routine. Creates a random input stream of stock quotes, and performs
        /// pattern detection for every stock symbol in this stream.
        /// </summary>
        /// <param name="args">The command line arguments (unused).</param>
        static void Main(string[] args)
        {
            using (var server = Server.Create("Default"))
            {
                var app = server.CreateApplication("PatternDetector");

                var stocks = CreateRandomInputStream(app, 1000);

                var initialState = new AfaOperator<StockTick, Register, AfaEqualDownticksUpticks>(TimeSpan.FromSeconds(10));

                var query = from s in stocks
                            group s by s.StockSymbol into g
                            from r in g.Scan(initialState)
                            select new { r.Counter, StockSymbol = g.Key };

                var sink = from p in query.ToPointEnumerable()
                           where p.EventKind == EventKind.Insert
                           select new { p.StartTime, p.Payload.Counter, p.Payload.StockSymbol, };

                foreach (var r in sink)
                {
                    Console.WriteLine(r);
                }

                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Generates a stream of stock quotes, with stock prices following a random walk.
        /// </summary>
        /// <param name="application">The application that will host the query.</param>
        /// <param name="c">The number of events to be generated.</param>
        /// <returns>The stream of stock quotes.</returns>
        static CepStream<StockTick> CreateRandomInputStream(Application application, int c)
        {
            Random rand = new Random(100);
            DateTimeOffset startTime = new DateTime(2011, 5, 31);

            // Create input stream using IEnumerable. The stock price follows a random
            // walk, where the price change at each tick is drawn from a uniform
            // random distribution.
            var source = from i in Enumerable.Range(0, c)
                         select PointEvent.CreateInsert(startTime.AddSeconds(i), new StockTick
                         {
                             StockSymbol = CreateStockSymbol(rand),
                             PriceChange = rand.Next(-10, 10)
                         });

            var stockStream = source.ToStream(application, AdvanceTimeSettings.StrictlyIncreasingStartTime);

            return stockStream;
        }

        /// <summary>
        /// Generates a randomly chosen stock symbol.
        /// </summary>
        /// <param name="rand">The random number generator</param>
        /// <returns>The randomly chosen stock symbol.</returns>
        static string CreateStockSymbol(Random rand)
        {
            switch (rand.Next(3))
            {
                case 0: return "MSFT";
                case 1: return "ACME";
                default: return "NWND";
            }
        }
    }
}
