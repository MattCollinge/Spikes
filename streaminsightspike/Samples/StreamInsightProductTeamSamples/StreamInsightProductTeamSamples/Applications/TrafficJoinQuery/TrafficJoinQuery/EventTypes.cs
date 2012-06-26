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

namespace StreamInsight.Samples.TrafficJoinQuery
{
    /// <summary>
    /// Sensor Payload, containing the sensor ID, average speed,
    /// and vehicular count over time period.
    /// <para/>
    /// Event Types can be structs or classes.
    /// </summary>
    public class SensorReading
    {
        /// <summary>
        /// Gets or sets the Sensor ID.
        /// </summary>
        public int SensorId { get; set; }

        /// <summary>
        /// Gets or sets the average speed of all vehicles measured over a
        /// short period.
        /// </summary>
        public int AverageSpeed { get; set; }

        /// <summary>
        /// Gets or sets the vehicle count measured over a short period.
        /// </summary>
        public int VehicularCount { get; set; }
    }

    /// <summary>
    /// Location Payload, containing sensor ID and location ID.
    /// </summary>
    public class LocationData
    {
        /// <summary>
        /// Gets or sets the Sensor ID.
        /// </summary>
        public int SensorId { get; set; }

        /// <summary>
        /// Gets or sets the Location ID.
        /// </summary>
        public int LocationId { get; set; }
    }
}
