========================================
SimpleTextFileReader Input Adapter
========================================

Last change: 5/24/2010

This set of input adapters demonstrates the basics of writing an input
adapter. It includes point, interval, and edge adapters. Each adapter reads
from a specified text file and expects lines with comma-separated values
according to the respective event shape.

These adapters are very simple (hence the name) and not targeted towards
performance. They use a StreamReader and read the file line by line until
its end. They show the following important aspects of the adapter development:

  * How to correctly handle the adapter state diagram.
  * How to release the threadpool thread the engine uses to call into the
    adapter.
  * How to retry enqueuing an event when the engine pushes back.
  * When to release an allocated event.
  * How to correctly assemble start and end edges in case of the edge adapter.