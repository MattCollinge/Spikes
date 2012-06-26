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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.ComplexEventProcessing;

    /// <summary>
    /// Facade over point input adapter proxy.
    /// </summary>
    public sealed class ClientPointInputAdapter : ClientAdapter<IPointInputAdapter>
    {
        private WcfPointEvent currentEvent;

        public ClientPointInputAdapter(Uri address)
            : base(address)
        {
        }

        /// <summary>
        /// Enqueues an insert event. Returns false if the adapter has stopped.
        /// </summary>
        public bool TryEnqueueInsertEvent(DateTimeOffset startTime, Dictionary<string, object> payload)
        {
            // The event stored in the "currentEvent" field is used within the TryRunTask
            // method.
            this.currentEvent = new WcfPointEvent
                {
                    IsInsert = true,
                    StartTime = startTime,
                    Payload = payload,
                };
            return RunTask();
        }

        /// <summary>
        /// Enqueues a CTI event. Returns false if the adapter has stopped.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cti")]
        public bool TryEnqueueCtiEvent(DateTimeOffset startTime)
        {
            this.currentEvent = new WcfPointEvent
                {
                    StartTime = startTime,
                };
            return RunTask();
        }

        protected override ResultCode TryRunTask()
        {
            return AdapterChannel.EnqueueEvent(this.currentEvent);
        }
    }
}