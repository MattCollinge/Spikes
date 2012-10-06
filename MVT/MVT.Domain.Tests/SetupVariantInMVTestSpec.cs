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
    public class SetupVariantInMVTestSpec : DomainTestFixture<SetupVariant, MVTestAR>
    {
        private Guid _testARId;
        private string _variantName;

        protected override IEnumerable<Event> Given()
        {
            _testARId = Guid.NewGuid();

            yield return new NewMVTestSetup()
            {
                TestId = _testARId,
                TestName = "Test Name"
            };
        }

        protected override SetupVariant When()
        {
            _variantName = "New MVTest Variant Name";
            return new SetupVariant()
                          {
                              TestId=_testARId,
                              VariantName=_variantName
                          };
        }

        protected override Action<SetupVariant, IRepository<MVTestAR>> CommandHandler()
        {
            return (cmd, repository) =>
                       {
                           var ar = repository.GetById(cmd.TestId);
                               ar.SetUpNewVariant(cmd.VariantName);
                           repository.Save(ar, ar.Version);
                       };

        }

        [Then]
        public void MVTestShouldCreateEvent()
        {
            Assert.That(this.PublishedEvents.First(), Is.Not.Null);
        }

        [Then]
        public void MVTestShouldCreateNewVariantSetupEvent()
        {
            Assert.That(this.PublishedEvents.First() as NewVariantSetup, Is.Not.Null);
        }

        [Then]
        public void MVTestShouldCreateNewVariantSetupEventWithExpectedProperties()
        {
            var @event = this.PublishedEvents.First() as NewVariantSetup;

            Assert.That(@event.TestId, Is.EqualTo(_testARId));
            Assert.That(@event.VariantName, Is.EqualTo(_variantName));
        }
    }
}
