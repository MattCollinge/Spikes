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

namespace StreamInsight.Samples.SequenceIntegration.Northwind
{
    using System;
    using System.Linq;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;
    using StreamInsight.Samples.SequenceIntegration.Northwind.OData;

    class Program
    {
        static void Main(string[] args)
        {
            NorthwindEntities northwind = new NorthwindEntities(
                new Uri("http://services.odata.org/Northwind/Northwind.svc"));

            using (Server server = Server.Create("Default"))
            {
                Application application = server.CreateApplication("app");

                RunQuery(northwind, application);
            }
        }

        static void RunQuery(NorthwindEntities northwind, Application application)
        {
            // Issue OData queries to determine start and end times for orders.
            // So that the sources behave like temporal streams, we order by the
            // corresponding dates.
            var ordersWithRegions =
                from o in northwind.Orders
                where o.ShipRegion != null
                select o;

            var orderStartTimes =
                from o in ordersWithRegions
                where o.OrderDate != null
                orderby o.OrderDate
                select new { StartTime = (DateTime)o.OrderDate, o.OrderID, o.ShipRegion };

            var orderEndTimes =
                from o in ordersWithRegions
                where o.ShippedDate != null
                orderby o.ShippedDate
                select new { EndTime = (DateTime)o.ShippedDate, o.OrderID };

            // Map OData queries to StreamInsight inputs
            var startStream = orderStartTimes.ToPointStream(application, s =>
                PointEvent.CreateInsert(s.StartTime, s), AdvanceTimeSettings.IncreasingStartTime);

            var endStream = orderEndTimes.ToPointStream(application, e =>
                PointEvent.CreateInsert(e.EndTime, e), AdvanceTimeSettings.IncreasingStartTime);

            // Use clip to synthesize events lasting from the start of each order to the end
            // of each order.
            var clippedStream = startStream
                .AlterEventDuration(e => TimeSpan.MaxValue)
                .ClipEventDuration(endStream, (s, e) => s.OrderID == e.OrderID);

            // Count the number of coincident orders per region
            var counts = from o in clippedStream
                         group o by o.ShipRegion into g
                         from win in g.SnapshotWindow(SnapshotWindowOutputPolicy.Clip)
                         select new { ShipRegion = g.Key, Count = win.Count() };

            // Display output whenever there are more than 2 active orders in a region.
            const int threshold = 2;
            var query = from c in counts
                        where c.Count > threshold
                        select c;

            // Map the query to an IEnumerable sink
            var sink = from i in query.ToIntervalEnumerable()
                       where i.EventKind == EventKind.Insert
                       select new { i.StartTime, i.EndTime, i.Payload.Count, i.Payload.ShipRegion };

            foreach (var r in sink)
            {
                Console.WriteLine(r);
            }
        }
    }
}
