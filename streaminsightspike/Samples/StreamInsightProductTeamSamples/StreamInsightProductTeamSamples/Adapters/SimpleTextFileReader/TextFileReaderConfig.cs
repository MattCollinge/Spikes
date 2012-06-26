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

namespace StreamInsight.Samples.Adapters.SimpleTextFileReader
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Configuration structure for the SimpleTextFileReader.
    /// </summary>
    public class TextFileReaderConfig
    {
        /// <summary>
        /// Gets or sets the filename to read from.
        /// </summary>
        public string InputFileName { get; set; }

        /// <summary>
        /// Gets or sets the character between values in a line.
        /// </summary>
        public char Delimiter { get; set; }

        /// <summary>
        /// Gets or sets the frequency (event count) of CTIs to be sent by the adapter.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cti", Justification = "StreamInsight specific terminology")]
        public uint CtiFrequency { get; set; }

        /// <summary>
        /// Gets or sets the input file culture. Important for reading timestamps correctly.
        /// </summary>
        public string CultureName { get; set; }

        /// <summary>
        /// Gets or sets the order of the values w.r.t. the event type. For each read line in
        /// the CSV file, the type's fields will be assigned in the order they
        /// appear in the list.
        /// </summary>
        public Collection<string> InputFieldOrders { get; set; }
    }
}
