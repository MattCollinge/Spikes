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

namespace StreamInsight.Samples.SequenceIntegration.HitchhikersGuide
{
    using System;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;

    class Queries
    {
        [DemoQuery(1, "Filter and Project", "Illustrates a simple filter and project query.")]
        public static dynamic RunFilterProject(Application application)
        {
            var inputStream = InputData.CreateTollDataInput(application);

            var queryStream = from w in inputStream
                              where w.TollId == "1"
                              select new { w.Make, w.Model, w.LicensePlate };

            return queryStream.ToIntervalObservable();
        }

        [DemoQuery(2, "Tumbling Window Count", 
            "Every 3 minutes, report the number of vehicles processed that were being processed at some " +
            "point during that period at the toll station since the last result. Report the result at a " +
            "point in time, at the beginning of the 3 minute window.")]
        public static dynamic RunTumblingCountDemo(Application application)
        {
            var inputStream = InputData.CreateTollDataInput(application);

            var queryStream = from w in inputStream.TumblingWindow(TimeSpan.FromMinutes(3), HoppingWindowOutputPolicy.ClipToWindowEnd)
                              select new { Count = w.Count() };

            return queryStream.ToIntervalObservable();
        }

        [DemoQuery(3, "Hopping Window Count",
            "Find the number of vehicles being processed in the toll station at some time over a 3 minute " +
            "window, with the time advancing in one minute hops. Provide the counts as of the last reported " +
            "result. Return the result at a point in time at the end of each 3 minute window.")]
        public static dynamic RunHoppingCountDemo(Application application)
        {
            var inputStream = InputData.CreateTollDataInput(application);

            var tQ2 = from w in inputStream.HoppingWindow(
                          TimeSpan.FromMinutes(3),
                          TimeSpan.FromMinutes(1),
                          HoppingWindowOutputPolicy.ClipToWindowEnd)
                      select new
                      {
                          Count = w.Count()
                      };
            var queryStream = from e in tQ2.ShiftEventTime(e => e.StartTime + TimeSpan.FromMinutes(3)).ToPointEventStream()
                              select e;

            return queryStream.ToPointObservable();
        }

        [DemoQuery(4, "Grouped Hopping Window",
            "Find the toll generated from vehicles being processed at each toll station at some time over " +
            "a 3 minute window, with the time advancing in one minute hops. Provide the value as of the " +
            "last reported result. Return the result at a point in time.")]
        public static dynamic RunGroupedHoppingWindow(Application application)
        {
            var inputStream = InputData.CreateTollDataInput(application);

            var queryStream = from e in inputStream
                              group e by e.TollId into perTollBooth
                              from w in perTollBooth.HoppingWindow(
                                            TimeSpan.FromMinutes(3),    // Window Size
                                            TimeSpan.FromMinutes(1),    // Hop Size
                                            HoppingWindowOutputPolicy.ClipToWindowEnd)
                              select new
                              {
                                  TollId = perTollBooth.Key,
                                  TollAmount = w.Sum(e => e.Toll),
                                  VehicleCount = w.Count()
                              };

            return queryStream.ToIntervalObservable();
        }

        [DemoQuery(5, "Grouped Sliding Window",
            "Find the most recent toll generated from vehicles being processed at each station over a " +
            "3 minute window reporting the result every time a change occurs in the input. Return the result " +
            "at a point in time.")]
        public static dynamic RunGroupedSlidingWindowDemo(Application application)
        {
            var inputStream = InputData.CreateTollDataInput(application);

            var queryStream = from e in inputStream.AlterEventDuration(e => TimeSpan.FromMinutes((double)3))
                              group e by e.TollId into perTollBooth
                              from w in perTollBooth.SnapshotWindow(SnapshotWindowOutputPolicy.Clip)
                              select new
                              {
                                  TollId = perTollBooth.Key,
                                  TollAmount = w.Sum(e => e.Toll),
                                  VehicleCount = w.Count() // computed as a bonus, not asked in the query
                              };

            return queryStream.ToIntervalObservable();
        }

        [DemoQuery(6, "Join",
            "Report the output whenever Toll Booth 2 has the same number of vehicles as Toll Booth 1, " +
            "every time a change occurs in either stream.")]
        public static dynamic RunJoinDemo(Application application)
        {
            var inputStream = InputData.CreateTollDataInput(application);

            var slidingWindowQuery = from e in inputStream.AlterEventDuration(e => TimeSpan.FromMinutes((double)3))
                                     group e by e.TollId into perTollBooth
                                     from w in perTollBooth.SnapshotWindow(SnapshotWindowOutputPolicy.Clip)
                                     select new
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
                              select new
                              {
                                  TollId1 = e1.TollId,
                                  TollId2 = e2.TollId,
                                  VehicleCount = e1.VehicleCount
                              };

            return queryStream.ToIntervalObservable();
        }

