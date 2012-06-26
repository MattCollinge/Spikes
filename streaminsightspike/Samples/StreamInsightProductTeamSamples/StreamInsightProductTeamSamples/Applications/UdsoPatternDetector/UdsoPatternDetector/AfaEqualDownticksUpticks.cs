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

namespace StreamInsight.Samples.PatternDetector
{
    using Microsoft.ComplexEventProcessing;
    using StreamInsight.Samples.UserExtensions.Afa;

    /// <summary>
    /// The input is a sequence of stock price quotes.
    /// We define an augmented finite automaton (AFA) that looks for a sequence of downticks, immediately 
    /// followed by an equal number of upticks.
    /// In this case, the register (counter) keeps track of '#downticks - #upticks', and when 
    /// this reaches 0, we have found the pattern.
    /// </summary>
    public class AfaEqualDownticksUpticks : IAfaDescriptorFactory<StockTick, Register>
    {
        /// <summary>
        /// Defines a transition function that checks for a downtick and increments register
        /// if the event is a downtick.
        /// </summary>
        /// <param name="inputEvent">The incoming event.</param>
        /// <param name="oldRegister">The old register value.</param>
        /// <param name="newRegister">Output the new register value after transition.</param>
        /// <returns>Whether or not the transition was successful.</returns>
        public static bool Transition0(PointEvent<StockTick> inputEvent, Register oldRegister, out Register newRegister)
        {
            // Check if the event is a downtick.
            if (inputEvent.Payload.PriceChange < 0)
            {
                newRegister = new Register { Counter = oldRegister.Counter + 1 };
                return true;
            }

            newRegister = oldRegister;
            return false;
        }

        /// <summary>
        /// Defines a transition function that checks for an uptick, with current counter 
        /// value greater than one. If yes, we decrement the counter register.
        /// </summary>
        /// <param name="inputEvent">The incoming event.</param>
        /// <param name="oldRegister">The old register value.</param>
        /// <param name="newRegister">Output the new register value after transition.</param>
        /// <returns>Whether or not the transition was successful.</returns>
        public static bool Transition1(PointEvent<StockTick> inputEvent, Register oldRegister, out Register newRegister)
        {
            // Check if the event is an uptick and the register (counter) is greater than 1
            if ((inputEvent.Payload.PriceChange >= 0)
                && (oldRegister.Counter > 1))
            {
                newRegister = new Register { Counter = oldRegister.Counter - 1 };
                return true;
            }

            newRegister = oldRegister;
            return false;
        }

        /// <summary>
        /// Defines a transition function that checks for an uptick, with current counter 
        /// value of exactly one. If yes, we decrement the counter register. This arc will
        /// be used to transition to the final state.
        /// </summary>
        /// <param name="inputEvent">The incoming event.</param>
        /// <param name="oldRegister">The old register value.</param>
        /// <param name="newRegister">Output the new register value after transition.</param>
        /// <returns>Whether or not the transition was successful.</returns>
        public static bool Transition2(PointEvent<StockTick> inputEvent, Register oldRegister, out Register newRegister)
        {
            // Check if the event is an uptick and the register (counter) is exactly 1
            if ((inputEvent.Payload.PriceChange >= 0) &&
                (oldRegister.Counter == 1))
            {
                newRegister = new Register { Counter = oldRegister.Counter - 1 };
                return true;
            }

            newRegister = oldRegister;
            return false;
        }

        /// <summary>
        /// Constructs the actual automaton, and specify its properties.
        /// </summary>
        /// <returns>The AFA descriptor.</returns>
        public AfaDescriptor<StockTick, Register> CreateDescriptor()
        {
            AfaDescriptor<StockTick, Register> afaDescriptor =
                new AfaDescriptor<StockTick, Register>();

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
            afaDescriptor.DefaultRegister = new Register { Counter = 0 };

            // This AFA does not see CTI events, and hence CtiVisibility is set to false.
            // If CtiVisibility is true, CTIs will be seen by arcs (transition functions).
            // In the latter case, the transition functions above will need to be modified
            // to check the kind of an incoming event before processing it.
            afaDescriptor.CtiVisibility = false;

            // This AFA allows a new pattern matching instance to start at every new event,
            // and hence AllowOverlappingInstances is set to true.
            // If AllowOverlappingInstances is instead set to false, an incoming event will 
            // not start a new pattern matching instance unless:
            // (1) there are no ongoing (in-progress) matches, or
            // (2) all ongoing matches end due to the current incoming event.
            afaDescriptor.AllowOverlappingInstances = true;

            return afaDescriptor;
        }
    }
}
