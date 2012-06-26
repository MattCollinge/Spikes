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
    using System.Globalization;
    using Microsoft.ComplexEventProcessing;

    /// <summary>
    /// Common methods for CSV Input and Output adapters.
    /// </summary>
    public sealed class CsvCommon
    {
        /// <summary>
        /// Prevents a default instance of the CsvCommon class from being created.
        /// </summary>
        private CsvCommon()
        {
        }

        /// <summary>
        /// Creates a dictionary that maps each payload field to a positional ordinal.
        /// This is used to specify the order of CSV fields in the input file,
        /// as well as to force a certain order when producing CSV output.
        /// </summary>
        /// <param name="fields">The ordered list of payload field names.</param>
        /// <param name="eventType">The event type.</param>
        /// <returns>A mapping between the string ordinal and the payload field ordinal.</returns>
        public static Dictionary<int, int> MapPayloadToFieldsByOrdinal(IList<string> fields, CepEventType eventType)
        {
            if (null != fields && fields.Count != eventType.Fields.Count)
            {
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.CurrentCulture,
                    "List of fields has {0} elements, but should have {1} elements",
                    fields.Count,
                    eventType.Fields.Count));
            }

            CepEventTypeField cepEventField;

            Dictionary<int, int> mapPayloadToCepEventField = new Dictionary<int, int>();

            for (int ordinal = 0; ordinal < eventType.Fields.Count; ordinal++)
            {
                if (null == fields)
                {
                    // Use default mapping: the built-in order of the payload fields
                    // (lexicographic)
                    mapPayloadToCepEventField.Add(ordinal, ordinal);
                }
                else
                {
                    // find the ordinal of each of the payload fields specified in the list
                    if (!eventType.Fields.TryGetValue(fields[ordinal], out cepEventField))
                    {
                        throw new ArgumentException(
                            string.Format(
                            CultureInfo.CurrentCulture,
                            "Event type {0} doesn't have an input field named '{1}'",
                            eventType.ShortName,
                            fields[ordinal]));
                    }

                    mapPayloadToCepEventField.Add(ordinal, cepEventField.Ordinal);
                }
            }

            return mapPayloadToCepEventField;
        }
    }
}