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
    using System.Runtime.Serialization;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Extensibility;

    /// <summary>
    /// The UDSO that performs pattern detection over a stream, given an AFA.
    /// </summary>
    /// <typeparam name="TInput">The event type.</typeparam>
    /// <typeparam name="TRegister">The register type.</typeparam>
    /// <typeparam name="TFactory">The type that produces an AFA descriptor for pattern detection.</typeparam>
    [DataContract]
    public sealed class AfaOperator<TInput, TRegister, TFactory> : CepPointStreamOperator<TInput, TRegister>
        where TFactory : IAfaDescriptorFactory<TInput, TRegister>, new()
    {
        /// <summary>
        /// The maximum duration of a pattern.
        /// </summary>
        [DataMember(Name = "PatternMaxDuration")]
        private readonly TimeSpan _patternMaxDuration;
        
        /// <summary>
        /// Descriptor for the AFA to be detected.
        /// </summary>
        private AfaDescriptor<TInput, TRegister> _descriptor;

        /// <summary>
        /// The current event start time.
        /// </summary>
        [DataMember(Name = "StartTime")]
        private DateTimeOffset? _startTime;

        /// <summary>
        /// The current set of (partial) pattern matches.
        /// </summary>
        [DataMember(Name = "CurrentRuns")]
        private LinkedList<Run> _currentRuns;

        /// <summary>
        /// The previous set of (partial) pattern matches.
        /// </summary>
        [DataMember(Name = "PreviousRuns")]
        private LinkedList<Run> _previousRuns;

        /// <summary>
        /// Initializes a new instance of AfaOperator.
        /// </summary>
        /// <param name="patternMaxDuration">The maximum duration of a pattern.</param>
        public AfaOperator(TimeSpan patternMaxDuration)
        {
            this._patternMaxDuration = patternMaxDuration;
            _descriptor = new TFactory().CreateDescriptor();
        }

        /// <summary>
        /// What to do on deserialization.
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            _descriptor = new TFactory().CreateDescriptor();
        }

        /// <summary>
        /// Is the UDSO internal state empty?
        /// </summary>
        public override bool IsEmpty
        {
            get { return _currentRuns == null && _previousRuns == null; }
        }

        /// <summary>
        /// Processes an incoming event.
        /// </summary>
        /// <param name="inputEvent">The incoming event.</param>
        /// <returns>The IEnumerable of registers corresponding to detected patterns.</returns>
        public override IEnumerable<TRegister> ProcessEvent(PointEvent<TInput> inputEvent)
        {
            // Process only the appropriate events
            if (!_descriptor.CtiVisibility && inputEvent.EventKind == EventKind.Cti)
            {
                yield break;
            }

            // Check if time has advanced; if yes, update _previousRuns and reset _currentRuns
            if (_startTime.HasValue && _startTime.Value < inputEvent.StartTime)
            {
                _previousRuns = _currentRuns;
                _currentRuns = null;
            }

            _startTime = inputEvent.StartTime;
            bool runsEnded = true;

            // Process this event: create new runs from existing runs associated with the previous timestamp
            if (_previousRuns != null)
            {
                foreach (Run oldRun in _previousRuns)
                {
                    IEnumerable<Run> newRuns = ApplyTransition(inputEvent.StartTime, inputEvent, oldRun);
                    foreach (var newRun in newRuns)
                    {
                        if (_currentRuns == null)
                            _currentRuns = new LinkedList<Run>();
                        _currentRuns.AddLast(newRun);

                        // If we have reached a final state, include the register value in result.
                        if (_descriptor.IsFinalState(newRun.State))
                        {
                            yield return newRun.Register;
                        }
                        else
                        {
                            runsEnded = false;
                        }
                    }
                }
            }

            if (_descriptor.AllowOverlappingInstances || runsEnded)
            {
                // Create new runs starting from this event
                Run run = new Run(_descriptor.StartState, _descriptor.DefaultRegister, inputEvent.StartTime);
                IEnumerable<Run> newRuns = ApplyTransition(inputEvent.StartTime, inputEvent, run);
                foreach (var newRun in newRuns)
                {
                    if (_currentRuns == null)
                        _currentRuns = new LinkedList<Run>();
                    _currentRuns.AddLast(newRun);

                    // If we have reached a final state, include the register value in result.
                    if (_descriptor.IsFinalState(newRun.State))
                        yield return newRun.Register;
                }
            }
        }

        /// <summary>
        /// Applies a transition on an incoming event.
        /// </summary>
        /// <param name="startTime">The event start time.</param>
        /// <param name="inputEvent">The input event.</param>
        /// <param name="previousRun">The previous partial match.</param>
        /// <returns>The set of new (partial) matches after applying the transition.</returns>
        private IEnumerable<Run> ApplyTransition(DateTimeOffset startTime, PointEvent<TInput> inputEvent, Run previousRun)
        {
            if (previousRun.PatternStartTime.Add(_patternMaxDuration) <= startTime)
            {
                yield break;
            }
            int fromState = previousRun.State;

            List<int> toStates = _descriptor.GetToStates(fromState);

            foreach (int toState in toStates)
            {
                TRegister targetRegister;
                if (_descriptor.TryApplyTransition(inputEvent, fromState, toState, previousRun.Register, out targetRegister))
                {
                    Run newRun = new Run(toState, targetRegister, previousRun.PatternStartTime);
                    yield return newRun;
                }
            }
        }

        /// <summary>
        /// Data structure defining a (possibly partial) pattern match.
        /// </summary>
        [DataContract]
        private struct Run
        {
            /// <summary>
            /// The current register value.
            /// </summary>
            [DataMember]
            private readonly TRegister _register;

            /// <summary>
            /// The current state.
            /// </summary>
            [DataMember]
            private readonly int _state;

            /// <summary>
            /// The start time of the pattern.
            /// </summary>
            [DataMember]
            private readonly DateTimeOffset _patternStartTime;

            /// <summary>
            /// Initializes a new partial pattern match.
            /// </summary>
            /// <param name="state"></param>
            /// <param name="register"></param>
            /// <param name="patternStartTime"></param>
            internal Run(int state, TRegister register, DateTimeOffset patternStartTime)
            {
                this._register = register;
                this._state = state;
                this._patternStartTime = patternStartTime;
            }

            /// <summary>
            /// Gets the current register value.
            /// </summary>
            public TRegister Register
            {
                get { return _register; }
            }

            /// <summary>
            /// Gets the current state.
            /// </summary>
            public int State
            {
                get { return _state; }
            }

            /// <summary>
            /// Gets the pattern start time.
            /// </summary>
            public DateTimeOffset PatternStartTime
            {
                get { return _patternStartTime; }
            }
        }
    }
}
