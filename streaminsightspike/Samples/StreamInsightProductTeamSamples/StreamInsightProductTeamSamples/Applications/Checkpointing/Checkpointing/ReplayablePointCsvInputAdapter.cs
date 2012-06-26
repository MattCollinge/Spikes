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
    using System.IO;
    using System.Threading;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;
    
    /// <summary>
    /// A replayable CSV input adapter. This reads files of the form:
    ///     Event Time,X,Y
    /// It is created with a time to start replay from, and will scan the input file for that time
    /// and begin there. This assumes that the input file is strictly time-ordered.
    /// </summary>
    public class ReplayablePointCsvInputAdapter : TypedPointInputAdapter<XYPayload>
    {
        /// <summary>
        /// How often to produce events. If this is 0, we'll go as fast as we can.
        /// </summary>
        private TimeSpan eventInterval;

        /// <summary>
        /// Out input.
        /// </summary>
        private StreamReader input;

        /// <summary>
        /// The "current" time: the time associated with the current row from the CSV file.
        /// </summary>
        private DateTimeOffset currentTime;

        /// <summary>
        /// The current row we're processing.
        /// </summary>
        private Tuple<DateTimeOffset, double, double> currentRow;

        /// <summary>
        /// Object used to lock operations that rely on or modify adapter state.
        /// </summary>
        private object stateLock = new object();

        /// <summary>
        /// Initializes a new instance of the ReplayablePointCsvInputAdapter class with replay.
        /// </summary>
        /// <param name="inputPath">The path to our input file.</param>
        /// <param name="interval">How often to produce events (in real time). Zero will produce as fast as possible.</param>
        /// <param name="highWaterMark">The high-water mark at which to begin replay.</param>
        public ReplayablePointCsvInputAdapter(string inputPath, TimeSpan interval, DateTimeOffset highWaterMark)
        {
            Util.Log(
                "ReplayablePointCsvInputAdapter init", 
                string.Format("Playing {0} from {1}.", Path.GetFileName(inputPath), highWaterMark));

            // Open the file.
            this.input = new StreamReader(inputPath);

            // Save our configuration.
            this.eventInterval = interval;
            this.currentTime = highWaterMark;

            // Cue the input to our start position.
            this.CueInputToCurrentTime();
        }

        /// <summary>
        /// Initializes a new instance of the ReplayablePointCsvInputAdapter class without replay.
        /// </summary>
        /// <param name="inputPath">The path to our input file.</param>
        /// <param name="interval">How often to produce events (in real time). Zero will produce as fast as possible.</param>
        public ReplayablePointCsvInputAdapter(string inputPath, TimeSpan interval) :
            this(inputPath, interval, DateTimeOffset.MinValue)
        {
            // For this adapter, this is equivalent to replay from DateTimeOffset.MinValue.
        }

        /// <summary>
        /// Start up. This forks off a thread that will run until we're stopped or paused.
        /// </summary>
        public override void Start()
        {
            Thread producerThread = new Thread(this.RunInput);
            producerThread.Name = "Producer";
            producerThread.Start();
            Util.Log("ReplayablePointCsvInputAdapter", "Started.");
        }

        /// <summary>
        /// Start the query again after we've paused by calling Ready().
        /// </summary>
        public override void Resume()
        {
            Thread producerThread = new Thread(this.RunInput);
            producerThread.Name = "Producer";
            producerThread.Start();
            Util.Log("ReplayablePointCsvInputAdapter", "Resumed.");
        }

        /// <summary>
        /// Stop the adapter. This will wait until the producer thread finishes.
        /// </summary>
        public override void Stop()
        {
            Util.Log("ReplayablePointCsvInputAdapter", "Stop called.");
            lock (this.stateLock)
            {
                if (this.AdapterState != AdapterState.Stopped)
                {
                    this.Stopped();
                    Util.Log("ReplayablePointCsvInputAdapter", "Stopped.");
                }
            }
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing">Should we clean up managed resources?</param>
        protected override void Dispose(bool disposing)
        {
            Util.Log("ReplayablePointCsvInputAdapter", "Dispose called.");
            if (disposing)
            {
                if (this.input != null)
                {
                    this.input.Dispose();
                    this.input = null;
                }
            }
        }

        /// <summary>
        /// This is the main loop for the proucer thread. Essentially, it first confirms that
        /// it hasn't been asked to stop, and that it hasn't completed its input, and then proceeds
        /// to queue an event and a CTI. This assumes that the input is ordered: if it isn't
        /// then CTI violations will occur.
        /// </summary>
        private void RunInput()
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

                    // If we're out of input, cue a CTI at +inf and stop.
                    if (this.currentRow == null)
                    {
                        this.EnqueueCti(DateTimeOffset.MaxValue);
                        this.Stopped(); // This informs the system that we're done.
                        return;
                    }

                    // Queue the event.
                    if (!this.EnqueueCurrentRow())
                    {
                        return;
                    }

                    // Capture the time of the next CTI we'll insert: we enqueue a CTI with the time of each event as we go.
                    // We'd have to rethink this if we wanted to handle out-of-order CSV files.
                    DateTimeOffset ctiTime = this.currentRow.Item1;

                    // Cue the next event.
                    this.CueNextRow();

                    // Queue the CTI.
                    if (!this.EnqueueCti(ctiTime))
                    {
                        return;
                    }
                }

                // If we've been asked to delay our output, do so here.
                if (this.eventInterval > TimeSpan.Zero)
                {
                    Thread.Sleep(this.eventInterval);
                }
            }
        }

        /// <summary>
        /// Queue the given CSV row.
        /// </summary>
        /// <returns>True if the row was successfully queued; false otherwise.</returns>
        private bool EnqueueCurrentRow()
        {
            // create the event
            var evt = PointEvent.CreateInsert(
                this.currentRow.Item1,
                new XYPayload(this.currentRow.Item2, this.currentRow.Item3));

            // queue the event
            if (Enqueue(ref evt) != EnqueueOperationResult.Success)
            {
                Util.Log("ReplayablePointCsvInputAdapter", "Encountered full while enqueueing event. Pausing.");
                ReleaseEvent(ref evt);
                Ready();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Queue a CTI at the given time.
        /// </summary>
        /// <param name="ctiTime">The time of the CTI to queue.</param>
        /// <returns>True if the CTI was successfully queued; false otherewise.</returns>
        private bool EnqueueCti(DateTimeOffset ctiTime)
        {
            // queue the CTI
            if (EnqueueCtiEvent(ctiTime) != EnqueueOperationResult.Success)
            {
                Util.Log("ReplayablePointCsvInputAdapter", "Encountered full while enqueueing CTI. Pausing.");
                Ready();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Advance the input up to the current time. This is primarily used to scan through
        /// the input in preparation for replay.
        /// </summary>
        private void CueInputToCurrentTime()
        {
            int skipcount = -1;
            do
            {
                skipcount++;
                this.CueNextRow();
            }
            while (this.currentRow != null
                && this.currentRow.Item1 < this.currentTime);

            Util.Log("ReplayablePointCsvInputAdapter", "Skipped " + skipcount + " lines.");
        }

        /// <summary>
        /// Get the next line from the file and stash it as our current row. This row will
        /// be set to null if there is no more.
        /// </summary>
        private void CueNextRow()
        {
            string line;
            if ((line = this.input.ReadLine()) != null)
            {
                this.currentRow = this.ParseLine(line);
            }
            else
            {
                this.currentRow = null;
            }
        }

        /// <summary>
        /// Parse a line of a CSV.
        /// </summary>
        /// <param name="line">The text line to parse.</param>
        /// <returns>A tuple giving the time, x, and y values parsed from the CSV line.</returns>
        private Tuple<DateTimeOffset, double, double> ParseLine(string line)
        {
            string[] pieces = line.Split(',');
            if (pieces.Length != 3)
            {
                throw new ArgumentException("Expected three comma-separated pieces on that line; only got " + pieces.Length + ".");
            }

            var rval = new Tuple<DateTimeOffset, double, double>(
                DateTimeOffset.Parse(pieces[0]),
                double.Parse(pieces[1]),
                double.Parse(pieces[2]));
            return rval;
        }
    }
}
