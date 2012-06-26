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
    using System;
    using System.Globalization;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    /// <summary>
    /// Factory to instantiate a text file reader input adapter.
    /// </summary>
    public sealed class TextFileReaderFactory : IInputAdapterFactory<TextFileReaderConfig>,
                                                  IDeclareAdvanceTimeProperties<TextFileReaderConfig>
    {
        /// <summary>
        /// Returns an instance of a text file reader input adapter.
        /// </summary>
        /// <param name="configInfo">Configuration passed from the query binding.</param>
        /// <param name="eventShape">Event shape requested from the query binding.</param>
        /// <param name="cepEventType">Event type expected by the bound query template.</param>
        /// <returns>An instance of a text file reader input adapter.</returns>
        public InputAdapterBase Create(TextFileReaderConfig configInfo, EventShape eventShape, CepEventType cepEventType)
        {
            InputAdapterBase adapter = default(InputAdapterBase);

            if (eventShape == EventShape.Point)
            {
                adapter = new TextFilePointInput(configInfo, cepEventType);
            }
            else if (eventShape == EventShape.Interval)
            {
                adapter = new TextFileIntervalInput(configInfo, cepEventType);
            }
            else if (eventShape == EventShape.Edge)
            {
                adapter = new TextFileEdgeInput(configInfo, cepEventType);
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "TextFileReaderFactory cannot instantiate adapter with event shape {0}",
                        eventShape.ToString()));
            }

            return adapter;
        }

        /// <summary>
        /// Dispose method.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Specifies the CTI behavior of this adapter. This method will be called whenever
        /// a new adapter instance is needed, with the respective configuration and event type.
        /// </summary>
        /// <param name="configInfo">Configuration passed from the query binding.</param>
        /// <param name="eventShape">Event shape requested from the query binding.</param>
        /// <param name="cepEventType">Event type expected by the bound query template.</param>
        /// <returns>An instance of AdapterAdvanceTimeSettings.</returns>
        public AdapterAdvanceTimeSettings DeclareAdvanceTimeProperties(TextFileReaderConfig configInfo, EventShape eventShape, CepEventType cepEventType)
        {
            // use the user-provided CTI frequency and -1 as the CTI delay. This will set the CTI timestamp
            // one tick _after_ the respective event timestamp, hence committing each event right away.
            var atgs = new AdvanceTimeGenerationSettings(configInfo.CtiFrequency, TimeSpan.FromTicks(-1), true);
            return new AdapterAdvanceTimeSettings(atgs, AdvanceTimePolicy.Drop);
        }
    }
}
