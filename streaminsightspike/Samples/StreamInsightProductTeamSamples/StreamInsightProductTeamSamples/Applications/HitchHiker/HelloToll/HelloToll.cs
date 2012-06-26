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

namespace StreamInsight.Samples.HelloToll
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Extensibility;
    using Microsoft.ComplexEventProcessing.Linq;
    using StreamInsight.Samples.Adapters.AsyncCsv;

    internal class HelloToll
    {
        /// <summary>
        /// Simulation of a reference database; for the user defined function to search against this.
        /// In reality, this could be a database, or an in-memory cache, or another input stream.
        /// </summary>
        private static List<TagInfo> tagList = new List<TagInfo>
        {
           new TagInfo { TagId = "123456789", RenewalDate = DateTime.Parse("2/20/2009", new CultureInfo("en-US")), IsReportedLostOrStolen = false, AccountId = "NJ100001JET1109" },
           new TagInfo { TagId = "234567891", RenewalDate = DateTime.Parse("12/6/2008", new CultureInfo("en-US")), IsReportedLostOrStolen = true, AccountId = "NY100002GNT0109" },
           new TagInfo { TagId = "345678912", RenewalDate = DateTime.Parse("9/1/2008", new CultureInfo("en-US")), IsReportedLostOrStolen = true, AccountId = "CT100003YNK0210" }
        };

        public static bool IsLostOrStolen(string tagId)
        {
            foreach (var tag in tagList)
            {
                if (tag.TagId.Equals(tagId) && tag.IsReportedLostOrStolen)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsExpired(string tagId)
        {
            foreach (var tag in tagList)
            {
                if (tag.TagId.Equals(tagId) && (DateTime.Now - tag.RenewalDate) > TimeSpan.FromDays(365))
                {
                    return true;
                }
            }

            return false;
        }

        private static int AskUserWhichQuery()
        {
            Console.WriteLine();
            Console.WriteLine("Pick a query:");
            Console.WriteLine("0. Filter and Project");
            Console.WriteLine("1. Tumbling Count");
            Console.WriteLine("2. Hopping Count");
            Console.WriteLine("3. Grouped Hopping");
            Console.WriteLine("4. Grouped Sliding");
            Console.WriteLine("5. Correlation - Join");
            Console.WriteLine("6. Anomaly Detection - LASJ");
            Console.WriteLine("7. Top-K and Rank");
            Console.WriteLine("8. Bottom-up Query Composition - Outer Join");
            Console.WriteLine("9. User Defined Function");
            Console.WriteLine("10. User Defined Aggregate");
            Console.WriteLine("11. User Defined Operator");
            Console.WriteLine("12. Exit from Demo");

            string queryNumStr = Console.ReadLine();
            int queryNum;

            try
            {
                queryNum = Int32.Parse(queryNumStr, new CultureInfo("en-US"));
            }
            catch (FormatException)
            {
                queryNum = 12;
            }

            return queryNum;
        }

        private static void RunFilterProject()
        {
            var inputStream = CepStream<TollReading>.Create("TollStream");
            var queryStream = from w in inputStream
                              where w.TollId == "1"
                              select w;
            BindAndRunQuery(
                "Tumbling",
                queryStream,
                EventShape.Interval,
                new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" },
                new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" });
        }

        /// <summary>[tQ1] "Every 3 minutes, report the number of vehicles processed
        /// that were being processed at some point during that period at
        /// the toll station since the last result. Report the result at a
        /// point in time, at the beginning of the 3 minute window"</summary>
        private static void RunTumblingCountDemo()
        {
            var inputStream = CepStream<TollReading>.Create("TollStream");
            var queryStream = from w in inputStream.TumblingWindow(TimeSpan.FromMinutes(3), HoppingWindowOutputPolicy.ClipToWindowEnd)
                              select new TollCount { Count = w.Count() };
            BindAndRunQuery(
                "Tumbling",
                queryStream,
                EventShape.Interval,
                new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" },
                new List<string>() { "Count" });
        }

        /// <summary>[Q2] “Find the number of vehicles being processed in the toll
        /// station at some time over a 3 minute window, with the time
        /// advancing in one minute hops. Provide the counts as of the last
        /// reported result. Return the result at a point in time at the
        /// end of each 3 minute window” </summary>
        private static void RunHoppingCountDemo()
        {
            var inputStream = CepStream<TollReading>.Create("TollStream");
            var tQ2 = from w in inputStream.HoppingWindow(
                          TimeSpan.FromMinutes(3),
                          TimeSpan.FromMinutes(1),
                          HoppingWindowOutputPolicy.ClipToWindowEnd)
                      select new TollCount
                      {
                          Count = w.Count()
                      };
            var queryStream = from e in tQ2.ShiftEventTime(e => e.StartTime + TimeSpan.FromMinutes(3)).ToPointEventStream()
                              select e;
            BindAndRunQuery(
                "Hopping",
                queryStream,
                EventShape.Point,
                new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" },
                new List<string>() { "Count" });
        }

        /// <summary>[Q3] “Find the toll generated from vehicles being processed at each
        /// toll station at some time over a 3 minute window, with the time advancing
        /// in one minute hops. Provide the value as of the last reported result.
        /// Return the result at a point in time” </summary>
        private static void RunGroupedHoppingWindow()
        {
            var inputStream = CepStream<TollReading>.Create("TollStream");
            var queryStream = from e in inputStream
                              group e by e.TollId into perTollBooth
                              from w in perTollBooth.HoppingWindow(
                                            TimeSpan.FromMinutes(3),    // Window Size
                                            TimeSpan.FromMinutes(1),    // Hop Size
                                            HoppingWindowOutputPolicy.ClipToWindowEnd)
                              select new Toll
                              {
                                  TollId = perTollBooth.Key,
                                  TollAmount = w.Sum(e => e.Toll),
                                  VehicleCount = w.Count()
                              };
            BindAndRunQuery(
                "GroupedHopping",
                queryStream,
                EventShape.Interval,
                new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" },
                new List<string>() { "TollId", "TollAmount", "VehicleCount" });
        }

        /// <summary>[Q4] “Find the most recent toll generated from vehicles being processed
        /// at each station over a 3 minute window reporting the result every time a
        /// change occurs in the input. Return the result at a point in time”.</summary>
        private static void RunGroupedSlidingWindowDemo()
        {
            var inputStream = CepStream<TollReading>.Create("TollStream");
            var queryStream = from e in inputStream.AlterEventDuration(e => TimeSpan.FromMinutes((double)3))
                              group e by e.TollId into perTollBooth
                              from w in perTollBooth.SnapshotWindow(SnapshotWindowOutputPolicy.Clip)
                              select new Toll
                              {
                                  TollId = perTollBooth.Key,
                                  TollAmount = w.Sum(e => e.Toll),
                                  VehicleCount = w.Count() // computed as a bonus, not asked in the query
                              };
            BindAndRunQuery(
                "GroupedSliding",
                queryStream,
                EventShape.Interval,
                new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" },
                new List<string>() { "TollId", "TollAmount", "VehicleCount" });
        }

        /// <summary>[Q5] “Report the output whenever Toll Booth 2 has the same number
        /// of vehicles as Toll Booth 1, every time a change occurs in either stream”</summary>
        private static void RunJoinDemo()
        {
            var inputStream = CepStream<TollReading>.Create("TollStream");
            var slidingWindowQuery = from e in inputStream.AlterEventDuration(e => TimeSpan.FromMinutes((double)3))
                                     group e by e.TollId into perTollBooth
                                     from w in perTollBooth.SnapshotWindow(SnapshotWindowOutputPolicy.Clip)
                                     select new Toll
                                     {
                                         TollId = perTollBooth.Key,
                                         TollAmount = w.Sum(e => e.Toll),
                                         VehicleCount = w.Count()
                                     };
            var stream1 = from e in slidingWindowQuery
                          where e.TollId.Equals(((int)1).ToString())
                          select e;
            var stream2 = from e in slidingWindowQuery
                          where e.TollId.Equals(((int)2).ToString())
                          select e;
            var queryStream = from e1 in stream1
                              join e2 in stream2
                              on e1.VehicleCount equals e2.VehicleCount
                              select new TollCompare
                              {
                                  TollId1 = e1.TollId,
                                  TollId2 = e2.TollId,
                                  VehicleCount = e1.VehicleCount
                              };
            BindAndRunQuery(
                "Join",
                queryStream,
                EventShape.Interval,
                new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" },
                new List<string>() { "TollId1", "TollId2", "VehicleCount" });
        }

        /// <summary>[Q7] "Report the output whenever a vehicle passes a EZ-Pass booth with no
        /// tags (i.e. report a toll violation)”</summary>
        private static void RunLASJDemo()
        {
            var inputStream = CepStream<TollReading>.Create("TollStream");

            // Simulate the reference stream from inputStream itself - convert it to a point event stream
            var referenceStream = from e in inputStream.ToPointEventStream() select e;

            // Simulate the tag violations in the observed stream by filtering out specific
            // vehicles. Let us filter out all the events in the dataset TollInput.txt with
            // a Tag length of 0 (this is the last element in each line in TollInput.txt).
            // In a real scenario, these events will not exist at all – this simulation is only
            // because we are reusing the same input stream for this example.
            // The events that were filtered out should be the ones that show up in the output of tQ7
            var observedStream = from e in inputStream.ToPointEventStream()
                                 where 0 != e.Tag.Length
                                 select e;

            // Report tag violations
            var queryStream = from left in referenceStream
                              where (from right in observedStream
                                     select right).IsEmpty()
                              select left;

            BindAndRunQuery(
                "LASJ",
                queryStream,
                EventShape.Point,
                new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" },
                new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" });
        }

        /// <summary>[Q8] “Report the top 2 toll amounts from the results of Q4 over a 3 minute tumbling window”</summary>
        private static void RunTopKDemo()
        {
            var inputStream = CepStream<TollReading>.Create("TollStream");

            var groupedSliding = from e in inputStream.AlterEventDuration(e => TimeSpan.FromMinutes((double)3))
                                 group e by e.TollId into perTollBooth
                                 from w in perTollBooth.SnapshotWindow(SnapshotWindowOutputPolicy.Clip)
                                 select new Toll
                                 {
                                     TollId = perTollBooth.Key,
                                     TollAmount = w.Sum(e => e.Toll),
                                     VehicleCount = w.Count()
                                 };
            var queryStream = (from window in groupedSliding.TumblingWindow(TimeSpan.FromMinutes((double)3), HoppingWindowOutputPolicy.ClipToWindowEnd)
                               from e in window
                               orderby e.TollAmount descending
                               select e).Take(
                               (uint)2,
                               e => new TopEvents
                               {
                                   TollRank = e.Rank,
                                   TollAmount = e.Payload.TollAmount,
                                   TollId = e.Payload.TollId,
                                   VehicleCount = e.Payload.VehicleCount
                               });
            BindAndRunQuery(
                "Top-K",
                queryStream,
                EventShape.Interval,
                new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" },
                new List<string>() { "TollRank", "TollAmount", "TollId", "VehicleCount" });
        }

        /// <summary>[QouterJoin] - Outer Join using query primitives</summary>
        private static void RunOuterJoinDemo()
        {
            var inputStream = CepStream<TollReading>.Create("TollStream");

            // Simulate the left stream input from inputStream
            var outerJoin_L = from e in inputStream
                              select new
                              {
                                  LicensePlate = e.LicensePlate,
                                  Make = e.Make,
                                  Model = e.Model,
                              };

            // Simulate the right stream input from inputStream – eliminate all events with Toyota as the vehicle
            // These should be the rows in the outer joined result with NULL values for Toll and LicensePlate
            var outerJoin_R = from e in inputStream
                              where !e.Make.Equals("Toyota")
                              select new
                              {
                                  LicensePlate = e.LicensePlate,
                                  Toll = e.Toll,
                                  TollId = e.TollId
                              };

            // Inner join the two simulated input streams
            var innerJoin = from left in outerJoin_L
                            from right in outerJoin_R
                            where left.LicensePlate.Equals(right.LicensePlate)
                            select new TollOuterJoin
                            {
                                LicensePlate = left.LicensePlate,
                                Make = left.Make,
                                Model = left.Model,
                                Toll = right.Toll,
                                TollId = right.TollId
                            };

            // Left anti-semijoin the two input simulated streams, and add the Project
            var outerJoin_lasj = from left in outerJoin_L
                                 where (from right in outerJoin_R
                                        where left.LicensePlate == right.LicensePlate
                                        select right).IsEmpty()
                                 select new TollOuterJoin
                                 {
                                     LicensePlate = left.LicensePlate,
                                     Make = left.Make,
                                     Model = left.Model,
                                     Toll = null,
                                     TollId = null
                                 };

            // Union the two streams to complete a Left Outer Join operation
            var queryStream = innerJoin.Union(outerJoin_lasj);

            BindAndRunQuery(
                "OuterJoin",
                queryStream,
                EventShape.Interval,
                new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" },
                new List<string>() { "LicensePlate", "Make", "Model", "Toll", "TollId" });
        }

        /// <summary>[Q10] “For each vehicle that is being processed at an EZ-Pass booth, report
        /// the TollReading if the tag does not exist, has expired, or is reported stolen</summary>
        private static void RunUDFDemo()
        {
            var inputStream = CepStream<TollReading>.Create("TollStream");
            var queryStream = from e in inputStream
                              where (0 == e.Tag.Length) || IsLostOrStolen(e.Tag) || IsExpired(e.Tag) 
                              select new TollViolation
                              {
                                  LicensePlate = e.LicensePlate,
                                  Make = e.Make,
                                  Model = e.Model,
                                  State = e.State,
                                  Tag = e.Tag,
                                  TollId = e.TollId
                              };
            BindAndRunQuery(
                "UDF",
                queryStream,
                EventShape.Interval,
                new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" },
                new List<string>() { "LicensePlate", "Make", "Model", "State", "Tag", "TollId" });
        }

        /// <summary>[Q11] “Over a 3 minute tumbling window, find the ratio of out-of-state
        /// vehicles to total vehicles being processed at a toll station”</summary>
        private static void RunUDADemo()
        {
            var inputStream = CepStream<TollReading>.Create("TollStream");
            var queryStream = from w in inputStream.TumblingWindow(
                                    TimeSpan.FromMinutes((double)3),
                                    HoppingWindowOutputPolicy.ClipToWindowEnd)
                              select new VehicleRatio
                              {
                                  Value = w.OutOfStateVehicleRatio()
                              };
            BindAndRunQuery(
                "UDA",
                queryStream,
                EventShape.Interval,
                new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" },
                new List<string>() { "Value" });
        }

        /// <summary>[Q12] “Over a one hour tumbling window, report all commercial vehicles
        /// with tonnage greater than one ton (2K lbs), along with their arrival
        /// times at the toll, and any charges due to weight violation. Overweight
        /// charges during the rush hour (7am to 7pm) are double that of non-rush hours”</summary>
        private static void RunUDODemo()
        {
            var inputStream = CepStream<TollReading>.Create("TollStream");
            var queryStream = from w in inputStream.TumblingWindow(TimeSpan.FromHours((double)1), HoppingWindowOutputPolicy.ClipToWindowEnd)
                              select w.VehicleWeights();
            BindAndRunQuery(
                "UDO",
                queryStream,
                EventShape.Interval,
                new List<string>() { "TollId", "LicensePlate", "State", "Make", "Model", "VehicleType", "VehicleWeight", "Toll", "Tag" },
                new List<string>() { "LicensePlate", "Weight", "WeightCharge" });
        }

        private static void BindAndRunQuery<TPayload>(string queryName, CepStream<TPayload> queryStream, EventShape outputEventShape, List<string> inputFields, List<string> outputFields)
        {
            var inputConfig = new CsvInputConfig
            {
                InputFileName = @"..\..\..\TollInput.txt",
                Delimiter = new char[] { ',' },
                BufferSize = 4096,
                CtiFrequency = 1,
                CultureName = "en-US",
                Fields = inputFields,
                NonPayloadFieldCount = 2,
                StartTimePos = 1,
                EndTimePos = 2
            };

            // The adapter recognizes empty filename as a write to console.
            var outputConfig = new CsvOutputConfig
            {
                OutputFileName = string.Empty,
                Delimiter = new string[] { "\t" },
                CultureName = "en-US",
                Fields = outputFields
            };

            // Note - Please change the instance name to the one you have chosen during installation
            using (var server = Server.Create("Default"))
            {
                Application application = server.CreateApplication("TollStationApp");

                // set up input and output adapters
                InputAdapter inputAdapter = application.CreateInputAdapter<CsvInputFactory>("input", "Csv Input Source");
                OutputAdapter outputAdapter = application.CreateOutputAdapter<CsvOutputFactory>("output", "Csv Output");

                // set up the query template
                QueryTemplate queryTemplate = application.CreateQueryTemplate("QueryTemplate", string.Empty, queryStream);

                // set up advance time settings to enqueue CTIs
                var advanceTimeGenerationSettings = new AdvanceTimeGenerationSettings(inputConfig.CtiFrequency, TimeSpan.Zero, true); 
                var advanceTimeSettings = new AdvanceTimeSettings(advanceTimeGenerationSettings, null, AdvanceTimePolicy.Adjust);

                // Bind query template to input and output streams
                QueryBinder queryBinder = new QueryBinder(queryTemplate);
                queryBinder.BindProducer<TollReading>("TollStream", inputAdapter, inputConfig, EventShape.Interval, advanceTimeSettings);
                queryBinder.AddConsumer("outputStream", outputAdapter, outputConfig, outputEventShape, StreamEventOrder.FullyOrdered);

                // Create a runnable query by binding the query template to the input stream of interval events,
                // and to an output stream of fully ordered point events (through an output adapter instantiated
                // from the output factory class)
                Query query = application.CreateQuery(queryName, "Hello Toll Query", queryBinder);

                RunQuery(query);
            }
        }

        private static void RunQuery(Query query)
        {
            query.Start();

            Console.WriteLine("***Hit Return to exit after viewing query output***");
            Console.WriteLine();
            Console.ReadLine();

            query.Stop();
        }

        internal static void Main()
        {
            while (true)
            {
                int queryToRun = AskUserWhichQuery();

                switch (queryToRun)
                {
                    case 0:
                        RunFilterProject();
                        break;
                    case 1:
                        RunTumblingCountDemo();
                        break;
                    case 2:
                        RunHoppingCountDemo();
                        break;
                    case 3:
                        RunGroupedHoppingWindow();
                        break;
                    case 4:
                        RunGroupedSlidingWindowDemo();
                        break;
                    case 5:
                        RunJoinDemo();
                        break;
                    case 6:
                        RunLASJDemo();
                        break;
                    case 7:
                        RunTopKDemo();
                        break;
                    case 8:
                        RunOuterJoinDemo();
                        break;
                    case 9:
                        RunUDFDemo();
                        break;
                    case 10:
                        RunUDADemo();
                        break;
                    case 11:
                        RunUDODemo();
                        break;
                    case 12:
                        return;
                    default:
                        Console.WriteLine("Unknown Query Demo");
                        break;
                }
            }
        }
    }
}