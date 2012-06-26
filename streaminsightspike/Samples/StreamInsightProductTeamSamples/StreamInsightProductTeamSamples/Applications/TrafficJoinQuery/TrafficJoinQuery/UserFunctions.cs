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
    /// Set of user-defined functions.
    /// </summary>
    public static class UserFunctions
    {
        /// <summary>
        /// Simple example of a user-defined function: look up a specific 
        /// threshold based on a real-time payload. This function can directly
        /// be used in a LINQ query template (for instance, in a filter,
        /// project, or grouping).
        /// </summary>
        /// <param name="location">Sensor location.</param>
        /// <returns>Average vehicle throughput threshold in the specified
        /// location.</returns>
        public static int LocationCountThreshold(int location)
        {
            switch (location)
            {
                case 1:
                    return 15;
                case 2:
                    return 11;
                case 3:
                    return 18;
                default:
                    return 0;
            }
        }
    }
}