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

namespace StreamInsight.Samples.UserExtensions.Afa
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.ComplexEventProcessing;

    /// <summary>
    /// Class that describes an AFA completely, along with its properties.
    /// </summary>
    /// <typeparam name="TInput">The event type.</typeparam>
    /// <typeparam name="TRegister">The register type.</typeparam>
    public sealed class AfaDescriptor<TInput, TRegister>
    {
        /// <summary>
        /// The set of final states in the AFA.
        /// </summary>
        private readonly List<int> _finalStates = new List<int>();

        /// <summary>
        /// The arcs present in the AFA.
        /// </summary>
        private readonly Dictionary<int, Dictionary<int, TransitionDelegate<TInput, TRegister>>> _transitionInfo = new Dictionary<int, Dictionary<int, TransitionDelegate<TInput, TRegister>>>();

        /// <summary>
        /// The start state in the AFA.
        /// </summary>
        public int StartState { get; set; }

        /// <summary>
        /// The default value of the register.
        /// </summary>
        public TRegister DefaultRegister { get; set; }

        /// <summary>
        /// This property specifies whether or not AFA arcs can see CTI events.
        /// </summary>
        public bool CtiVisibility { get; set; }

        /// <summary>
        /// This property specifies whether, on a new incoming event, we allow new pattern 
        /// matches to be initiated while an existing pattern is still being matched.
        /// </summary>
        public bool AllowOverlappingInstances { get; set; }

        /// <summary>
        /// Add a final state to the AFA.
        /// </summary>
        /// <param name="state">The state being added as a final state.</param>
        public void AddFinalState(int state)
        {
            this._finalStates.Add(state);
        }

        /// <summary>
        /// Adds an arc to the AFA.
        /// </summary>
        /// <param name="fromState">The state from which the arc begins.</param>
        /// <param name="toState">The state at which the arc ends.</param>
        /// <param name="arc">The arc, defined as a method.</param>
        public void AddArc(int fromState, int toState, TransitionDelegate<TInput, TRegister> arc)
        {
            if (IsFinalState(fromState))
            {
                throw new InvalidOperationException(string.Format("AFA cannot have outgoing arcs from final state {0}.", fromState));
            }

            if (!this._transitionInfo.ContainsKey(fromState))
            {
                this._transitionInfo.Add(fromState, new Dictionary<int, TransitionDelegate<TInput, TRegister>>());
            }

            if (!this._transitionInfo[fromState].ContainsKey(toState))
            {
                this._transitionInfo[fromState].Add(toState, arc);
            }
            else
            {
                this._transitionInfo[fromState][toState] = arc;
            }
        }

        /// <summary>
        /// Checks whether a given state is a final state.
        /// </summary>
        /// <param name="state">The AFA state.</param>
        /// <returns>Whether or not the state is a final state.</returns>
        internal bool IsFinalState(int state)
        {
            return this._finalStates.Contains(state);
        }

        /// <summary>
        /// Gets the set of states in the AFA, that are reachable from a given state.
        /// </summary>
        /// <param name="fromState">The AFA state.</param>
        /// <returns>The set of states reachable from the given state.</returns>
        internal List<int> GetToStates(int fromState)
        {
            List<int> result = new List<int>();
            Dictionary<int, TransitionDelegate<TInput, TRegister>> tmp;
            if (this._transitionInfo.TryGetValue(fromState, out tmp))
            {
                foreach (KeyValuePair<int, TransitionDelegate<TInput, TRegister>> kvp in tmp)
                {
                    result.Add(kvp.Key);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks whether an AFA arc is triggered by a given incoming event and current register value.
        /// </summary>
        /// <param name="inputEvent">The input event.</param>
        /// <param name="fromState">The AFA from-state.</param>
        /// <param name="toState">The AFA to-state.</param>
        /// <param name="oldRegister">The old register value</param>
        /// <param name="newRegister">Output the new register value</param>
        /// <returns>Whether or not the transition was successful.</returns>
        internal bool TryApplyTransition(PointEvent<TInput> inputEvent, int fromState, int toState, TRegister oldRegister, out TRegister newRegister)
        {
            TransitionDelegate<TInput, TRegister> arc;
            if (this._transitionInfo.ContainsKey(fromState) && this._transitionInfo[fromState].TryGetValue(toState, out arc))
            {
                return arc(inputEvent, oldRegister, out newRegister);
            }

            newRegister = default(TRegister);
            return false;
        }
    }
}
