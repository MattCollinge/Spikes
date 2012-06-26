========================================
SequenceIntegration Samples
========================================

Last change: 2010-10-26

These samples illustrate techniques for integrating .NET push- and pull-
based sequences with StreamInsight. StreamInsight supports IEnumerable<>
and IObservable<> event sources and event sinks. Three applications are
included as well as one class library. The contents of and principles
illustrated by these projects are outlined below.

In order to run these samples, you need to set one of the executable
projects (HitchhikersGuide, Northwind, PerformanceCounters) as startup
project.

Common
------

A class library containing some generally useful functionality:

  * ObservableDataGridControl: supports visualization of IObservable
    event sinks and includes specialized behavior for StreamInsight event types.
  * Utility: methods supporting construction of IObservable, IDisposable
    and IObserver instances based on implementations of the interface members. For a
	broader range of utilities and helpers related to IObservable sequences,
	see Reactive Extensions for .NET:
	
	http://msdn.microsoft.com/en-us/devlabs/ee794896.aspx

HitchhikersGuide
----------------

A remix of the Hitchhiker's Guide to StreamInsight sample application
targeting StreamInsight 1.0:

http://blogs.msdn.com/b/streaminsight/archive/2010/06/08/hitchhiker-s-guide-to-streaminsight-queries.aspx

Concepts:

  * Using IEnumerable<> event source to read from a file.
  * Using IObservable<> event sinks to integrate with WPF surface.

Northwind
---------

A simple standalone application that connects to an OData feed to read historical
data, in this case data from the Northwind database. Order data is translated
into a temporal stream that is then queried using StreamInsight's LINQ dialect.

Concepts:

  * Using an IQueryable<> as an IEnumerable<> event source to read from an OData endpoint.
  * Leveraging StreamInsight's temporal query capabilities against historical-relational
    data.
  * Using IEnumerable<> event sinks to write to the console.

PerformanceCounters
-------------------

Ties together multiple event sources and sinks in a dashboard application. Real-time
data is produced by an IObservable<> event source bound to performance counters. Two queries
are exposed through a simple dashboard:

  * Queries.ProcessorUtilizationPerCore: measures the processor utilization per core
    on the local machine averaged over hopping windows.
  * Queries.ProcessorUtilizationSustainedThreshold: produces output whenever the processor
    utilization exceeds a threshold consistently over some duration. Both the threshold
	and duration are configurable in real-time via the UI.

Concepts:

  * Creating an IObservable<> event source based on performance counters.
  * Creating an IObservable<> event source based on UI events.
  * Using AdvanceTimeSettings to drive time between two sequence-based event
    sources.
