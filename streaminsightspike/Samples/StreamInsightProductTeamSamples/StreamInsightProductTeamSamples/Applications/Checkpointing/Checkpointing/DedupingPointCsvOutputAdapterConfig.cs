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
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Configuration for a DedupingPointCsvOutputAdapter.
    /// </summary>
    [DataContract]
    public class DedupingPointCsvOutputAdapterConfig
    {
        /// <summary>
        /// Initializes a new instance of the DedupingPointCsvOutputAdapterConfig class by providing the 
        /// target file for its output. This file will be cleared if it is a new executiton, and appended to 
        /// if this is a replay.
        /// </summary>
        /// <param name="file">The path to the target output file.</param>
        /// <param name="fields">The list of fields to write, in the order they will be written.</param>
        public DedupingPointCsvOutputAdapterConfig(string file, IEnumerable<string> fields)
        {
            this.File = file;
            this.Fields = fields;
        }

        /// <summary>
        /// Initializes a new instance of the DedupingPointCsvOutputAdapterConfig class by providing the 
        /// target file for its output. This file will be cleared if it is a new executiton, and appended to 
        /// if this is a replay. This will write the fields in arbitrary order.
        /// </summary>
        /// <param name="file">The path to the target output file.</param>
        public DedupingPointCsvOutputAdapterConfig(string file)
            : this(file, null)
        {
        }

        /// <summary>
        /// Gets a path to write the resulting CSV file to.
        /// </summary>
        [DataMember]
        public string File { get; private set; }

        /// <summary>
        /// Gets list of fields to write, in the order they will be written. This may be null, indicating that
        /// all fields will be written in arbitrary order.
        /// </summary>
        [DataMember]
        public IEnumerable<string> Fields { get; private set; }
    }
}
