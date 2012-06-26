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
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;
    using System.Threading;

    public sealed class CsvOutputAdapter<TOutputAdapter, TEvent> 
        where TEvent : UntypedEvent
        where TOutputAdapter : IOutputAdapter<TEvent>
    {
        private StreamWriter streamWriter;
        private CepEventType bindtimeEventType;
        private Dictionary<int, int> mapOrdinalsFromOutputToCepEvent;
        private CsvOutputConfig config;
        private CultureInfo culture;

        public CsvOutputAdapter(CsvOutputConfig configInfo, CepEventType eventType)
        {
            this.streamWriter = configInfo.OutputFileName.Length == 0 ? new StreamWriter(Console.OpenStandardOutput()) : new StreamWriter(configInfo.OutputFileName);
            this.config = configInfo;
            this.bindtimeEventType = eventType;
            this.mapOrdinalsFromOutputToCepEvent = CsvCommon.MapPayloadToFieldsByOrdinal(configInfo.Fields, eventType);
            this.culture = new CultureInfo(configInfo.CultureName);
        }

        public void ConsumeEvents(TOutputAdapter outputAdapter)
        {
            TEvent currentEvent = default(TEvent);

            while (true)
            {
                // if the engine is attempting to stop the adapter,  make one more attempt to dequeue the last event from the engine,
                // clean up state and exit worker thread
                if (AdapterState.Stopping == outputAdapter.AdapterState)
                {
                    this.Cleanup(outputAdapter);
                    return;
                }

                // NOTE: at any point in time during execution of the code below, if the Adapter
                // state changes to Stopping, the engine will resume the adapter (i.e. call Resume())
                // just one more time, and the stopping condition will be trapped at the check above.

                // Dequeue the event
                // if the engine does not have any events, it puts the adapter in a Suspended state; or it
                // could also be putting the adapter in a Stopping state. If it is in the suspended state,
                // you can optionally invoke a method here for housekeeping. Signal to the engine that the
                // adapter is ready to be resumed, and exit the worker thread
                if (DequeueOperationResult.Empty == outputAdapter.Dequeue(out currentEvent))
                {
                    outputAdapter.Ready();
                    return;
                }

                try
                {
                    string line = CreateLineFromEvent(currentEvent, this.bindtimeEventType, this.config.Delimiter, this.mapOrdinalsFromOutputToCepEvent, this.culture);
                    this.streamWriter.WriteLine(line);
                }
                finally
                {
                    outputAdapter.ReleaseEvent(ref currentEvent);
                }
            }
        }

        private static string CreateLineFromEvent(UntypedEvent evt, CepEventType eventType, string[] delimiter, Dictionary<int, int> order, CultureInfo culture)
        {
            StringBuilder builder = new StringBuilder();
            PointEvent pointEvent = evt as PointEvent;
            IntervalEvent intervalEvent = evt as IntervalEvent;
            EdgeEvent edgeEvent = evt as EdgeEvent;

            if (EventKind.Cti == evt.EventKind)
            {
                builder
                    .Append("CTI")
                    .Append(delimiter[0]);

                if (null != pointEvent)
                {
                    builder.Append(pointEvent.StartTime.ToString());
                }
                else if (null != intervalEvent)
                {
                    builder.Append(intervalEvent.StartTime.ToString());
                }
                else
                {
                    builder.Append(edgeEvent.StartTime.ToString());
                }
            }
            else
            {
                builder
                    .Append("INSERT")
                    .Append(delimiter[0]);

                if (null != pointEvent)
                {
                    builder.Append(pointEvent.StartTime.ToString());
                }
                else if (null != intervalEvent)
                {
                    builder
                        .Append(intervalEvent.StartTime.ToString())
                        .Append(delimiter[0])
                        .Append(intervalEvent.EndTime.ToString())
                        .Append(delimiter[0]);
                }
                else
                {
                    builder
                        .Append(edgeEvent.EdgeType.ToString())
                        .Append(delimiter[0])
                        .Append(edgeEvent.StartTime.ToString())
                        .Append(delimiter[0])
                        .Append((EdgeType.End == edgeEvent.EdgeType) ? edgeEvent.EndTime.ToString() : string.Empty)
                        .Append(delimiter[0]);
                }

                SerializePayload(evt, eventType, delimiter, order, culture, ref builder);
            }

            return builder.ToString();
        }

        private static void SerializePayload(UntypedEvent evt, CepEventType eventType, string[] delimiter, Dictionary<int, int> order, CultureInfo culture, ref StringBuilder builder)
        {
            for (int ordinal = 0; ordinal < eventType.FieldsByOrdinal.Count; ordinal++)
            {
                object value = evt.GetField(order[ordinal]) ?? "NULL";
                builder
                    .AppendFormat(culture, "{0}", value)
                    .Append(delimiter[0]);
            }
        }

        private void Cleanup(TOutputAdapter outputAdapter)
        {
            if (null != this.streamWriter)
            {
                this.streamWriter.Flush();
                this.streamWriter.Close();
            }

            outputAdapter.Stopped();
        }
    }
}