        [DemoQuery(7, "LASJ",
            "Report the output whenever a vehicle passes a EZ-Pass booth with no tags (i.e. report a toll violation).")]
        public static dynamic RunLasjDemo(Application application)
        {
            var inputStream = InputData.CreateTollDataInput(application);

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

            return queryStream.ToPointObservable();
        }

        [DemoQuery(8, "Top K",
            "Report the top 2 toll amounts from the results of Q4 over a 3 minute tumbling window.")]
        public static dynamic RunTopKDemo(Application application)
        {
            var inputStream = InputData.CreateTollDataInput(application);

            var groupedSliding = from e in inputStream.AlterEventDuration(e => TimeSpan.FromMinutes((double)3))
                                 group e by e.TollId into perTollBooth
                                 from w in perTollBooth.SnapshotWindow(SnapshotWindowOutputPolicy.Clip)
                                 select new
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
                               e => new
                               {
                                   TollRank = e.Rank,
                                   TollAmount = e.Payload.TollAmount,
                                   TollId = e.Payload.TollId,
                                   VehicleCount = e.Payload.VehicleCount
                               });

            return queryStream.ToIntervalObservable();
        }

        [DemoQuery(9, "Outer Join",
            "Outer Join using query primitives.")]
        public static dynamic RunOuterJoinDemo(Application application)
        {
            var inputStream = InputData.CreateTollDataInput(application);

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
                            select new
                            {
                                LicensePlate = left.LicensePlate,
                                Make = left.Make,
                                Model = left.Model,
                                Toll = (float?)right.Toll,
                                TollId = right.TollId
                            };

            // Left anti-semijoin the two input simulated streams, and add the Project
            var outerJoin_lasj = from left in outerJoin_L
                                 where (from right in outerJoin_R
                                        where left.LicensePlate == right.LicensePlate
                                        select right).IsEmpty()
                                 select new
                                 {
                                     LicensePlate = left.LicensePlate,
                                     Make = left.Make,
                                     Model = left.Model,
                                     Toll = (float?)null,
                                     TollId = (string)null
                                 };

            // Union the two streams to complete a Left Outer Join operation
            var queryStream = innerJoin.Union(outerJoin_lasj);

            return queryStream.ToIntervalObservable();
        }

        [DemoQuery(10, "User-Defined Function",
            "For each vehicle that is being processed at an EZ-Pass booth, report the TollReading if the " +
            "tag does not exist, has expired, or is reported stolen.")]
        public static dynamic RunUdfDemo(Application application)
        {
            var inputStream = InputData.CreateTollDataInput(application);

            var queryStream = from e in inputStream
                              where (0 == e.Tag.Length) || e.Tag.IsLostOrStolen() || e.Tag.IsExpired()
                              select new
                              {
                                  LicensePlate = e.LicensePlate,
                                  Make = e.Make,
                                  Model = e.Model,
                                  State = e.State,
                                  Tag = e.Tag,
                                  TollId = e.TollId
                              };

            return queryStream.ToIntervalObservable();
        }

        [DemoQuery(11, "User-Defined Aggregate",
            "Over a 3 minute tumbling window, find the ratio of out-of-state vehicles to total vehicles " +
            "being processed at a toll station.")]
        public static dynamic RunUdaDemo(Application application)
        {
            var inputStream = InputData.CreateTollDataInput(application);

            var queryStream = from w in inputStream.TumblingWindow(
                                    TimeSpan.FromMinutes((double)3),
                                    HoppingWindowOutputPolicy.ClipToWindowEnd)
                              select new
                              {
                                  Value = w.OutOfStateVehicleRatio()
                              };

            return queryStream.ToIntervalObservable();
        }

        [DemoQuery(12, "User-Defined Operator Demo",
            "Over a one hour tumbling window, report all commercial vehicles with tonnage greater than " +
            "one ton (2K lbs), along with their arrival times at the toll, and any charges due to weight " +
            "violation. Overweight charges during the rush hour (7am to 7pm) are double that of non-rush " +
            "hours.")]
        private static dynamic RunUdoDemo(Application application)
        {
            var inputStream = InputData.CreateTollDataInput(application);

            var queryStream = from w in inputStream.TumblingWindow(TimeSpan.FromHours((double)1), HoppingWindowOutputPolicy.ClipToWindowEnd)
                              select w.VehicleWeights();

            return queryStream.ToIntervalObservable();
        }
    }
}
