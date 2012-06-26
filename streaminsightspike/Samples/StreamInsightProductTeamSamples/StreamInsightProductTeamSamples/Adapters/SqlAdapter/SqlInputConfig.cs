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

namespace StreamInsight.Samples.Adapters.Sql
{
    using System;
    using System.Collections.Generic;

    /// <summary> Configuration structure for the SQL Input Adapter </summary>
    public struct SqlInputConfig
    {
        public string ConnectionString { get; set; }            // DB Connection String

        public string Statement { get; set; }                   // SQL statement that presents data ORDER BY time (this is important)

        public string StartTimeColumnName { get; set; }          // name of the StartTimestamp column (mandatory for all event shapes - Point and Interval)

        public string EndTimeColumnName { get; set; }            // name of the EndTimestamp column (mandatory for Interval events - ignored for Point)

        public string CultureName { get; set; }                 // Input file culture. Important for reading strings correctly

        public static bool operator ==(SqlInputConfig config1, SqlInputConfig config2)
        {
            return config1.Equals(config2);
        }

        public static bool operator !=(SqlInputConfig config1, SqlInputConfig config2)
        {
            return config1.Equals(config2);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
