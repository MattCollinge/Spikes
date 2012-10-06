using System;
using CtM.EDA;

namespace MVT.Domain.Commands
{
    public class AssociateSuccessCriteriaWithMVTest : Command
    {
        public Guid TestId;
        public Guid SuccessCriteriaId;
    }
}