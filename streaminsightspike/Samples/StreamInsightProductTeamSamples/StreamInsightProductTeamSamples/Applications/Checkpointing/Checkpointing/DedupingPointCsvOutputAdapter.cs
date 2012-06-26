//*********************************************************
//
// Copyright Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of
// the License at
//
// http://www.apache.org/licenses/LICENSE-2.0 
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES
// OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES
// OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache 2 License for the specific language
// governing permissions and limitations under the License.
//
//*********************************************************

namespace StreamInsight.Samples.Checkpointing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;
    
    /// <summary>
    /// This adapter takes a stream of point XYPayload events and writes them to a CSV file of the form.
    ///     time,field1,field2,...
    /// This will also do deduplication in a checkpoint recovery situation.
    /// </summary>
    public class DedupingPointCsvOutputAdapter : PointOutputAdapter
    {
        /// <summary>
        /// Our output writer.
        /// </summary>
        private TextWriter writer;

        /// <summary>
        /// A collection of items that are known duplicates. This will be populated during initialization in a recovery
        /// situation, and then be winnowed down as events are processed.
        /// </summary>
        private HashSet<string> duplicates;

        /// <summary>
        /// The type we've been configured to output.
        /// </summary>
        private CepEventType cepEventType;

        /// <summary>
        /// A list of the ordinals for our fields, in the order in which they should be written.
        /// </summary>
        private IEnumerable<int> fieldOrdinals;

        /// <summary>
        /// Object used to lock operations that rely on or modify adapter state.
        /// </summary>
        private object stateLock = new object();

        /// <summary>
        /// Initializes a new instance of the DedupingPointCsvOutputAdapter class that will write to a (new) file without deduplication.
        /// </summary>
        /// <param name="file">A path to the target file.</param>
        /// <param name="fields">The ordered list of field names to output.</param>
        /// <param name="cepEventType">The output type.</param>
        public DedupingPointCsvOutputAdapter(string file, IEnumerable<string> fields, CepEventType cepEventType)
            : base()
        {
            if (!this.RecordFields(fields, cepEventType))
            {
                throw new ArgumentException("The list of fields to print is not compatible with the output event type.");
            }

            this.cepEventType = cepEventType;
            this.duplicates = new HashSet<string>();
            this.writer = new StreamWriter(file, false); // Clear the output file.

            Util.Log("DedupingPointsCsvOutputAdapter init", "Created new for " + Path.GetFileName(file));
        }

        /// <summary>
        /// Initializes a new instance of the DedupingPointCsvOutputAdapter class that will append to a file and do deduplication of events based
        /// on the marked provided.
        /// </summary>
        /// <param name="file">A path to the target file.</param>
        /// <param name="fields">The ordered list of field names to output.</param>
        /// <param name="cepEventType">The output event type.</param>
        /// <param name="replayHighWaterMark">The high-water mark to deduplicate from.</param>
        /// <param name="eventOffset">The event offset from the high-water mark to deduplicate from.</param>
        public DedupingPointCsvOutputAdapter(string file, IEnumerable<string> fields, CepEventType cepEventType, DateTimeOffset replayHighWaterMark, int eventOffset)
            : base()
        {
            if (!this.RecordFields(fields, cepEventType))
            {
                throw new ArgumentException("The list of fields to print is not compatible with the output event type.");
            }

            this.cepEventType = cepEventType;
            this.duplicates = new HashSet<string>();
            this.PopulateDuplicates(file, replayHighWaterMark, eventOffset);
            this.writer = new StreamWriter(file, true); // Open output for append.

            Util.Log("DedupingPointsCsvOutputAdapter init", "Created for restart for " + Path.GetFileName(file));
        }

        /// <summary>
        /// Resume consuming events.
        /// </summary>
        public override void Resume()
        {
            Thread consumerThread = new Thread(this.ConsumeEvents);
            consumerThread.Name = "Consumer";
            consumerThread.Start();
        }

        /// <summary>
        /// Start consuming events.
        /// </summary>
        public override void Start()
        {
            Thread consumerThread = new Thread(this.ConsumeEvents);
            consumerThread.Name = "Consumer";
            consumerThread.Start();
        }

        /// <summary>
        /// Called when the adapter is asked to stop.
        /// </summary>
        public override void Stop()
        {
            Util.Log("DedupingPointsCsvOutputAdapter", "Stop called.");

            // We may need to handle the stop if the consumer thread isn't running.
            lock (this.stateLock)
            {
                if (this.AdapterState != AdapterState.Stopped)
                {
                    this.Stopped();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            Util.Log("DedupingPointsCsvOutputAdapter", "Dispose called.");
            if (this.writer != null)
            {
                this.writer.Dispose();
                this.writer = null;
            }
        }

        /// <summary>
        /// The main loop for our consumer thread. So long as the adapter is running, it will continue to consume
        /// and process events.
        /// </summary>
        private void ConsumeEvents()
        {
            while (true)
            {
                lock (this.stateLock)
                {
                    // If we've been asked to stop, stop.
                    if (this.AdapterState == AdapterState.Stopped || this.AdapterState == AdapterState.Stopping)
                    {
                        return;
                    }
                 
                    // Get the event.
                    PointEvent evt;
                    DequeueOperationResult result = Dequeue(out evt);
                    if (result == DequeueOperationResult.Empty)
                    {
                        // If there wasn't anything for us to dequeue, we pause. We'll kick off a new thread later when
                        // we restart.
                        Ready();
                        return;
                    }

                    // Handle writing the event. We only care about inserts.
                    if (evt.EventKind == EventKind.Insert)
                    {
                        // Create the line
                        string line = this.BuildOutputLine(evt);

                        // If the hash does not contain our event, then write it.
                        if (!this.duplicates.Contains(line))
                        {
                            this.writer.WriteLine(line);
                            this.writer.Flush();
                        }
                        else
                        {
                            // Otherwise we've seen this event before.
                            this.duplicates.Remove(line);
                            if (this.duplicates.Count == 0)
                            {
                                Util.Log("DedupingPointsCsvOutputAdapter", "All duplicates eliminated.");
                            }
                        }
                    }

                    // We're done with the event!
                    ReleaseEvent(ref evt);
                }
            }
        }

        /// <summary>
        /// Build a CSV line for output from the given event.
        /// </summary>
        /// <param name="evt">The event to output.</param>
        /// <returns>A string representing the CSV form of the event.</returns>
        private string BuildOutputLine(PointEvent evt)
        {
            StringBuilder line = new StringBuilder();
            line.Append(evt.StartTime);

            foreach (var i in this.fieldOrdinals)
            {
                line.Append(',');
                line.Append(evt.GetField(i));
            }

            return line.ToString();
        }

        /// <summary>
        /// Given an output CSV file and a marker (high-water mark + eventOffset) indicating where we'll start
        /// seeing duplicates, populate our hash that we can eliminate the duplicates.
        /// </summary>
        /// <param name="file">A path to the file containing our previous output.</param>
        /// <param name="replayHighWaterMark">The high-water mark portion of our duplicate marker.</param>
        /// <param name="eventOffset">The event offset from the high-water mark; part of our duplicate marker.</param>
        private void PopulateDuplicates(string file, DateTimeOffset replayHighWaterMark, int eventOffset)
        {
            if (File.Exists(file))
            {
                // Open the file...
                using (var reader = new StreamReader(file))
                {
                    string line;

                    // Scan to the high-water mark.
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (this.GetLineTimestamp(line) >= replayHighWaterMark)
                        {
                            break;
                        }
                    }

                    // Scan past the specified number of events past the HWM.
                    for (int i = 0; i < eventOffset && (line = reader.ReadLine()) != null; i++)
                    {
                    }

                    // Read in the remainder of the file and populate our hash.
                    while (line != null)
                    {
                        this.duplicates.Add(line);
                        line = reader.ReadLine();
                    }

                    Util.Log("DedupingPointsCsvOutputAdapter (" + Path.GetFileName(file) + ")", this.duplicates.Count + " duplicates.");
                }
            }
        }

        /// <summary>
        /// Assuming the line is a CSV of the form datetimeoffset{,...}*, pull off the leading timestamp.
        /// </summary>
        /// <param name="line">The CSV line.</param>
        /// <returns>The DateTimeOffset at the head of the line.</returns>
        private DateTimeOffset GetLineTimestamp(string line)
        {
            string[] pieces = line.Split(new char[] { ',' }, 2);
            return DateTimeOffset.Parse(pieces[0]);
        }

        /// <summary>
        /// Verify that all of the fields we've been asked to output actually exist in the output type and
        /// record the fields we are to print in our fields list.
        /// </summary>
        /// <param name="fields">The list of fields to output.</param>
        /// <param name="cepEventType">The type of our output.</param>
        /// <returns>True if the list of fields is compatible with the type; false otherwise.</returns>
        private bool RecordFields(IEnumerable<string> fields, CepEventType cepEventType)
        {
            if (fields == null)
            {
                // Null is fine: it just means we should output them all.
                fields = cepEventType.Fields.Keys;
            }
            else if (!fields.All(s => cepEventType.Fields.ContainsKey(s)))
            {
                // Oops... something didn't match.
                return false;
            }

            // build the list of ordinals
            this.fieldOrdinals = fields.Select(s => cepEventType.Fields[s].Ordinal).ToArray();
            return true;
        }
    }
}
