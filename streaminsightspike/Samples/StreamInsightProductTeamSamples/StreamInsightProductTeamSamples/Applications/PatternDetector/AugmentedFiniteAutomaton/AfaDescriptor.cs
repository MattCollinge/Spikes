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

namespace StreamInsight.Samples.UserExtensions.Afa
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class AfaDescriptor<TInput, TRegister>
    {
        private readonly List<int> _finalStates = new List<int>();
        private readonly Dictionary<int, Dictionary<int, TransitionDelegate<TInput, TRegister>>> _transitionInfo = new Dictionary<int, Dictionary<int, TransitionDelegate<TInput, TRegister>>>();

        public int StartState { get; set; }

        public TRegister DefaultRegister { get; set; }

        public void AddFinalState(int state)
        {
            this._finalStates.Add(state);
        }

        public void AddArc(int fromState, int toState, TransitionDelegate<TInput, TRegister> arc)
        {
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

        internal bool IsFinalState(int state)
        {
            return this._finalStates.Contains(state);
        }

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

        internal bool TryApplyTransition(EventSet<TInput> inputEvents, int fromState, int toState, TRegister oldRegister, out TRegister newRegister)
        {
            TransitionDelegate<TInput, TRegister> arc;
            if (this._transitionInfo.ContainsKey(fromState) && this._transitionInfo[fromState].TryGetValue(toState, out arc))
            {
                return arc(inputEvents.Events.AsReadOnly(), oldRegister, out newRegister);
            }

            newRegister = default(TRegister);
            return false;
        }
    }
}
