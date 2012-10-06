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
    class ApplyVariantWeightScheduleSpec : DomainTestFixture<ApplyVariantWeightSchedule, MVTestAR>

    {
        private Guid _testARId;
        private string _variantAName;
        private string _variantBName; 
        private Dictionary<string, int> _variantWeightSchedule;


        protected override IEnumerable<Event> Given()
        {
            _testARId = Guid.NewGuid();
            _variantAName = "Test VariantA Name";
            _variantBName = "Test VariantB Name";

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

        }

        protected override ApplyVariantWeightSchedule When()
        {
            _variantWeightSchedule = new Dictionary<string, int>();
            _variantWeightSchedule.Add(_variantAName, 20);
            _variantWeightSchedule.Add(_variantBName, 80);

            return new ApplyVariantWeightSchedule()
            {
                TestId = _testARId,
                VariantWeightSchedule= _variantWeightSchedule
            };
        }

        protected override Action<ApplyVariantWeightSchedule, IRepository<MVTestAR>> CommandHandler()
        {
            return (cmd, repository) =>
            {
                var ar = repository.GetById(cmd.TestId);
                ar.SetVariantWeightSchedule(cmd.VariantWeightSchedule);
                repository.Save(ar, ar.Version);
            };
        }

        [Then]
        public void MVTestShouldCreateEvent()
        {
            Assert.That(this.PublishedEvents.First(), Is.Not.Null);
        }

        [Then]
        public void MVTestShouldCreateSetVariantWeightScheduleEvent()
        {
            Assert.That(this.PublishedEvents.First() as VariantWeightScheduleApplied, Is.Not.Null);
        }

        [Then]
        public void MVTestShouldCreateSetVariantWeightScheduleEventWithExpectedProperties()
        {
            var @event = this.PublishedEvents.First() as VariantWeightScheduleApplied;

            Assert.That(@event.TestId, Is.EqualTo(_testARId));
            Assert.That(@event.VariantWeightSchedule, Is.EqualTo(_variantWeightSchedule));
        }
    }
}
