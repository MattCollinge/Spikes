using System;
using CtM.EDA;

namespace MVT.Domain.Commands
{
    public class SetUpNewMVTest : Command
    {
        public Guid TestId;
        public string TestName;
    }
}