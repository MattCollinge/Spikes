using System;
using CtM.EDA;

namespace MVT.Domain.Events
{
    public class SuccessCriteriaAssociated : Event
    {
        public Guid MVTestId;
        public Guid SuccessCriteriaId;
    }
}