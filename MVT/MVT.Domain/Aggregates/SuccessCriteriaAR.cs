using System;
using CtM.EDA;
using MVT.Domain.Events;

namespace MVT.Domain.Aggregates
{
    public class SuccessCriteriaAR : AggregateRoot
    {
        private Guid _id;
        private string _name;

        public SuccessCriteriaAR()
        {

        }

        public SuccessCriteriaAR(Guid successCriteriaId, string successCriteriaName)
        {
            var @event = new SuccessCriteriaSetup()
                             {
                                 SuccessCriteriaId = successCriteriaId,
                                 SuccessCriteriaName = successCriteriaName
                             };

            ApplyChange(@event);
        }

        public override Guid Id
        {
            get { return _id; }
        }

        private void Apply(SuccessCriteriaSetup e)
        {
            _id = e.SuccessCriteriaId;
            _name = e.SuccessCriteriaName;
        }
    }
}