using System;
using CtM.EDA;

namespace MVT.Domain.Commands
{
    public class RegisterExposure : Command
    {
        public Guid MVTestVisitorId;
        public Guid MVTestId;
        public Guid VisitorId;
        public Guid AccountId;
        public Guid VisitId;
    }
}