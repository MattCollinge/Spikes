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

namespace StreamInsight.Samples.Adapters.OutputTracer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Possible trace types.
    /// </summary>
    public enum TracerKind
    {
        /// <summary>
        /// Write messages to a tracer.
        /// </summary>
        Trace,

        /// <summary>
        /// Write messages to the debug output.
        /// </summary>
        Debug,

        /// <summary>
        /// Write messages to the console.
        /// </summary>
        Console,

        /// <summary>
        /// Write messages to a file.
        /// </summary>
        File
    }

    /// <summary>
    /// Configuration structure for tracer output adapters.
    /// </summary>
    public class TracerConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not to display CTI events in the output stream.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cti", Justification = "StreamInsight specific terminology")]
        public bool DisplayCtiEvents { get; set; }

        /// <summary>
        /// Gets or sets a value indicating which output stream to use.
        /// </summary>
        public TracerKind TracerKind { get; set; }

        /// <summary>
        /// Gets or sets the trace name. Will be used as an identifier when
        /// displaying events and as filename for file streams,
        /// </summary>
        public string TraceName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to output each event in
        /// a single line or use a more verbose output format.
        /// </summary>
        public bool SingleLine { get; set; }
    }
}
