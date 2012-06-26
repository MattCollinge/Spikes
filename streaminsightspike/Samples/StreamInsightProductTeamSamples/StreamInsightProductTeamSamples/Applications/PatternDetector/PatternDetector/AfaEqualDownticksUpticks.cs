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
    using System.Collections.Generic;
    using StreamInsight.Samples.UserExtensions.Afa;

    // The input is a sequence of stock price ticks.
    // We define an augmented finite automaton (AFA) that looks for a sequence of downticks, immediately 
    // followed by an equal number of upticks.
    // In this case, the register (counter) keeps track of '#downticks - #upticks', and when 
    // this reaches 0, we have found the pattern.
    public class AfaEqualDownticksUpticks : IAfaDescriptorFactory<StockTick, RegisterType>
    {
        // Define a transition function that checks for a downtick and increments register
        // if the event is a downtick. Note that since many events can have the same start
        // time, the transition functions accept (as input) a collection of events having 
        // the same start time.
        public static bool Transition0(IList<StockTick> inputEvents, RegisterType oldRegister, out RegisterType newRegister)
        {
            // Check if the event is a downtick.
            // NOTE: If there are many events with same timestamp, 
            // we simply look at the first event and ignore the rest 
            // (this will not happen if each stock tick for the 
            // same stock has a unique timestamp)
            if (inputEvents[0].PriceChange < 0)
            {
                newRegister = new RegisterType { Counter = oldRegister.Counter + 1 };
                return true;
            }

            newRegister = oldRegister;
            return false;
        }

        // Define a transition function that checks for an uptick, with current counter 
        // value greater than one. If yes, we decrement the counter register.
        public static bool Transition1(IList<StockTick> inputEvents, RegisterType oldRegister, out RegisterType newRegister)
        {
            // Check if the event is an uptick and the register (counter) is greater than 1
            if ((inputEvents[0].PriceChange >= 0)
                && (oldRegister.Counter > 1))
            {
                newRegister = new RegisterType { Counter = oldRegister.Counter - 1 };
                return true;
            }

            newRegister = oldRegister;
            return false;
        }

        // Define a transition function that checks for an uptick, with current counter 
        // value of exactly one. If yes, we decrement the counter register. This arc will
        // be used to transition to the final state.
        public static bool Transition2(IList<StockTick> inputEvents, RegisterType oldRegister, out RegisterType newRegister)
        {
            // Check if the event is an uptick and the register (counter) is exactly 1
            if ((inputEvents[0].PriceChange >= 0) &&
                (oldRegister.Counter == 1))
            {
                newRegister = new RegisterType { Counter = oldRegister.Counter - 1 };
                return true;
            }

            newRegister = oldRegister;
            return false;
        }

        public AfaDescriptor<StockTick, RegisterType> CreateDescriptor()
        {
            AfaDescriptor<StockTick, RegisterType> afaDescriptor =
                new AfaDescriptor<StockTick, RegisterType>();

            // Specify the start and final states
            afaDescriptor.StartState = 0;
            afaDescriptor.AddFinalState(2);

            // Specify the arcs in the automaton. See AFAexample.pptx
            // for a visualization of this automaton.
            //
            // If we see a downtick in the start state 0, we make 
            // two transitions: one stays in state 0 and the other
            // progresses the automaton to state 1. In both cases,
            // the counter is incremented by 1.
            afaDescriptor.AddArc(0, 0, Transition0);
            afaDescriptor.AddArc(0, 1, Transition0);

            // If we see an uptick, and the counter is greater than
            // 1, we decrement the counter and stay in the same state
            // (state 1) because the pattern has not yet been found
            afaDescriptor.AddArc(1, 1, Transition1);

            // If we see an uptick, and the counter is equal to 1, we 
            // decrement the counter (so it becomes 0) and move to state 2,
            // which is a final state and produces a pattern match output
            afaDescriptor.AddArc(1, 2, Transition2);

            // Specify the default (initial) register content
            afaDescriptor.DefaultRegister = new RegisterType { Counter = 0 };

            return afaDescriptor;
        }
    }
}
