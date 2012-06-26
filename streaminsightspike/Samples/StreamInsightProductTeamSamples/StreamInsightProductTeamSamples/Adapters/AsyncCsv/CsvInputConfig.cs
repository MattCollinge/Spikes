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

namespace StreamInsight.Samples.Adapters.AsyncCsv
{
    using System;
    using System.Collections.Generic;

    /// <summary> Configuration structure for the CSV Input Adapter </summary>
    public struct CsvInputConfig
    {
        public string InputFileName { get; set; }               // Input file name
        public int BufferSize { get; set; }                     // Async read buffer size in bytes - 0 for synchronous ReadLine
        public char[] Delimiter { get; set; }                   // List of delimiters separating the values in each line

        public string CultureName { get; set; }                 // Input file culture. Important for reading timestamps correctly.
        public IList<string> Fields { get; set; }               // Field names in the order in which they are expected to be processed
        public ushort CtiFrequency { get; set; }                // the frequency with which CTI needs to be enqueued, in terms of number of input events

        // The input may have different schemas, and placement of the timestamp values. You can use the following fields
        // to make the adapter implementation to handle different input formats.
        public int NonPayloadFieldCount { get; set; }           // count of non-payload fields in the input file
        public int StartTimePos { get; set; }                   // position of StartTime field
        public int EndTimePos { get; set; }                     // position of EndTime field (zero implies non-existent)

        public static bool operator ==(CsvInputConfig config1, CsvInputConfig config2)
        {
            return config1.Equals(config2);
        }

        public static bool operator !=(CsvInputConfig config1, CsvInputConfig config2)
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
