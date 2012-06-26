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
    using System.Globalization;

    /// <summary>
    /// Event type for the generator adapters.
    /// </summary>
    public class GeneratedEvent
    {
        /// <summary>
        /// Initializes a new instance of the GeneratedEvent class.
        /// </summary>
        /// <param name="deviceCount">Number of devices to simulate.</param>
        /// <param name="maxValue">Maximum value for the 'Value' payload field.</param>
        public GeneratedEvent(uint deviceCount, uint maxValue)
        {
            this.DeviceId = RandomGenerator.GetRandomNumber((int)deviceCount).ToString(CultureInfo.InvariantCulture.NumberFormat);
            this.Value = RandomGenerator.GetRandomNumber((int)maxValue);
        }

        /// <summary>
        /// Gets or sets the device ID.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the simulated measurement value.
        /// </summary>
        public float Value { get; set; }
    }
}
