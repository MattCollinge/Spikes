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
    /// <summary>
    /// This is the configuration type for the GeneratorFactory. Use instances
    /// of this class to configure the data and frequency of a generator adapter.
    /// </summary>
    public class GeneratorConfig
    {
        /// <summary>
        /// Gets or sets how often to send a CTI event (1 = send a CTI event with every data event).
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cti", Justification = "StreamInsight specific terminology")]
        public uint CtiFrequency { get; set; }

        /// <summary>
        /// Gets or sets the interval between events in ms.
        /// </summary>
        public uint EventInterval { get; set; }

        /// <summary>
        /// Gets or sets the maximum deviation from the interval in ms.
        /// </summary>
        public int EventIntervalVariance { get; set; }

        /// <summary>
        /// Gets or sets the number of devices to simulate.
        /// </summary>
        public uint DeviceCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum value for the 'Value' payload field.
        /// </summary>
        public uint MaxValue { get; set; }
    }
}
