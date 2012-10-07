using System;
using CtM.EDA;
using MVT.Domain.Events;

namespace MVT.Domain.Aggregates
{
    public class MVTestVisitorAR : AggregateRoot, IRegisterExposureToVariants
    {
        private readonly Guid _id;
        private Guid _MVTestId;
        private Guid _visitorId;
        private Guid _visitId;
        private Guid _accountId;
        private string _variantId;

        public MVTestVisitorAR()
        {
        }

        public MVTestVisitorAR(Guid mvTestVisitorId)
        {
            _id = mvTestVisitorId;
        }

        public override Guid Id
        {
            get { return _id; }
        }

        public void RegisterVisitorVariant(Guid MVTestId, Guid visitorId, Guid visitId, Guid accountId, string variantId)
        {
            var @event = new MVTestVisitorExposed()
                             {
                                 MVTestId = MVTestId,
                                 VisitorId = visitorId,
                                 VisitId = visitId,
                                 AccountId = accountId,
                                 VariantId = variantId 
                             };

            ApplyChange(@event);
        }

        private void Apply(MVTestVisitorExposed e)
        {
            _MVTestId = e.MVTestId;
            _visitorId = e.VisitorId;
            _visitId = e.VisitId;
            _accountId = e.AccountId;
            _variantId = e.VariantId;
        }
    }
}