========================================================
Simple SQL Adapter and Application for StreamInsight 1.2
========================================================

Last change: 06/09/2011

This set of input and output adapters demonstrate a simple interface
with a Microsoft SQL Server source and target. These are untyped or
generic adapters in the sense that the adapters are not designed to
work with just one specific event type, but can work with an arbitrary
event type that is provided by the StreamInsight application, as a
parameter to the query bind interface.

The focus in this simple SQL adapter sample is to show:
  * How to build an adapter that is not tied to a particular event type,
    but can work with the event type that is provided during bind time.
  * How to build an adapter that works with events being supplied by a SQL
    data source, and/or written into a SQL data sink.
  * How a row from a SQL database corresponds to an event instance
  * How to correctly handle the adapter state transitions, including the
    new Stop() interface introduced in StreamInsight 1.2.

Any adapter design entails two parts:
  (A) The adapter's interactions and configuration with the input or output device. 
  (B) The adapter's interaction with the engine.

Adapter interactions and configuration with Input:
  * You specify the database configuration parameters and specify a SQL statement
    through the SqlInputConfig class. (The sample SqlApplication uses AdventureWorks
    and SalesHeader table.)
  * You specify an input EventType that has the same set of fields in name and type
    as the columns you project out from the SELECT statement. The SQL input adapter
    verifies that the two match.
  * The input adapter reads from the SQL data source synchronously using SqlDataReader.
    
Adapter interactions and configuration with Output:
  * Similar to the input, you specify the db configuration parameters and an INSERT
    statement through the SqlOutputConfig class (The sample SqlApplication uses
    AdventureWorks). Unlike the input adapter, there is no equivalent check to
    make sure that the type matches in name with the columns specified in INSERT
    statement - any mismatch will be detected and surfaced at runtime. This is done
    by design, to help a curious developer step through the debugger and understand
    how exceptions are handled.
  * The adapter expects the target table to exist, and as part of initialization, it
    truncates the table.

Adapter Interactions with the Server:
  * The input (output) adapters synchronously enqueue (dequeue) events - with the
    producer and consumer routines executing on the same worker thread that the
    engine assigns for Start() and Resume() methods.
  * In the event of runtime exceptions, the adapter handle the exceptions, do the
    cleanup, and then re-throw those exceptions so that the query can detect this
    and start the shutdown process. The query does this by calling the
    Stop() routine - which does the cleanup of resources and invokes Stopped() -
    thereby allowing the engine to complete the query shutdown in a graceful manner.

Application to exercise the Adapter sample:

The requirements to exercise this sample are:
1. You must have access to a SQL Server database. AdventureWorks database - this can be downloaded
   and installed from the SQL Server Samples area in Codeplex.
2. Download the SimpleTextFileWriter from CodePlex (where you got this sample from).
2. Add a reference to this project from your SqlApplication solution in Visual Studio.
3. Add a reference to the SqlAdapter project from your SqlApplication solution.
4. Build the application and Run.

SqlApplication is the main program that exercises the SQL adapter. Events are
provided from the SalesHeader table that is part of the AdventureWorks database.
The output events are inserted into a pre-existing table that has columns
that match the specified output event type in both name and type. The input
events are modeled as interval events with the OrderDate column mapping to
the event StartTime and ShipDate mapping to the event EndTime. The output
events are modeled as interval events. The query logic returns the the time
periods (i.e. intervals) when the order count exceeded 3.

The application provides you the ability to output your events to the table,
or for your test purposes, to a file (or the console itself - if you provide
a null filename).

The input adapter retrieves data from the source query (as a proxy for a table
or view) synchronously. The output adapter performs row by row inserts into an
existing table synchronously.

The output adapter truncates an existing table during initialization. This helps
you repeat the sample run and validate the row count after a complete run. The
application will return 7366 events in output - this includes the CTI events and
final Infinity-CTI event. If you divert the output to the table, you will notice
6240 rows - the rest of the rows are accounted for by the CTI events (which are
skipped in the output adapter). 

If you'd like to make changes to your event source and or your types:

Do the following for the input side:
1. Provide a new SELECT statement as part of the input configuration object,
   and fill up the rest of the configuration fields.
2. Provide an EventType that matches the columns from the SELECT statement
   in both name and type (i.e. matching SqlDbType and CLR types)

Similarly for the output side:
3. Provide an EventType for output that reflects the result that you expect
4. In the output configuration object, provide a tablename that exists in the
   target database and has columns that match the EventType in both name and type
   (i.e. matching SqlDbType and CLR types). The output adapter does not strictly
   enforce the CLR to SqlDbType for the field values. Instead, when retrieving
   specific field values from a CepEvent, it uses the SqlDataReader's ability
   to correctly map to the column by name, and the Event Type's ability to provide
   the field's ordinal to ensure the correct mapping. The output value will be
   implicitly converted based on this mapping. For this example, the table is
   created using this statement if not present:
   create table [AdventureWorks].[Dbo].[PeakSalesByTerritory] (
			OrderDate DateTime,
			ShipDate  DateTime,
			OrderCount integer,
			TerritoryID integer)
5. Change the LINQ query statement that consumes and produces events.
6. Compile the application and run.

General Notes:
7. The InitInput and InitOutput methods demonstrate the use of ADO.NET 3.5
   disconnected constructs for table to field mapping. The motivation is to
   enable you to adapt this program to work against any other database. Of
   course, you will need to replace SqlDataReader with the suitable ADO.NET
   reader object provided for your target database.
