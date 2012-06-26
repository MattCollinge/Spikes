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
    public sealed class BrandQuoteInputAdapterFactory : IInputAdapterFactory<BrandQuoteInputConfig>
    {
        public InputAdapterBase Create(BrandQuoteInputConfig configInfo, EventShape eventShape, CepEventType cepEventType)
        {
            InputAdapterBase adapter = default(InputAdapterBase);
            switch (eventShape)
            {
                //case EventShape.Interval:
                //    adapter = new CsvInputInterval(configInfo, cepEventType);
                //    break;
                case EventShape.Point:
                    adapter = new BrandQuoteInputAdapter(configInfo);//, cepEventType);
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