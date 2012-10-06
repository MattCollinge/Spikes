using System;
using CtM.EDA;

namespace MVT.Domain.Commands
{
    public class SetupVariant : Command
    {
        public string VariantName;
        public Guid TestId;
    }
}