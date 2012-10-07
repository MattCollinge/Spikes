using System;
using System.Collections.Generic;
using System.Linq;
using CtM.EDA;
using CtM.EDA.Tests;
using MVT.Domain.Aggregates;
using MVT.Domain.Commands;
using MVT.Domain.Events;
using NUnit.Framework;

namespace MVT.Domain.Tests
{
    public class RegisterMVTestExposureSpec : DomainTestFixture<RegisterExposure, MVTestAR>
    {
        private Guid _MVTestVisitorARId;
        private Guid _MVTestId;
        private Guid _visitorId;
        private Guid _accountId;
        private Guid _visitId;
        private Guid _testARId;
        private string _variantAName;
        private string _variantBName;
        private Dictionary<string, int> _variantWeightSchedule;


        protected override IEnumerable<Event> Given()
        {
            _testARId = Guid.NewGuid();
            _variantAName = "Test VariantA Name";
            _variantBName = "Test VariantB Name";
            _variantWeightSchedule = new Dictionary<string, int> {{_variantAName, 20}, {_variantBName, 80}};

            yield return new NewMVTestSetup()
            {
                TestId = _testARId,
                TestName = "Test Name"
            };

            yield return new NewVariantSetup()
            {
                TestId = _testARId,
                VariantName = _variantAName
            };

            yield return new NewVariantSetup()
            {
                TestId = _testARId,
                VariantName = _variantBName
            };

            yield return new VariantWeightScheduleApplied()
            {
                TestId = _testARId,
                VariantWeightSchedule = _variantWeightSchedule
            };
        }

        protected override RegisterExposure When()
        {
             _MVTestVisitorARId = Guid.NewGuid();
            _visitorId = Guid.NewGuid();
            _visitId = Guid.NewGuid();
            _accountId = Guid.NewGuid();

            return new RegisterExposure()
                       {
                           MVTestVisitorId = _MVTestVisitorARId,
                           MVTestId = _MVTestId,
                           VisitorId = _visitorId,
                           AccountId = _accountId,
                           VisitId = _visitId
                       };
        }

        protected override Action<RegisterExposure, IRepository<MVTestAR>> CommandHandler()
        {
            return (cmd, repository) =>
                       {
                           //Get MVTestAR, Inject a DomainService to Hash VisitorId, TestId and Get Variant Weighting and Properties
                           //Pass MVTTestVisitorAR into MVTestAR
                           var MVTAR = repository.GetById(cmd.MVTestId);
                          
                           var MVTestVisitorAR = new MVTestVisitorAR(cmd.MVTestVisitorId); //Not sure I like this but hey ho - exploring...
                           var registerationDoubleDispatch = (IRegisterExposureToVariants) MVTestVisitorAR;

                           MVTAR.RegisterExposure(ref registerationDoubleDispatch, cmd.VisitorId, cmd.VisitId, cmd.AccountId);

                           repository.Save(MVTestVisitorAR, MVTestVisitorAR.Version);
                       };

        }

        [Then]
        public void MVTestVisitorShouldCreateEvent()
        {
            Assert.That(this.PublishedEvents.First(), Is.Not.Null);
        }

        [Then]
        public void MVTestVisitorShouldCreateMVTestVisitorExposedEvent()
        {
            Assert.That(this.PublishedEvents.First() as MVTestVisitorExposed, Is.Not.Null);
        }

        [Then]
        public void MVTestShouldCreateMVTestVisitorExposedEventWithExpectedProperties()
        {
            var @event = this.PublishedEvents.First() as MVTestVisitorExposed;

            Assert.That(@event.MVTestId, Is.EqualTo(_testARId));
            Assert.That(@event.VariantId, Is.EqualTo(_variantAName));
            Assert.That(@event.VisitorId, Is.EqualTo(_visitorId));
            Assert.That(@event.AccountId, Is.EqualTo(_accountId));
            Assert.That(@event.VisitId, Is.EqualTo(_visitId));
        }
    }
}
