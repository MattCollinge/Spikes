========================================
SimpleTextFileWriter Output Adapter
========================================

Last change: 5/24/2010

This set of output adapters demonstrates the basics of writing an output
adapter. It includes point, interval, and edge adapters. Each adapter writes
into a specified text file, using a comma-separated value format, one event
per line.

These adapters are very simple (hence the name) and not targeted towards
performance. They use a StreamWriter and write the file line by line. They show
the following important aspects of the adapter development:

  * How to correctly handle the adapter state diagram.
  * How to release the threadpool thread the engine uses to call into the
    adapter.
  * When to release a dequeued event.
  * How to signal the end of the stream to the main application, if running
    in the same process.
