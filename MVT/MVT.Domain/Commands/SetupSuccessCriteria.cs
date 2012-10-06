using System;
using CtM.EDA;

namespace MVT.Domain.Commands
{
    public class SetupSuccessCriteria : Command
    {
        public Guid SuccessCriteriaId;
        public string SuccessCriteriaName;
    }
}