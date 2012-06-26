// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.IO;
using System.IO.Pipes;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace AdvantIQ.ExampleAdapters.Output.WinFormStacker
{
    /// <summary>
    /// Simple output adapter that writes quotes to console
    /// </summary>
    public class StackerIntervalOutput<T> : TypedIntervalOutputAdapter<T>
    {
        private readonly PropertyInfo[] propertyInfos = typeof(T).GetProperties();
        protected bool stopFlag = false;
        private IStackerCtl stackerCtl;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Adapter configuration</param>
        public StackerIntervalOutput(StackerConfig config)
        {
            ChannelFactory<IStackerCtl> stackerCtlFactory =
                new ChannelFactory<IStackerCtl>(new NetNamedPipeBinding(),
                    new EndpointAddress("net.pipe://" + config.StackerCtlHostName + "/" + config.StackerCtlPipeName));
            stackerCtl = stackerCtlFactory.CreateChannel();
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
            IntervalEvent<T> currEvent;
            DequeueOperationResult result;

            try
            {
                // Run until stop state
                while (AdapterState == AdapterState.Running && !stopFlag)
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
                        // Write to output control
                        if (currEvent.EventKind == EventKind.Insert)
                        {
                            var sb = new StringBuilder();
                            sb.AppendFormat("[{0} - {1}] ", currEvent.StartTime, currEvent.EndTime);
                            var sep = "";

                            foreach (var propertyInfo in propertyInfos)
                            {
                                sb.Append(sep);
                                var value = propertyInfo.GetValue(currEvent.Payload, null);
                                if (value != null)
                                    sb.Append(value.ToString());
                                sep = ", ";
                            }

                            Push(sb.ToString());
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
                //Push("AdvantIQ.StockInsightTypedPointOutput.ConsumeEvents - " + e.Message + e.StackTrace + "\r\n");
            }
        }

        private void PrepareToResume()
        {
        }

        private void PrepareToStop(IntervalEvent<T> currEvent, DequeueOperationResult result)
        {
            if (result == DequeueOperationResult.Success)
            {
                ReleaseEvent(ref currEvent);
            }
        }

        private void Push(string msg)
        {
            try
            {
                stackerCtl.Push(msg);
            }
            catch
            {
                stopFlag = true;
            }
        }
    }
}
