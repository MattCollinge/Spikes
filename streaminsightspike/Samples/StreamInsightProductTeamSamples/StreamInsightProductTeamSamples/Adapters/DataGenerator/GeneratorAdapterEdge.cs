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
    using System.Collections.Generic;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    /// <summary>
    /// Edge version of the data generator input adapter.
    /// <para/>
    /// The edge input adapter interprets each data source as a separate signal,
    /// enqueueing a start edge for a new value, after ending the previous value
    /// with an end edge. Since an end edge needs to be matched with the
    /// corresponding start edge through the entire payload, the adapter needs
    /// to maintain the last payload for each data source until the next value.
    /// </summary>
    public class GeneratorAdapterEdge : TypedEdgeInputAdapter<GeneratedEvent>
    {
        /// <summary>
        /// State of the entire data source.
        /// </summary>
        private Dictionary<string, DeviceState> dataSourceState;

        /// <summary>
        /// Event source.
        /// </summary>
        private DataSource dataSource;

        /// <summary>
        /// Initializes a new instance of the GeneratorAdapterEdge class.
        /// </summary>
        /// <param name="config">The configuration passed from the adapter factory.</param>
        public GeneratorAdapterEdge(GeneratorConfig config)
        {
            this.dataSource = new DataSource(config.DeviceCount, config.MaxValue, config.EventInterval, config.EventIntervalVariance);

            this.dataSource.Callback = new DataSource.EventCallback(this.ProduceEvent);

            this.dataSourceState = new Dictionary<string, DeviceState>((int)config.DeviceCount);
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
        /// This method enqueues one end and one start edge event.
        /// It is used as the callback for the actual data source.
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

            EdgeEvent<GeneratedEvent> currentEvent;

            // Did we have a previous value for the data source with this ID?
            // If so, then we need to send the end edge for a previous start edge.
            if (this.dataSourceState.ContainsKey(data.DeviceId))
            {
                currentEvent = CreateInsertEvent(EdgeType.End);

                if (null == currentEvent)
                {
                    // We just went out of the running state - throw away the
                    // current event. We will try to send the end edge with
                    // the next incoming data.
                    return;
                }

                // Create the end event for the previous start event of this
                // data source. Start time and data are stored, end time is the
                // new event's time.
                currentEvent.StartTime = this.dataSourceState[data.DeviceId].Timestamp;
                currentEvent.Payload = this.dataSourceState[data.DeviceId].Data;
                currentEvent.EndTime = timestamp;

                // Try to enqueue. If it fails, we will just ignore the current
                // event and try to send the proper end edge the next time.
                if (EnqueueOperationResult.Full == Enqueue(ref currentEvent))
                {
                    ReleaseEvent(ref currentEvent);
                    Ready();
                    return;
                }

                // We don't need this state anymore.
                this.dataSourceState.Remove(data.DeviceId);
            }

            // Now enqueue the new data as a start edge.
            currentEvent = CreateInsertEvent(EdgeType.Start);

            if (null == currentEvent)
            {
                // Throw away the current event. At this point, there is no
                // previous event stored for this data source, so we can try
                // a new start edge next time.
                return;
            }

            currentEvent.Payload = data;
            currentEvent.StartTime = timestamp;

            if (EnqueueOperationResult.Full == Enqueue(ref currentEvent))
            {
                ReleaseEvent(ref currentEvent);
                Ready();

                // Throw away the current event. At this point, there is no
                // previous event stored for this data source, so we can try
                // a new start edge next time.
                return;
            }

            // If we arrived here, we did enqueue a start edge. Now we need
            // to remember it for the next round, when the corresponding end
            // edge will be enqueued.
            this.dataSourceState.Add(data.DeviceId, new DeviceState(data, timestamp));
        }

        /// <summary>
        /// State of one device. Contains the device data and the corresponding
        /// timestamp.
        /// </summary>
        private class DeviceState
        {
            /// <summary>
            /// Stored payload.
            /// </summary>
            private GeneratedEvent data;

            /// <summary>
            /// Stored timestamp.
            /// </summary>
            private DateTimeOffset timestamp;

            /// <summary>
            /// Initializes a new instance of the DeviceState class.
            /// </summary>
            /// <param name="data">Payload to store.</param>
            /// <param name="timestamp">Timestamp to store.</param>
            public DeviceState(GeneratedEvent data, DateTimeOffset timestamp)
            {
                this.data = data;
                this.timestamp = timestamp;
            }

            /// <summary>
            /// Gets the payload of the stored event.
            /// </summary>
            public GeneratedEvent Data
            {
                get { return this.data; }
            }

            /// <summary>
            /// Gets the timestamp of the stored event.
            /// </summary>
            public DateTimeOffset Timestamp
            {
                get { return this.timestamp; }
            }
        }
    }
}
