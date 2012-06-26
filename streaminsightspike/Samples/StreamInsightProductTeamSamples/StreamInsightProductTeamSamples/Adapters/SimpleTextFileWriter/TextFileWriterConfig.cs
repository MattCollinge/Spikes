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

namespace StreamInsight.Samples.Adapters.SimpleTextFileWriter
{
    /// <summary>
    /// Configuration structure for the SimpleTextFileWriter.
    /// </summary>
    public class TextFileWriterConfig
    {
        /// <summary>
        /// Gets or sets the file name for output file. Adapter writes to console if null.
        /// </summary>
        public string OutputFileName { get; set; }

        /// <summary>
        /// Gets or sets the delimiter to use between payload fields.
        /// </summary>
        public char Delimiter { get; set; }
    }
}
