//*********************************************************
//
//  Copyright (c) Microsoft. All rights reserved.
//  This code is licensed under the Apache 2.0 License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OR
//  CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
//  INCLUDING, WITHOUT LIMITATION, ANY IMPLIED WARRANTIES
//  OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

namespace StreamInsight.Samples.Adapters.OutputTracer
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    /// <summary>
    /// Interval version of the tracer output adapter.
    /// </summary>
    internal sealed class TracerIntervalOutputAdapter : IntervalOutputAdapter
    {
        /// <summary>
        /// Tracer to use to output events.
        /// </summary>
        private readonly Tracer tracer;

        /// <summary>
        /// Initializes a new instance of the TracerIntervalOutputAdapter class, based on
        /// the payload type (CepEventType) and the configuration information.
        /// </summary>
        /// <param name="config">Configuration passed from the factory.</param>
        /// <param name="type">Runtime event type passed from the factory.</param>
        public TracerIntervalOutputAdapter(TracerConfig config, CepEventType type)
        {
            this.tracer = new Tracer(config, type);
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
        /// Main worker thread function responsible for dequeueing events and 
        /// posting them to the output stream.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catching all exceptions to avoid stopping process")]
        private void ConsumeEvents()
        {
            IntervalEvent currentEvent = default(IntervalEvent);

            try
            {
                while (true)
                {
                    if (AdapterState.Stopping == AdapterState)
                    {
                        Stopped();
                        return;
                    }

                    // Dequeue the event. If the dequeue fails, then the adapter state is suspended
                    // or stopping. Assume the former and call Ready() to indicate
                    // readiness to be resumed, and exit the thread.
                    if (DequeueOperationResult.Empty == Dequeue(out currentEvent))
                    {
                        Ready();
                        return;
                    }

                    if (currentEvent.EventKind == EventKind.Insert)
                    {
                        string prefix = String.Format(
                            CultureInfo.InvariantCulture,
                            "Interval from {0} to {1}:",
                            currentEvent.StartTime,
                            currentEvent.EndTime);

                        this.tracer.TraceInsert(currentEvent, prefix);
                    }
                    else if (currentEvent.EventKind == EventKind.Cti)
                    {
                        this.tracer.TraceCti(currentEvent.StartTime);
                    }

                    // Every received event needs to be released.
                    ReleaseEvent(ref currentEvent);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
