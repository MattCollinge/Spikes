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
    using System.Globalization;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    public sealed class CsvInputFactory : IInputAdapterFactory<CsvInputConfig>
    {
        public InputAdapterBase Create(CsvInputConfig configInfo, EventShape eventShape, CepEventType cepEventType)
        {
            InputAdapterBase adapter = default(InputAdapterBase);
            switch (eventShape)
            {
                case EventShape.Interval:
                    adapter = new CsvInputInterval(configInfo, cepEventType);
                    break;
                case EventShape.Point:
                    adapter = new CsvInputPoint(configInfo, cepEventType);
                    break;
                default:
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Unknown event shape {0}", eventShape.ToString()));
            }

            return adapter;
        }

        public void Dispose()
        {
        }
    }
}