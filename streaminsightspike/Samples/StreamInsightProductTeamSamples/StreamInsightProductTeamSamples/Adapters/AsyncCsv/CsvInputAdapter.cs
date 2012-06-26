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
    using System.IO;
    using System.Text;
    using System.Threading;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    /// <summary>
    /// Generic implementation of an asynchronous CSV input adapter.
    /// This class contains code that is common for all CSV input adapters.
    /// </summary>
    /// <typeparam name="TInputAdapter">The type of the concrete adapter to use (Point, Interval).</typeparam>
    /// <typeparam name="TEvent">The concrete event shape (Point, Interval).</typeparam>
    public sealed class CsvInputAdapter<TInputAdapter, TEvent>
        where TEvent : UntypedEvent
        where TInputAdapter : IInputAdapter<TEvent>
    {
        /// <summary>
        /// Specific adapter object. Since it has to implement the IInputAdapter
        /// interface, this object provides all necessary adapter interactions.
        /// </summary>
        private TInputAdapter inputAdapter;

        /// <summary>
        /// The configuration object passed from the factory.
        /// </summary>
        private CsvInputConfig config;

        /// <summary>
        /// Filestream to read from.
        /// </summary>
        private FileStream fileStreamReader;

        /// <summary>
        /// Event type to be produced by the adapter at runtime.
        /// </summary>
        private CepEventType bindTimeEventType;

        /// <summary>
        /// Buffer to read the file into.
        /// </summary>
        private byte[] buffer;

        /// <summary>
        /// Residual line that needs to be remembered until the next buffer is read.
        /// </summary>
        private StringBuilder pendingBuffer;

        /// <summary>
        /// Mapping from user-specified field order to the event type's field order.
        /// </summary>
        private Dictionary<int, int> mapOrdinalsFromInputToCepEvent;

        /// <summary>
        /// Singal to coordinate when the engine is pushing back.
        /// </summary>
        private ManualResetEvent readyToEnqueue;

        public CsvInputAdapter(CsvInputConfig configInfo, CepEventType eventType, TInputAdapter inputAdapter)
        {
            this.inputAdapter = inputAdapter;
            this.config = configInfo;
            this.fileStreamReader = new FileStream(configInfo.InputFileName, FileMode.Open, FileAccess.Read, FileShare.None, this.config.BufferSize, true);
            this.buffer = new byte[this.config.BufferSize];
            this.bindTimeEventType = eventType;
            this.mapOrdinalsFromInputToCepEvent = CsvCommon.MapPayloadToFieldsByOrdinal(this.config.Fields, this.bindTimeEventType);
            this.readyToEnqueue = new ManualResetEvent(true);
        }

        internal void Start()
        {
            this.AsyncReadEventData();
        }

        internal void Resume()
        {
            this.readyToEnqueue.Set();
        }

        private void AsyncReadEventData()
        {
            // Start to read into buffer. OnReadComplete will be called upon
            // completion.
            // While the adapter is running, we will only call this after we
            // have successfully enqueued the previous buffer, so no need to
            // worry about concurrent calls.
            this.fileStreamReader.BeginRead(this.buffer, 0, this.config.BufferSize, new AsyncCallback(this.OnReadComplete), null);
        }

        private void OnReadComplete(IAsyncResult result)
        {
            // if the engine is attempting to stop the adapter, clean up state and exit thread
            if (AdapterState.Stopping == this.inputAdapter.AdapterState)
            {
                this.Cleanup();
                return;
            }

            int bytesRead = this.fileStreamReader.EndRead(result);

            // are we done?
            if (0 == bytesRead)
            {
                this.Cleanup();
                return;
            }

            // turn the buffer into events and enqueue them
            new Thread(() => this.ProduceEvents(this.buffer, bytesRead)).Start();
        }

        /// <summary>
        /// Interprets a byte buffer as a set of lines and enqueues them as events.
        /// </summary>
        /// <param name="data">The buffer.</param>
        /// <param name="bytesRead">Size of the data in the buffer.</param>
        private void ProduceEvents(byte[] data, int bytesRead)
        {
            TEvent currentEvent = default(TEvent);

            // generate new lines from a buffer's worth of asynchronously read data
            IEnumerator<string> lines = this.ByteArrayToLines(data, bytesRead).GetEnumerator();

            // move to the first line, if there is one at all
            if (!lines.MoveNext())
            {
                return;
            }

            // each line corresponds to an event - enqueue them
            do
            {
                // account for empty lines (owing to possible \n as starting character)
                if (0 == lines.Current.Length)
                {
                    continue;
                }

                currentEvent = this.CreateEventFromLine(lines.Current);

                if (currentEvent == null)
                {
                    // something went wrong there
                    this.Cleanup();
                    return;
                }

                EnqueueOperationResult result = this.inputAdapter.Enqueue(ref currentEvent);
                    
                if (EnqueueOperationResult.Full == result)
                {
                    this.inputAdapter.ReleaseEvent(ref currentEvent);

                    this.inputAdapter.Ready();

                    // wait to be resumed
                    this.readyToEnqueue.WaitOne();

                    return;
                }
            } while (lines.MoveNext());

            // kick off the next reading cycle
            this.AsyncReadEventData();
        }

        /// <summary>
        /// Turns a CSV line into an event.
        /// </summary>
        public TEvent CreateEventFromLine(string line)
        {
            CultureInfo culture = new CultureInfo(this.config.CultureName);

            // split the incoming line into component fields
            string[] split = line.Split(this.config.Delimiter, StringSplitOptions.None);
            if (this.bindTimeEventType.Fields.Count != (split.Length - this.config.NonPayloadFieldCount))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "Payload field count in input file {0} does not match field count in EventType {1}",
                    split.Length - this.config.NonPayloadFieldCount,
                    this.bindTimeEventType.Fields.Count));
            }

            // create a new event
            TEvent evt = this.inputAdapter.CreateInsertEvent();
            if (null == evt)
            {
                return evt;
            }

            // populate the payload fields
            // Assume your EventType definition consists of fields {b, a, c}
            // and the input file provides  values of these fields in this
            // order. StreamInsight understands CepEventType in alphabetical
            // order of the fields - that is, {a, b, c}. If you set values by
            // position, there could be mismatched types or assignments of
            // values to fields. So we map the fields in each line with the
            // correct field in CepEventType using an ordinal mapper - and use
            // SetField() to assign the correct input values to the payload fields.
            object value = null;
            for (int pos = 0; pos < (this.bindTimeEventType.FieldsByOrdinal.Count + this.config.NonPayloadFieldCount); pos++)
            {
                // if the position counter maps to a StartTime or EndTime field position in the file, skip.
                if ((this.config.StartTimePos - 1) == pos || (this.config.EndTimePos - 1) == pos)
                {
                    continue;
                }

                // get the corresponding ordinal for the CepEvent based on the specified payload field ordering
                int cepOrdinal = this.mapOrdinalsFromInputToCepEvent[pos - this.config.NonPayloadFieldCount];

                // identify the CepEvent field by ordinal
                CepEventTypeField evtField = this.bindTimeEventType.FieldsByOrdinal[cepOrdinal];

                // set the value for the field, handling NULLs (byte array and GUID not covered)
                if (split[pos].Length == 0)
                {
                    value = this.GetDefaultValue(evtField.Type.ClrType, culture);
                }
                else
                {
                    Type t = Nullable.GetUnderlyingType(evtField.Type.ClrType) ?? evtField.Type.ClrType;
                    value = Convert.ChangeType(split[pos], t, culture);
                }

                evt.SetField(cepOrdinal, value);
            }

            // set the timestamps
            PointEvent pointEvent = evt as PointEvent;
            IntervalEvent intervalEvent = evt as IntervalEvent;

            if (intervalEvent != null)
            {
                intervalEvent.StartTime = DateTime.Parse(split[this.config.StartTimePos - 1], culture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
                intervalEvent.EndTime = DateTime.Parse(split[this.config.EndTimePos - 1], culture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
            }
            else if (pointEvent != null)
            {
                pointEvent.StartTime = DateTime.Parse(split[this.config.StartTimePos - 1], culture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
            }

            return evt;
        }

        private IEnumerable<string> ByteArrayToLines(byte[] data, int bytesRead)
        {
            // The line break added at the end of the data buffer ensures that the last
            // line in the buffer is extracted properly.
            using (var reader = new StringReader(Encoding.ASCII.GetString(data, 0, bytesRead) + Environment.NewLine))
            {
                string line;
                string previousLine = null;

                // Read all lines from the buffer. This will also read a partial
                // line at the end of the buffer.
                // Since we added an extra newline above, the very last value
                // returned from ReadLine will be an empty string. After that, it
                // will return null.
                while ((line = reader.ReadLine()) != null)
                {
                    // Did we remember a partial line from before?
                    if (this.pendingBuffer != null)
                    {
                        // Add current line to that and reset
                        line = this.pendingBuffer.ToString() + line;
                        this.pendingBuffer = null;
                    }

                    // If the current ReadLine succeeded, we must have had a
                    // complete line in the loop before.
                    if (previousLine != null)
                    {
                        yield return previousLine;
                    }

                    previousLine = line;
                }

                // Did we read anything at all?
                if (null != previousLine)
                {
                    // The last read line was partial. Remember for next time.
                    this.pendingBuffer = new StringBuilder(previousLine);
                }
            }
        }

        public object GetDefaultValue(Type clrType, CultureInfo culture)
        {
            if (clrType == typeof(string))
            {
                return string.Empty;
            }
            else if (clrType == typeof(byte[]))
            {
                return new byte[] { };
            }
            else if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return null;
            }
            else
            {
                return Convert.ChangeType(0, clrType, culture);
            }
        }

        private void Cleanup()
        {
            if (null != this.fileStreamReader)
            {
                this.fileStreamReader.Close();
            }

            this.inputAdapter.Stopped();
        }
    }
}