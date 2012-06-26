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
    using System.Globalization;
    using System.Text;
    using Microsoft.ComplexEventProcessing;

    /// <summary>
    /// Common routines for all text file writer adapters.
    /// </summary>
    internal sealed class TextFileWriterCommon
    {
        /// <summary>
        /// Prevents a default instance of the TextFileWriterCommon class from being created.
        /// </summary>
        private TextFileWriterCommon()
        {
        }

        /// <summary>
        /// Creates a string from a point event.
        /// </summary>
        /// <param name="evt">The point event to serialize.</param>
        /// <param name="eventType">CEP event type.</param>
        /// <param name="delimiter">Delimiter between event fields.</param>
        /// <returns>Serialized event.</returns>
        public static string CreateLineFromEvent(PointEvent evt, CepEventType eventType, char delimiter)
        {
            StringBuilder builder = new StringBuilder();

            if (EventKind.Cti == evt.EventKind)
            {
                builder
                    .Append("CTI")
                    .Append(delimiter)
                    .Append(evt.StartTime.ToString());
            }
            else
            {
                builder
                    .Append("INSERT")
                    .Append(delimiter)
                    .Append(evt.StartTime.ToString())
                    .Append(delimiter);

                SerializePayload(evt, eventType, delimiter, ref builder);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Creates a string from an edge event.
        /// </summary>
        /// <param name="evt">The edge event to serialize.</param>
        /// <param name="eventType">CEP event type.</param>
        /// <param name="delimiter">Delimiter between event fields.</param>
        /// <returns>Serialized event.</returns>
        public static string CreateLineFromEvent(EdgeEvent evt, CepEventType eventType, char delimiter)
        {
            StringBuilder builder = new StringBuilder();

            if (EventKind.Cti == evt.EventKind)
            {
                builder
                    .Append("CTI")
                    .Append(delimiter)
                    .Append(evt.StartTime.ToString());
            }
            else
            {
                builder
                    .Append("INSERT")
                    .Append(delimiter)
                    .Append(evt.EdgeType.ToString())
                    .Append(delimiter)
                    .Append(evt.StartTime.ToString())
                    .Append(delimiter)
                    .Append((EdgeType.End == evt.EdgeType) ? evt.EndTime.ToString() : string.Empty)
                    .Append(delimiter);

                SerializePayload(evt, eventType, delimiter, ref builder);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Creates a string from an interval event.
        /// </summary>
        /// <param name="evt">The interval event to serialize.</param>
        /// <param name="eventType">CEP event type.</param>
        /// <param name="delimiter">Delimiter between event fields.</param>
        /// <returns>Serialized event.</returns>
        public static string CreateLineFromEvent(IntervalEvent evt, CepEventType eventType, char delimiter)
        {
            StringBuilder builder = new StringBuilder();

            if (EventKind.Cti == evt.EventKind)
            {
                builder
                    .Append("CTI")
                    .Append(delimiter)
                    .Append(evt.StartTime.ToString());
            }
            else
            {
                builder
                    .Append("INSERT")
                    .Append(delimiter)
                    .Append(evt.StartTime.ToString())
                    .Append(delimiter)
                    .Append(evt.EndTime.ToString())
                    .Append(delimiter);

                SerializePayload(evt, eventType, delimiter, ref builder);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Creates a string from an event payload.
        /// </summary>
        /// <param name="evt">Event of which to serialize the payload.</param>
        /// <param name="eventType">CEP event type.</param>
        /// <param name="delimiter">Delimiter between event fields.</param>
        /// <param name="builder">StringBuilder to append the payload string to.</param>
        private static void SerializePayload(UntypedEvent evt, CepEventType eventType, char delimiter, ref StringBuilder builder)
        {
            for (int ordinal = 0; ordinal < eventType.FieldsByOrdinal.Count; ordinal++)
            {
                object value = evt.GetField(ordinal) ?? "NULL";

                if (value.GetType().FullName == "System.Byte[]")
                {
                    System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                    builder
                        .AppendFormat(CultureInfo.InvariantCulture, "{0}", enc.GetString((byte[])value))
                        .Append(delimiter);
                }
                else
                {
                    builder
                        .AppendFormat(CultureInfo.InvariantCulture, "{0}", value)
                        .Append(delimiter);
                }
            }
        }
    }
}
