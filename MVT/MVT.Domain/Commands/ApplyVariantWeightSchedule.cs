using System;
using System.Collections.Generic;
using CtM.EDA;

namespace MVT.Domain.Commands
{
    public class ApplyVariantWeightSchedule : Command
    {
        public Guid TestId;
        public string VariantName;
        public Dictionary<string, int> VariantWeightSchedule;
    }
}