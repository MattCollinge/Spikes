using System;
using CtM.EDA;

namespace MVT.Domain.Events
{
    public class NewMVTestSetup :Event 
    {
        public Guid TestId;
        public String TestName;
    }
}