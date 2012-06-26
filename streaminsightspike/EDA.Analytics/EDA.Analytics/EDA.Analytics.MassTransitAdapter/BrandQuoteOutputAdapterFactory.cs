using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using EDA.Analytics.MassTransitAdapter;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace EDA.Analytics.Adapters
{
    public sealed class BrandQuoteOutputAdapterFactory : IOutputAdapterFactory<BrandQuoteOutputConfig>
    {
        public OutputAdapterBase Create(BrandQuoteOutputConfig configInfo, EventShape eventShape, CepEventType cepEventType)
        {
            OutputAdapterBase adapter = default(OutputAdapterBase);
            switch (eventShape)
            {
                //case EventShape.Interval:
                //    adapter = new CsvInputInterval(configInfo, cepEventType);
                //    break;
                case EventShape.Point:
                    adapter = new BrandQuoteOutputAdapter(configInfo);//, cepEventType);
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

    public class BrandQuoteOutputConfig
    {
    }

    public class BrandQuoteInputConfig
    {
        public int Interval;
    }
}