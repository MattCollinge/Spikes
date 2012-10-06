using System;
using System.Collections.Generic;
using CtM.EDA;

namespace MVT.Domain.Events
{
    public class VariantWeightScheduleApplied : Event
    {
        public Dictionary<string,int> VariantWeightSchedule;
        public Guid TestId;
    }
}