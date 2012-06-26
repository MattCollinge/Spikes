//*********************************************************
//
// Copyright 2010 Microsoft Corporation
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

namespace StreamInsight.Samples.PatternDetector
{
    /// <summary>
    /// Stock ticker information type.
    /// </summary>
    public class StockTick
    {
        /// <summary>
        /// Gets or sets the stock symbol
        /// </summary>
        public string StockSymbol { get; set; }

        /// <summary>
        /// Gets or sets the stock price
        /// </summary>
        public int StockPrice { get; set; }

        /// <summary>
        /// Gets or sets the change in stock price
        /// </summary>
        public int PriceChange { get; set; }
    }

    /// <summary>
    /// Event type representing the register of the transition network. A register
    /// is used to maintain additional state (context) during pattern matching using
    /// an augmented finite automaton. For example, if we want to detect a sequence
    /// of downticks followed by a sequence of the same number of upticks, we can
    /// use the register as a counter to keep track of the number of downticks that
    /// are yet to be matched by an uptick.
    /// </summary>
    public class RegisterType
    {
        /// <summary>
        /// Gets or sets the register counter, which tracks #downticks - #upticks
        /// </summary>
        public int Counter { get; set; }
    }
}
