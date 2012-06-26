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

    /// <summary>
    /// This class represents the actual data source. The data source generates
    /// new events at certain points in time and can push these events into
    /// a callback function. This simulates the functionality of a data source
    /// that is able to push new data, as opposed to having to pull the data
    /// from the outside.
    /// <para/>
    /// The event type of the simulated data source is fixed. The payload is
    /// randomly generated according to the runtime configuration. The
    /// frequency of the generation is determined by configuration parameters
    /// as well.
    /// </summary>
    internal sealed class DataSource : IDisposable
    {
        /// <summary>
        /// Period at which to produce a new data item.
        /// </summary>
        private uint period;

        /// <summary>
        /// Maximum variance to deviate from the exact period.
        /// </summary>
        private int variance;

        /// <summary>
        /// Number of sources to simulate.
        /// </summary>
        private uint numberOfSources;

        /// <summary>
        /// Maximum value of each data item.
        /// </summary>
        private uint maxValue;

        /// <summary>
        /// Timer used to trigger the generation of new data items.
        /// </summary>
        private System.Timers.Timer timer;

        /// <summary>
        /// Mutex for the timer-triggered data generation.
        /// </summary>
        private object thisLock = new object();

        /// <summary>
        /// Initializes a new instance of the DataSource class.
        /// </summary>
        /// <param name="sources">Number of sources to simulate for the 'SourceID' field.</param>
        /// <param name="maxValue">Maximum value of the 'Value' field.</param>
        /// <param name="period">Period at which to produce a new data item.</param>
        /// <param name="variance">Maximum variance to deviate from the exact period.</param>
        public DataSource(uint sources, uint maxValue, uint period, int variance)
        {
            this.numberOfSources = sources;
            this.maxValue = maxValue;
            this.period = period;
            this.variance = variance;

            // Initalize timer.
            this.timer = new System.Timers.Timer(period);
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.ProduceData);
            this.timer.AutoReset = true;
        }

        /// <summary>
        /// Callback type to push new data to.
        /// </summary>
        /// <param name="data">New data item to push to the callback.</param>
        /// <param name="timestamp">Timestamp of the data.</param>
        public delegate void EventCallback(GeneratedEvent data, DateTimeOffset timestamp);

        /// <summary>
        /// Gets or sets the callback.
        /// </summary>
        public EventCallback Callback { get; set; }

        /// <summary>
        /// Starts the data generation.
        /// </summary>
        public void Start()
        {
            this.timer.Enabled = true;
        }

        /// <summary>
        /// Stops the data generation.
        /// </summary>
        public void Stop()
        {
            this.timer.Enabled = false;
        }

        /// <summary>
        /// Dispose method.
        /// </summary>
        public void Dispose()
        {
            this.timer.Dispose();
        }
        
        /// <summary>
        /// Generates a new data item and calls the callback.
        /// </summary>
        /// <param name="sender">Event origin.</param>
        /// <param name="e">Event arguments.</param>
        private void ProduceData(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (this.thisLock)
            {
                // Check whether this timer is still active.
                if (!this.timer.Enabled)
                {
                    return;
                }

                // Set a new timer interval. Random number bound by
                // period - variance and period + variance. Must be positive.
                this.timer.Interval = Math.Max((int)this.period - (RandomGenerator.GetRandomNumber((int)this.variance * 2) - this.variance), 1);

                // push the data into the callback, with current time.
                this.Callback(new GeneratedEvent(this.numberOfSources, this.maxValue), DateTime.Now);
            }
        }
    }
}
