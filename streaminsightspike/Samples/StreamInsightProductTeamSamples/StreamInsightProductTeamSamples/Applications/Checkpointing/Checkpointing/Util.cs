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

    /// <summary>
    /// Some global utilities.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// This is the amount of time that any thread in a spin loop should delay
        /// if there is no action to take. The smaller this is, the more the system will
        /// burn CPU spinning; the larger it is, the less lively the system may be.
        /// </summary>
        public const int ThreadSpinDelay = 100;

        /// <summary>
        /// Write a message to whatever log we're using. Probably just the console...
        /// </summary>
        /// <param name="source">An indication of where the message is from.</param>
        /// <param name="message">The message.</param>
        public static void Log(string source, string message)
        {
            Console.WriteLine("[" + source + "]: " + message);
        }
    }
}
