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

namespace StreamInsight.Samples.Adapters.Wcf
{
    using System;
    using System.Linq;
    using System.ServiceModel;
    using System.Threading;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    /// <summary>
    /// Implementation of adapter contract and service contract for point event outputs.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    internal sealed class WcfPointOutputAdapter : PointOutputAdapter, IPointOutputAdapter
    {
        private static readonly int StopPollingPeriod = 1000; // msec.
        private readonly CepEventType eventType;
        private readonly Timer timer;
        private readonly object sync;
        private readonly ServiceHost host;

        public WcfPointOutputAdapter(CepEventType eventType, Uri address)
        {
            this.eventType = eventType;
            this.sync = new object();

            // Initialize the service host. The host is opened and closed as the adapter is started
            // and stopped.
            this.host = new ServiceHost(this);
            host.AddServiceEndpoint(typeof(IPointOutputAdapter), new WSHttpBinding(), address);

            // Poll the adapter to determine when it is time to stop.
            this.timer = new Timer(CheckStopping);
            this.timer.Change(StopPollingPeriod, Timeout.Infinite);
        }

        public override void Start()
        {
            this.host.Open();
        }

        public override void Resume()
        {
        }

        private void CheckStopping(object state)
        {
            lock (this.sync)
            {
                if (AdapterState == AdapterState.Stopping)
                {
                    this.host.Close();
                    Stopped();
                }
                else
                {
                    // Check again after the polling period has elapsed.
                    this.timer.Change(StopPollingPeriod, Timeout.Infinite);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((IDisposable)this.host).Dispose();
            }
            base.Dispose(disposing);
        }

        ResultCode IPointOutputAdapter.DequeueEvent(out WcfPointEvent result)
        {
            lock (sync)
            {
                result = default(WcfPointEvent);
                switch (AdapterState)
                {
                    case AdapterState.Created:
                    case AdapterState.Suspended:
                        return ResultCode.Suspended;
                    case AdapterState.Stopping:
                    case AdapterState.Stopped:
                        return ResultCode.Stopped;
                }

                PointEvent pointEvent = null;
                try
                {
                    if (Dequeue(out pointEvent) == DequeueOperationResult.Empty)
                    {
                        Ready();
                        return ResultCode.Suspended;
                    }
                    else
                    {
                        result = new WcfPointEvent
                        {
                            IsInsert = pointEvent.EventKind == EventKind.Insert,
                            StartTime = pointEvent.StartTime,
                        };
                        if (result.IsInsert)
                        {
                            // Extract all field values to generate the payload.
                            result.Payload = this.eventType.Fields.Values.ToDictionary(
                                f => f.Name,
                                f => pointEvent.GetField(f.Ordinal));
                        }
                        return ResultCode.Success;
                    }
                }
                finally
                {
                    if (null != pointEvent)
                    {
                        ReleaseEvent(ref pointEvent);
                    }
                }
            }
        }
    }
}