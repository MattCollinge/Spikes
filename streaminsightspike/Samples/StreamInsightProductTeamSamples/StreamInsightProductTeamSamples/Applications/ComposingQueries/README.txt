========================================
ComposingQueries Sample
========================================

Last change: 5/24/2010

This sample processes data from a data generator input adapter and streams its
results to a tracer output adapter. The application contains two chained
queries: the 'primary' query uses a user-defined aggregate to compute the
deltas between two subsequent readings from each device simulated by the data
generator. The result of this query is displayed through a tracer output
adapter. A 'secondary' query is composed on top of the first one, feeding
off its results and applying further query logic: it filters out a single
device and computes a maximum within a hopping window. This result is
streamed to another tracer output adapter. Both tracers dump into the same
console window, but are differentiated by their respective names, showing
as prefixes of each event line.

By uncommenting the lines host.Open() and host.Close(), the web service
manageability endpoint can be activated, in order to attach the debugger tool
and inspect the queries and runtime diagnostics.

This sample demonstrates the following aspects:

  * Specifying a query directly on top of an input adapter (implicit binding).
  * Exposing the manageability web service endpoint.
  * Using a count window.
  * Defining and using a user-defined aggregate.
  * Composing queries at runtime.
  