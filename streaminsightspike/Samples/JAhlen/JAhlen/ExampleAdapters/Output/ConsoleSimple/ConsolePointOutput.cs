// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace AdvantIQ.ExampleAdapters.Output.ConsoleSimple
{
    /// <summary>
    /// Simple output adapter that writes quotes to console
    /// </summary>
    public class ConsolePointOutput<T> : TypedPointOutputAdapter<T>
    {
        private readonly FieldInfo[] fieldInfos = typeof(T).GetFields();
        private readonly PropertyInfo[] propertyInfos = typeof(T).GetProperties();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Adapter configuration</param>
        public ConsolePointOutput(ConsoleConfig config)
        {
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
            PointEvent<T> currEvent;
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
                        // Write to console
                        if (currEvent.EventKind == EventKind.Insert)
                        {
                            Console.Write("[{0}] ", currEvent.StartTime);
                            var sep = "";

                            foreach (var propertyInfo in propertyInfos)
                            {
                                Console.Write(sep);
                                var value = propertyInfo.GetValue(currEvent.Payload, null);
                                if (value != null)
                                    Console.Write(value.ToString());
                                sep = ", ";
                            }
                            foreach (var fieldInfo in fieldInfos)
                            {
                                Console.Write(sep);
                                var value = fieldInfo.GetValue(currEvent.Payload);
                                if (value != null)
                                    Console.Write(value.ToString());
                                sep = ", ";
                            }
                            Console.WriteLine();
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
        }

        private void PrepareToResume()
        {
        }

        private void PrepareToStop(PointEvent<T> currEvent, DequeueOperationResult result)
        {
            if (result == DequeueOperationResult.Success)
            {
                ReleaseEvent(ref currEvent);
            }
        }
    }
}
