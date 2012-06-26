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

    /// <summary> Configuration structure for the CSV Output Adapter </summary>
    public struct CsvOutputConfig
    {
        public string OutputFileName { get; set; } // Input file name (NULL implies Console output)
        public string[] Delimiter { get; set; }    // String separating the values in a line.
        public string CultureName { get; set; }    // Input file culture. Important for reading timestamps correctly.
        public IList<string> Fields { get; set; }  // Order of the values with respect to the event type.
        public static bool operator ==(CsvOutputConfig config1, CsvOutputConfig config2)
        {
            return config1.Equals(config2);
        }

        public static bool operator !=(CsvOutputConfig config1, CsvOutputConfig config2)
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
