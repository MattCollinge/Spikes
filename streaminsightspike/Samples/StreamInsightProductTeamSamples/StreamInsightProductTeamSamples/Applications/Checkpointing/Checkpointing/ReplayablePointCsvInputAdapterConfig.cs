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
    using System.Runtime.Serialization;
    
    /// <summary>
    /// Configuration for a ReplayablePointCsvInputAdapter.
    /// </summary>
    [DataContract]
    public class ReplayablePointCsvInputAdapterConfig
    {
        /// <summary>
        /// Initializes a new instance of the ReplayablePointCsvInputAdapterConfig class with a target file and interval at which to produce events.
        /// </summary>
        /// <param name="file">The input file.</param>
        /// <param name="interval">How often to produce events. Will produce as fast as possible if Zero.</param>
        public ReplayablePointCsvInputAdapterConfig(string file, TimeSpan interval)
        {
            this.File = file;
            this.Interval = interval;
        }

        /// <summary>
        /// Gets the CSV file from which to read events.
        /// </summary>
        [DataMember]
        public string File { get; private set; }

        /// <summary>
        /// Gets a real-time pace at which to produce events from the target file. If this is TimeSpan.Zero, events will be produced as fast as possible.
        /// </summary>
        [DataMember]
        public TimeSpan Interval { get; private set; }
    }
}
