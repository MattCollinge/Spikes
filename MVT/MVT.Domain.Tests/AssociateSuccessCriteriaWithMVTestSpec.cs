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
    public class AssociateSuccessCriteriaWithMVTestSpec : DomainTestFixture<AssociateSuccessCriteriaWithMVTest, MVTestAR>
    {
        private Guid _testARId;
        private Guid _successCriteriaARId;

        protected override IEnumerable<Event> Given()
        {
            _testARId = Guid.NewGuid();

            yield return new NewMVTestSetup()
            {
                TestId = _testARId,
                TestName = "Test Name"
            };
        }

        protected override AssociateSuccessCriteriaWithMVTest When()
        {
            _successCriteriaARId = Guid.NewGuid();
            return new AssociateSuccessCriteriaWithMVTest()
                          {
                              TestId=_testARId,
                              SuccessCriteriaId= _successCriteriaARId
                          };
        }

        protected override Action<AssociateSuccessCriteriaWithMVTest, IRepository<MVTestAR>> CommandHandler()
        {
            return (cmd, repository) =>
                       {
                           var ar = repository.GetById(cmd.TestId);
                               ar.AssociateSuccesCriteria(cmd.SuccessCriteriaId);
                           repository.Save(ar, ar.Version);
                       };

        }

        [Then]
        public void MVTestShouldCreateEvent()
        {
            Assert.That(this.PublishedEvents.First(), Is.Not.Null);
        }

        [Then]
        public void MVTestShouldCreatSuccessCriteriaAssociatedEvent()
        {
            Assert.That(this.PublishedEvents.First() as SuccessCriteriaAssociated, Is.Not.Null);
        }

        [Then]
        public void MVTestShouldCreateNewVariantSetupEventWithExpectedProperties()
        {
            var @event = this.PublishedEvents.First() as SuccessCriteriaAssociated;

            Assert.That(@event.MVTestId, Is.EqualTo(_testARId));
            Assert.That(@event.SuccessCriteriaId, Is.EqualTo(_successCriteriaARId));
        }
    }
}
