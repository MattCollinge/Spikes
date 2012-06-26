========================================
TrafficJoin Sample
========================================

Last change: 5/24/2010

This sample represents a StreamInsight application that uses the Object Model
API. The user registers the adapters and the query logic (in form of a LINQ
query template) explicitly into the server's metadata and then binds them into
a query, using specific adapter configurations.

The sample data represents traffic control data. Two input streams are
processed:

  1. Road sensor readings from several sensors, delivering throughput and
     speed of vehicles.
  2. Reference data, containing sensor IDs and their location codes.

Here, both data streams are read from CSV files, using the appropriate text
file input adapter. On the output side, the text file output adapter is used
with an empty string as filename, which will cause the adapter to write its
results directly to the console window.

The sample query shows how to compute the average of the vehicle count for a
sliding one-minute window for each sensor separately and how to join the
real-time data stream with the reference data stream. Modeling such static or
slowly changing data as a CEP stream is an alternative to implementing a custom
lookup function towards a reference data store.

The result is represented by the outgoing stream of point events. Each such
event is the average vehicle count for the minute preceding the given
timestamp, for all values that have passed the included filter condition.

This sample also demonstrates the usage of diagnostic views at runtime. After
the input data has been entirely consumed, a snapshot of diagnostic data about
the query is taken and displayed on the console.