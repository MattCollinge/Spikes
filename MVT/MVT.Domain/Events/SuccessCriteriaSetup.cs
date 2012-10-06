using System;
using CtM.EDA;

namespace MVT.Domain.Events
{
    public class SuccessCriteriaSetup : Event
    {
        public Guid SuccessCriteriaId;
        public string SuccessCriteriaName;
    }
}