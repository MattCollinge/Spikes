﻿// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace AdvantIQ.StockInsight.OutputAdapters
{
    /// <summary>
    /// Simple output adapter that writes quotes to console
    /// </summary>
    public class StockQuoteTypedPointOutput : TypedPointOutputAdapter<StockQuote>
    {
        private EventWaitHandle _adapterStopSignal;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Adapter configuration</param>
        public StockQuoteTypedPointOutput(StockQuoteOutputConfig config)
        {
            if (!string.IsNullOrEmpty(config.AdapterStopSignal))
                _adapterStopSignal = EventWaitHandle.OpenExisting(config.AdapterStopSignal);
            else
                _adapterStopSignal = null;
        }

        public override void Start()
        {
            ConsumeEvents();
        }

        public override void Resume()
        {
            ConsumeEvents();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// Main loop
        /// </summary>
        private void ConsumeEvents()
        {
            PointEvent<StockQuote> currEvent;
            DequeueOperationResult result;

            try
            {
                // Run until stop state
                while (AdapterState != AdapterState.Stopping)
                {
                    result = Dequeue(out currEvent);

                    // Take a break if queue is empty
                    if (result == DequeueOperationResult.Empty)
                    {
                        PrepareToResume();
                        Ready();
                        return;
                    }
                    else
                    {
                        //PrintEvent(currEvent);

                        // Write to console
                        if (currEvent.EventKind == EventKind.Insert)
                        {
                            Console.WriteLine("Output: " +
                                currEvent.StartTime + " " +
                                currEvent.Payload.StockID + " " +
                                currEvent.Payload.FieldID + " " +
                                currEvent.Payload.Value.ToString("f2"));
                        }

                        ReleaseEvent(ref currEvent);
                    }
                }
                result = Dequeue(out currEvent);
                PrepareToStop(currEvent, result);
                Stopped();
            }
            catch (AdapterException e)
            {
                Console.WriteLine("AdvantIQ.StockInsightTypedPointOutput.ConsumeEvents - " + e.Message + e.StackTrace);
            }

            if (_adapterStopSignal != null)
                _adapterStopSignal.Set();
        }

        private void PrepareToResume()
        {
        }

        private void PrepareToStop(PointEvent<StockQuote> currEvent, DequeueOperationResult result)
        {
            if (result == DequeueOperationResult.Success)
            {
                ReleaseEvent(ref currEvent);
            }
        }

        /// <summary>
        /// Used for debugging purposes
        /// </summary>
        /// <param name="evt"></param>
        private void PrintEvent(PointEvent<StockQuote> evt)
        {
            if (evt.EventKind == EventKind.Cti)
            {
                //Console.WriteLine("Output: CTI " + evt.StartTime);
            }
            else
            {
                Console.WriteLine("Output: " + evt.EventKind + " " +
                    evt.StartTime + " " + evt.Payload.StockID + " " +
                    evt.Payload.Value);
            }
        }
    }
}
