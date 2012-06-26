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
    /// <summary>
    /// A simple payload containing an x and y coordinate.
    /// </summary>
    public class XYPayload
    {
        /// <summary>
        /// Initializes a new instance of the XYPayload class with a default (0,0) payload.
        /// </summary>
        public XYPayload()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XYPayload class with a specified payload.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public XYPayload(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Gets or sets the y coordinate of the payload.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the x coordinate of the payload.
        /// </summary>
        public double X { get; set; }
    }
}