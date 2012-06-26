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

    internal sealed class EventSet<TInput>
    {
        private readonly DateTimeOffset startTime;
        private DateTimeOffset endTime = DateTimeOffset.MaxValue;
        private readonly List<TInput> eventList = new List<TInput>();

        public EventSet(DateTimeOffset _startTime)
        {
            this.startTime = _startTime;
        }

        public DateTimeOffset StartTime
        {
            get { return this.startTime; }
        }

        public DateTimeOffset EndTime
        {
            get { return this.endTime; }
        }

        public List<TInput> Events
        {
            get { return this.eventList; }
        }

        public void Add(IntervalEvent<TInput> evt)
        {
            this.eventList.Add(evt.Payload);
            if (evt.EndTime < this.endTime)
            {
                this.endTime = evt.EndTime;
            }
        }
    }
}
