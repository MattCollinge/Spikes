========================================
AsyncCsv Adapters
========================================

Last change: 8/10/2010


Input Adapters

The input adapters in this project are demonstrating the following design
aspects:

 - Abstraction of common adapter code
 - Reading from an input file using asynchronous IO

The point and interval adapter classes both derive from the appropriate
StreamInsight base class, and implement the interface IInputAdapter.
They also both maintain an object of type CsvInputAdapter, which is
the generic adapter implementation. This class receives the concrete
adapter in its constructor, so that it can interact with the StreamInsight
adapter API via the interface that the concrete classes implement. Hence,
The implementation of CsvPointInput and CsvIntervalInput become very concise.
(Adding an input edge adapter would be straightforward from there.)

The asynchronous IO is implemented through FileStream.BeginRead(), which starts
Reading a file into a buffer. As soon as the specified amount is read, a
callback () is executed, which is then converting the buffer into lines and
further into events, and enqueueing them into the engine. Only after that,
the next block of data is read from the file. Hence, the operation is still
synchronous in the sense that nothing is executed concurrent with the reading
of the input file.
If the engine pushes pack during enqueueing, the adapter will wait for a signal
to be set, which happens in the Resume() method.


Output Adapters

The output adapters in this project follow the same abstraction approach as the
input adapters. In this version, they are producing results synchronously.