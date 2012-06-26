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

namespace StreamInsight.Samples.Adapters.DataGenerator
{
    using System;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    /// <summary>
    /// Point version of the data generator input adapter.
    /// <para/>
    /// The point input adapter produces point events, corresponding to the 
    /// timestamp received from the data source.
    /// </summary>
    public class GeneratorAdapterPoint : TypedPointInputAdapter<GeneratedEvent>
    {
        /// <summary>
        /// Event source.
        /// </summary>
        private DataSource dataSource;

        /// <summary>
        /// Initializes a new instance of the GeneratorAdapterPoint class.
        /// </summary>
        /// <param name="config">The configuration passed from the adapter factory.</param>
        public GeneratorAdapterPoint(GeneratorConfig config)
        {
            this.dataSource = new DataSource(config.DeviceCount, config.MaxValue, config.EventInterval, config.EventIntervalVariance);

            this.dataSource.Callback = new DataSource.EventCallback(this.ProduceEvent);
        }

        /// <summary>
        /// The engine will call the Start() method upon starting the adapter. This is where
        /// the event enqueueing is supposed to be kicked off.
        /// </summary>
        public override void Start()
        {
            this.dataSource.Start();
        }

        /// <summary>
        /// The engine will call the Resume() method when resuming the suspended
        /// adapter. 
        /// </summary>
        public override void Resume()
        {
            this.dataSource.Start();
        }

        /// <summary>
        /// This method enqueues one event. It is used as the callback for the
        /// actual data source.
        /// </summary>
        /// <param name="data">Data item to be enqueued as event.</param>
        /// <param name="timestamp">Timestamp of the data</param>
        private void ProduceEvent(GeneratedEvent data, DateTimeOffset timestamp)
        {
            // Check for the stopping state and stop if necessary.
            if (AdapterState.Stopping == AdapterState)
            {
                this.dataSource.Stop();
                Stopped();
                return;
            }

            // Since this method is called from a thread that is independend of
            // Start() and Resume(), we need to make sure that the adapter is
            // actually running.
            if (AdapterState.Running != AdapterState)
            {
                // Throw away the current event.
                return;
            }

            PointEvent<GeneratedEvent> currEvent = CreateInsertEvent();
            if (null == currEvent)
            {
                // Throw away the current event.
                return;
            }

            currEvent.StartTime = timestamp;

            currEvent.Payload = data;

            if (EnqueueOperationResult.Full == Enqueue(ref currEvent))
            {
                ReleaseEvent(ref currEvent);
                Ready();
                return;
            }
        }
    }
}
