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
    using System.IO;

    /// <summary>
    /// Router for various tracers.
    /// </summary>
    internal static class TracerRouter
    {
        /// <summary>
        /// Gets a print handler based on a trace type.
        /// </summary>
        /// <param name="tracerKind">Enum value that identifies the trace type.</param>
        /// <param name="traceName">Name of the trace.</param>
        /// <returns>Print handler to write to.</returns>
        public static Action<string> GetHandler(TracerKind tracerKind, string traceName)
        {
            switch (tracerKind)
            {
                case TracerKind.Console:
                    return Console.WriteLine;
                case TracerKind.Debug:
                    return WriteMessageToDebug;
                case TracerKind.File:
                    {
                        FileTracer ft = new FileTracer(traceName);
                        return ft.WriteLine;
                    }

                case TracerKind.Trace:
                default:
                    return WriteMessageToTrace;
            }
        }
     
        /// <summary>
        /// Delegate to write a line to a tracer.
        /// </summary>
        /// <param name="msg">Line to write.</param>
        private static void WriteMessageToTrace(string msg)
        {
            System.Diagnostics.Trace.WriteLine(msg);
            System.Diagnostics.Trace.Flush();
        }

        /// <summary>
        /// Delegate to write a line to a debug stream.
        /// </summary>
        /// <param name="msg">Line to write.</param>
        private static void WriteMessageToDebug(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        /// <summary>
        /// Tracer to write into a file.
        /// </summary>
        private sealed class FileTracer
        {
            /// <summary>
            /// File name to write.
            /// </summary>
            private readonly string fileName;

            /// <summary>
            /// Initializes a new instance of the FileTracer class.
            /// </summary>
            /// <param name="fileName">File name.</param>
            public FileTracer(string fileName)
            {
                this.fileName = fileName;
            }

            /// <summary>
            /// Writes a line to the file.
            /// </summary>
            /// <param name="msg">Line to write.</param>
            public void WriteLine(string msg)
            {
                using (TextWriter tw = new StreamWriter(this.fileName, true))
                {
                    tw.WriteLine(msg);
                }
            }
        }
    }
}
