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
    using System.Globalization;
    using System.Text;
    using Microsoft.ComplexEventProcessing;
    
    /// <summary>
    /// Manages tracing of event data.
    /// </summary>
    internal sealed class Tracer
    {
        /// <summary>
        /// Delegate to call when tracing.
        /// </summary>
        private readonly Action<string> trace;

        /// <summary>
        /// Tracer configuration from the query binding.
        /// </summary>
        private readonly TracerConfig config;

        /// <summary>
        /// Event type passed from the query binding.
        /// </summary>
        private readonly CepEventType type;

        /// <summary>
        /// Initializes a new instance of the Tracer class.
        /// </summary>
        /// <param name="config">Adapter configuration passed from the query binding.</param>
        /// <param name="type">Event type passed from the query binding.</param>
        public Tracer(TracerConfig config, CepEventType type)
        {
            this.trace = TracerRouter.GetHandler(config.TracerKind, config.TraceName);
            this.config = config;
            this.type = type;
        }

        /// <summary>
        /// Traces an insert event.
        /// </summary>
        /// <param name="evt">Event to trace.</param>
        /// <param name="prefix">String prefix to use when tracing the event.</param>
        public void TraceInsert(UntypedEvent evt, string prefix)
        {
            StringBuilder builder = new StringBuilder()
                .Append(this.config.TraceName)
                .Append(": ")
                .Append(prefix);

            if (evt.EventKind != EventKind.Insert)
            {
                throw new InvalidOperationException();
            }

            for (int ordinal = 0; ordinal < this.type.Fields.Count; ordinal++)
            {
                if (this.config.SingleLine)
                {
                    builder.Append(" ");
                }
                else
                {
                    builder
                        .AppendLine()
                        .Append('\t')
                        .Append(this.type.FieldsByOrdinal[ordinal].Name)
                        .Append(" = ");
                }

                object value = evt.GetField(ordinal) ?? "NULL";
                builder.Append(String.Format(CultureInfo.InvariantCulture, "{0}", value));
            }

            this.trace(builder.ToString());
        }

        /// <summary>
        /// Traces a CTI.
        /// </summary>
        /// <param name="time">Timestamp of the CTI to be traced.</param>
        public void TraceCti(DateTimeOffset time)
        {
            if (this.config.DisplayCtiEvents)
            {
                this.trace(String.Format(CultureInfo.InvariantCulture, "{0}: CTI at {1}", this.config.TraceName, time));
            }
        }
    }
}