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

namespace StreamInsight.Samples.SqlApplication
{
    using System;

    /// <summary>
    /// SQL input payload - maps to fields of [AdventureWorks].SalesOrderHeader
    /// See here (http://msdn.microsoft.com/en-us/library/ms124879.aspx)
    /// OrderDate corresponds to the event's StartTime, ShipDate corresponds to event's EndTime
    /// <para/>
    /// Event Types can be structs or classes.
    /// </summary>
    public class SqlInput
    {
        /// <summary>
        /// Gets or sets the primary key - sales order id
        /// </summary>
        public int SalesOrderID { get; set; }

        /// <summary>
        /// Gets or sets the territory id
        /// </summary>
        public int TerritoryID { get; set; }
    }

    /// <summary>
    /// SQL output payload - maps to fields of [AdventureWorks].SalesOrderHeader
    /// See here (http://msdn.microsoft.com/en-us/library/ms124879.aspx)
    /// OrderDate corresponds to the event's StartTime, ShipDate corresponds to event's EndTime
    /// <para/>
    /// Event Types can be structs or classes.
    /// </summary>
    public class SqlOutput
    {
        /// <summary>
        /// Gets or sets the primary key - sales order id
        /// </summary>
        public int OrderCount { get; set; }

        /// <summary>
        /// Gets or sets the territory id
        /// </summary>
        public int TerritoryID { get; set; }
    }
}
