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

    /// <summary> Configuration structure for the SQL Output Adapter </summary>
    public struct SqlOutputConfig
    {
        public string ConnectionString { get; set; }            // DB Connection String

        public string Statement { get; set; }                   // SQL statement that INSERTs rows into a pre-existing table with the same schema as the event type

        public string TableName { get; set; }                   // Table name into which the output data is populated

        public string StartTimeColumnName { get; set; }          // name of the StartTimestamp column

        public string EndTimeColumnName { get; set; }            // name of the EndTimestamp column

        public static bool operator ==(SqlOutputConfig config1, SqlOutputConfig config2)
        {
            return config1.Equals(config2);
        }

        public static bool operator !=(SqlOutputConfig config1, SqlOutputConfig config2)
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
