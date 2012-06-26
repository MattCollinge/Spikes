// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Extensibility;

namespace AdvantIQ.StockInsight
{
    /// <summary>
    /// Observable class that reads stock quotes from a Comma Separated file
    /// </summary>
    public static class Enumerable
    {
        // Format provider that is used to ensure we get no problems with different language settings
        public readonly static IFormatProvider QuoteFormatProvider = CultureInfo.InvariantCulture.NumberFormat;

        /// <summary>
        /// Reads a Comma Separated file and outputs an IObservable of Events
        /// </summary>
        /// <param name="filename">Full path of file</param>
        /// <param name="stockID">StockID for the Events</param>
        /// <param name="fieldIDs">Fields to include in the production of Events</param>
        /// <returns>An IObservable of Stock Quotes</returns>
        public static IEnumerable<StockQuote> GetStockObservable(string filename, string stockID, string[] fieldIDs)
        {
            // Read file header and extract field names
            var streamReader = new StreamReader(filename);
            var line = streamReader.ReadLine();
            var values = line.Split(',');
            var fields = new SortedList<string, int>(values.Length);
            for (int i = 0; i < values.Length; i++)
                fields.Add(values[i], i);

            // Create List for holding the quotes
            var quotes = new List<StockQuote>();

            // Read remaining lines
            while (!streamReader.EndOfStream)
            {
                line = streamReader.ReadLine();
                values = line.Split(',');
                var timestamp = DateTime.Parse(values[0], QuoteFormatProvider);

                // Add a quote for every fieldID
                foreach (var fieldID in fieldIDs)
                {
                    var quote = new StockQuote();
                    var value = values[fields[fieldID]];
                    quote.TimeStamp = timestamp;
                    quote.StockID = stockID;
                    quote.FieldID = fieldID;
                    quote.Value = double.Parse(value, QuoteFormatProvider);
                    quotes.Add(quote);
                }
            }

            streamReader.Close();

            // Sort the List
            quotes.Sort((x, y) => DateTimeOffset.Compare(x.TimeStamp, y.TimeStamp));

            // Convert the List into an IObservable and return
            return quotes;
        }
    }
}
