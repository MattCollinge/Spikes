using System;
using CtM.EDA;

namespace MVT.Domain.Events
{
    public class MVTestPaused : Event
    {
        public Guid TestId;
        public string PausedBy;
    }
}