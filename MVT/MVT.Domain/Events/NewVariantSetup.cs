using System;
using CtM.EDA;

namespace MVT.Domain.Events
{
    public class NewVariantSetup :Event
    {
        public Guid TestId;
        public string VariantName;
    }
}