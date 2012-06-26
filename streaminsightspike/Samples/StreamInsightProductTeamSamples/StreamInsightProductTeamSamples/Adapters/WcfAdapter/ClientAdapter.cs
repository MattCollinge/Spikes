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
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Threading;

    /// <summary>
    /// A facade over a WCF exposed input or output adapter proxy. Handles retry logic.
    /// </summary>
    /// <typeparam name="TAdapterChannel">The type of the adapter channel.</typeparam>
    public abstract class ClientAdapter<TAdapterChannel>
    {
        /// <summary>
        /// Length of time between calls to a suspended provider.
        /// </summary>
        private static readonly TimeSpan PollingPeriod = TimeSpan.FromMilliseconds(50);

        /// <summary>
        /// Maximum number of retries before it is assumed that the adapter has stopped.
        /// </summary>
        private const int MaxRetryCount = 10;

        /// <summary>
        /// Channel of communication with the adapter.
        /// </summary>
        private readonly TAdapterChannel adapterChannel;

        /// <summary>
        /// Indicates whether the adapter has stopped.
        /// </summary>
        private bool stopped;

        protected ClientAdapter(Uri address)
        {
            if (null == address)
            {
                throw new ArgumentNullException("address");
            }

            this.adapterChannel = ChannelFactory<TAdapterChannel>.CreateChannel(new WSHttpBinding(), new EndpointAddress(address));
        }

        /// <summary>
        /// Gets the client proxy for the input or output adapter.
        /// </summary>
        protected TAdapterChannel AdapterChannel
        {
            get { return this.adapterChannel; }
        }

        /// <summary>
        /// Gets a value indicating whether future operations can return anything.
        /// </summary>
        public bool Stopped
        {
            get { return this.stopped; }
        }

        /// <summary>
        /// Runs the current task against the channel wrapped by this facade. If the targeted adapter
        /// is suspended or there is a failure communicating with the endpoint, the task is retried
        /// a fixed number of times. If it is not possible to complete the task, we assume the 
        /// adapter has stopped and return false.
        /// </summary>
        protected bool RunTask()
        {
            if (this.stopped)
            {
                throw new InvalidOperationException("Adapter is stopped.");
            }

            int retryCount = 0;
            while (true)
            {
                if (retryCount > MaxRetryCount)
                {
                    this.stopped = true;
                    return false;
                }

                try
                {
                    ResultCode resultCode = TryRunTask();
                    switch (resultCode)
                    {
                        case ResultCode.Stopped:
                            this.stopped = true;
                            return false;
                        case ResultCode.Success:
                            return true;
                        case ResultCode.Suspended:
                            // Retry later.
                            break;
                    }
                }
                catch (CommunicationObjectAbortedException e)
                {
                    // It may be possible to reconnect when this exception occurs.
                    Trace.WriteLine(e);
                    this.stopped = true;
                    return false;
                }
                catch (CommunicationObjectFaultedException e)
                {
                    // It may be possible to reconnect when this exception occurs.
                    Trace.WriteLine(e);
                    this.stopped = true;
                    return false;
                }
                catch (CommunicationException e)
                {
                    // Retry later.
                    Trace.WriteLine(e);
                }
                catch (TimeoutException e)
                {
                    // Retry later.
                    Trace.WriteLine(e);
                }

                Thread.Sleep(PollingPeriod);
                retryCount++;
            }
        }

        /// <summary>
        /// Try running the current client adapter task.
        /// </summary>
        /// <returns>Result code indicating success, a suspended or a stopped adapter.</returns>
        protected abstract ResultCode TryRunTask();
    }
}
