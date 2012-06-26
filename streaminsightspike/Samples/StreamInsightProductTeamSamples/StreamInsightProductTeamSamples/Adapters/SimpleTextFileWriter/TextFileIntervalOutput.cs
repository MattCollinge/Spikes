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

namespace StreamInsight.Samples.Adapters.SimpleTextFileWriter
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    /// <summary>
    /// Interval version of the simple text file writer output adapter.
    /// <para/>
    /// TextFileIntervalOutput adapter dequeues generic interval events into a CSV file with the format:
    ///   {EventKind}{delimiter}{Starttime}{delimiter}{EndTime}{delimiter}{field2}{delimiter}...{fieldN}
    /// By generic events, we mean that, for a given output device (in this case a CSV file), you
    /// can specify an arbitrary payload class or struct at the time of query binding. This enables
    /// you to reuse this adapter implementation for multiple instantiations of the adapter - where
    /// each instantiation corresponds to a particular payload type, and is associated/bound 1:1
    /// with a query.
    /// <para/>
    /// Please study this code in conjunction with the adapter state transition diagram provided in
    /// the documentation.
    /// </summary>
    public class TextFileIntervalOutput : IntervalOutputAdapter
    {
        /// <summary>
        /// Stream to write to.
        /// </summary>
        private StreamWriter streamWriter;

        /// <summary>
        /// Delimiting character between each field to write.
        /// </summary>
        private char delimiter;

        /// <summary>
        /// Stream type received at binding time.
        /// </summary>
        private CepEventType bindtimeEventType;

        /// <summary>
        /// Debug and error output.
        /// </summary>
        private System.Diagnostics.TraceListener consoleTracer;

        /// <summary>
        /// Initializes a new instance of the TextFileIntervalOutput class.
        /// </summary>
        /// <param name="configInfo">Configuration passed from the factory.</param>
        /// <param name="eventType">Runtime event type passed from the factory.</param>
        public TextFileIntervalOutput(TextFileWriterConfig configInfo, CepEventType eventType)
        {
            // If an empty string as filename was provided, dump output on the 
            // console
            this.streamWriter = configInfo.OutputFileName.Length == 0 ?
                              new StreamWriter(Console.OpenStandardOutput()) :
                              new StreamWriter(configInfo.OutputFileName);

            this.delimiter = configInfo.Delimiter;
            this.bindtimeEventType = eventType;

            this.consoleTracer = new System.Diagnostics.ConsoleTraceListener();
        }

        /// <summary>
        /// Start() is called when the engine wants to let the adapter start producing events.
        /// This method is called on a threadpool thread, which should be released as soon as possible.
        /// </summary>
        public override void Start()
        {
            new Thread(this.ConsumeEvents).Start();
        }

        /// <summary>
        /// Resume() is called when the engine is able to produce further events after having been emptied
        /// by Dequeue() calls. Resume() will only be called after the adapter called Ready().
        /// This method is called on a threadpool thread, which should be released as soon as possible.
        /// </summary>
        public override void Resume()
        {
            new Thread(this.ConsumeEvents).Start();
        }

        /// <summary>
        /// Default Dispose from the base class.
        /// </summary>
        /// <param name="disposing">Indicates whether to free both managed and unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.streamWriter.Dispose();
                this.consoleTracer.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Main driver to dequeue events and output them to the CSV file
        /// </summary>
        private void ConsumeEvents()
        {
            IntervalEvent currentEvent = default(IntervalEvent);

            try
            {
                while (true)
                {
                    if (AdapterState.Stopping == AdapterState)
                    {
                        this.streamWriter.Flush();
                        this.streamWriter.Close();

                        Stopped();

                        return;
                    }

                    if (DequeueOperationResult.Empty == Dequeue(out currentEvent))
                    {
                        Ready();
                        return;
                    }

                    this.streamWriter.WriteLine(TextFileWriterCommon.CreateLineFromEvent(currentEvent, this.bindtimeEventType, this.delimiter));

                    // Every received event needs to be released.
                    ReleaseEvent(ref currentEvent);
                }
            }
            catch (AdapterException e)
            {
                this.consoleTracer.WriteLine("ConsumeEvents - " + e.Message + e.StackTrace);
            }
        }
    }
}
