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

    /// <summary>
    /// Facade over point output adapter proxy.
    /// </summary>
    public sealed class ClientPointOutputAdapter : ClientAdapter<IPointOutputAdapter>
    {
        private WcfPointEvent currentEvent;

        public ClientPointOutputAdapter(Uri address)
            : base(address)
        {
        }

        /// <summary>
        /// Dequeues an event. Returns false if the adapter has stopped.
        /// </summary>
        public bool TryDequeueEvent(out WcfPointEvent wcfPointEvent)
        {
            // The result of the dequeue task is stored in the "currentEvent"
            // field by the "TryRunTask" method.
            if (RunTask())
            {
                wcfPointEvent = this.currentEvent;
                return true;
            }
            wcfPointEvent = default(WcfPointEvent);
            return false;
        }

        protected override ResultCode TryRunTask()
        {
            return AdapterChannel.DequeueEvent(out this.currentEvent);
        }
    }
}
