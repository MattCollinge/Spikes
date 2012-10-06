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
    public class SetupMVTestSpec : DomainTestFixture<SetUpNewMVTest, MVTestAR>
    {
        private Guid _testARId;
        private string _testName;

        protected override IEnumerable<Event> Given()
        {
            yield return null;
        }

        protected override SetUpNewMVTest When()
        {
            _testARId = Guid.NewGuid();
            _testName = "New MVTest Name";
            return new SetUpNewMVTest()
                          {
                              TestId=_testARId,
                              TestName=_testName   
                          };
        }

        protected override Action<SetUpNewMVTest, IRepository<MVTestAR>> CommandHandler()
        {
            return (cmd, repository) =>
                       {
                           var ar = new MVTestAR(cmd.TestId, cmd.TestName);
                           repository.Save(ar, ar.Version);
                       };

        }

        [Then]
        public void MVTestShouldCreateEvent()
        {
            Assert.That(this.PublishedEvents.First(), Is.Not.Null);
        }

        [Then]
        public void MVTestShouldCreateNewMVTestSetupEvent()
        {
            Assert.That(this.PublishedEvents.First() as NewMVTestSetup, Is.Not.Null);
        }

        [Then]
        public void MVTestShouldCreateNewMVTestSetupEventWithExpectedProperties()
        {
            var @event = this.PublishedEvents.First() as NewMVTestSetup;

            Assert.That(@event.TestId, Is.EqualTo(_testARId));
            Assert.That(@event.TestName, Is.EqualTo(_testName));
        }
    }
}
