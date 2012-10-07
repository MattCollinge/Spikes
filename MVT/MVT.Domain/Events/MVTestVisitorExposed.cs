using System;
using CtM.EDA;

namespace MVT.Domain.Events
{
    public class MVTestVisitorExposed : Event
    {
        public Guid MVTestId;
        public Guid VisitorId;
        public Guid VisitId;
        public Guid AccountId;
        public string VariantId;
    }
}