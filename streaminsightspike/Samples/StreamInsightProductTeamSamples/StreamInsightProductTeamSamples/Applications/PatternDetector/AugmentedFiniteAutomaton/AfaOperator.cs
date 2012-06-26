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
    using System;
    using System.Collections.Generic;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Extensibility;

    public sealed class AfaOperator<TInput, TRegister> : CepTimeSensitiveOperator<TInput, TRegister>
    {
        private readonly AfaDescriptor<TInput, TRegister> _descriptor;

        public AfaOperator(AfaDescriptor<TInput, TRegister> descriptor)
        {
            _descriptor = descriptor;
        }

        public AfaOperator(string factoryTypeName)
        {
            // Load the AFA descriptor using the fully qualified type name
            Type inputType = Type.GetType(factoryTypeName, true);

            if (!typeof(IAfaDescriptorFactory<TInput, TRegister>).IsAssignableFrom(inputType))
            {
                throw new InvalidOperationException(string.Format("Supplied type {0} does not implemented the required interface {1}.",
                    inputType, typeof(IAfaDescriptorFactory<TInput, TRegister>)));
            }

            IAfaDescriptorFactory<TInput, TRegister> afaDescriptorGenerator = (IAfaDescriptorFactory<TInput, TRegister>)Activator.CreateInstance(inputType);
            _descriptor = afaDescriptorGenerator.CreateDescriptor();
        }

        public override IEnumerable<IntervalEvent<TRegister>> GenerateOutput(IEnumerable<IntervalEvent<TInput>> events, WindowDescriptor windowDescriptor)
        {
            SortedDictionary<DateTimeOffset, EventSet<TInput>> allEvents = new SortedDictionary<DateTimeOffset, EventSet<TInput>>();

            foreach (IntervalEvent<TInput> evt in events)
            {
                if (evt.StartTime.AddTicks(1) != evt.EndTime)
                {
                    // This condition implies an event that is not a point event. This AFA
                    // operator supports only point events, hence we raise an exception.
                    throw new InvalidOperationException("Received an event that is not a point event. The AFA operator only supports point events.");
                }

                EventSet<TInput> eventSet;
                if (!allEvents.TryGetValue(evt.StartTime, out eventSet))
                {
                    eventSet = new EventSet<TInput>(evt.StartTime);
                    allEvents.Add(evt.StartTime, eventSet);
                }

                eventSet.Add(evt);
            }

            List<IntervalEvent<TRegister>> result = new List<IntervalEvent<TRegister>>();
            LinkedList<Run> previousRuns = null;
            foreach (var item in allEvents)
            {
                previousRuns = this.Insert(result, windowDescriptor.EndTime, previousRuns, item.Value);
            }

            return result;
        }

        private LinkedList<Run> Insert(List<IntervalEvent<TRegister>> result, DateTimeOffset endTime, LinkedList<Run> previousRuns, EventSet<TInput> inputEvents)
        {
            // Insert assumes that EventSets are fed in increasing order of timestamps
            LinkedList<Run> currentRuns = new LinkedList<Run>();

            // Create new runs from existing runs that end at immediately preceeding sequence number
            if (previousRuns != null)
            {
                foreach (Run previousRun in previousRuns)
                {
                    ApplyTransition(result, endTime, previousRun, inputEvents, currentRuns);
                }
            }

            // Create new runs starting from the new sequence number
            Run run = new Run(_descriptor.StartState, _descriptor.DefaultRegister);
            ApplyTransition(result, endTime, run, inputEvents, currentRuns);

            if (currentRuns.Count == 0)
            {
                currentRuns = null;
            }

            return currentRuns;
        }

        private void ApplyTransition(List<IntervalEvent<TRegister>> result, DateTimeOffset endTime, Run previousRun, EventSet<TInput> inputEvents, LinkedList<Run> currentRuns)
        {
            int fromState = previousRun.State;

            List<int> toStates = _descriptor.GetToStates(fromState);

            foreach (int toState in toStates)
            {
                TRegister targetRegister;
                if (_descriptor.TryApplyTransition(inputEvents, fromState, toState, previousRun.Register, out targetRegister))
                {
                    Run newRun = new Run(toState, targetRegister);

                    if (_descriptor.IsFinalState(toState))
                    {
                        // If we have reached a final state, include the register value in result.
                        IntervalEvent<TRegister> outputEvent = CreateIntervalEvent();
                        outputEvent.Payload = newRun.Register;
                        outputEvent.StartTime = inputEvents.StartTime;
                        outputEvent.EndTime = endTime;
                        result.Add(outputEvent);
                    }

                    currentRuns.AddLast(newRun);
                }
            }
        }

        private struct Run
        {
            private readonly TRegister _register;
            private readonly int _state;

            internal Run(int state, TRegister register)
            {
                this._register = register;
                this._state = state;
            }

            public TRegister Register
            {
                get { return _register; }
            }

            public int State
            {
                get { return _state; }
            }
        }
    }
}
