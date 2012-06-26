========================================
Tracer Output Adapter
========================================

Last change: 5/24/2010

This set of output adapters demonstrates how to dump result events on a trace,
for point, interval, and edge output. The adapters post the events to one of
the standard "debugging" streams (Console, Debug, Trace and File).

These adapters are useful for testing, debugging and prototyping purposes.
They show:

  * How to correctly handle the adapter state diagram.
  * How to release the threadpool thread the engine uses to call into the
    adapter.
  * When to release a dequeued event.
