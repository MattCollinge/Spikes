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
   public class SetupSuccessCriteriaSpec : DomainTestFixture<SetupSuccessCriteria, SuccessCriteriaAR>

    {
        private Guid _successCriteriaARId;
       private string _successCriteriaName;

       protected override IEnumerable<Event> Given()
       {
           _successCriteriaARId = Guid.NewGuid();
           _successCriteriaName = "Test Success Criteria";
           return new Event[0];
       }

       protected override SetupSuccessCriteria When()
        {
              return new SetupSuccessCriteria()
            {
                SuccessCriteriaId = _successCriteriaARId,
                SuccessCriteriaName= _successCriteriaName
            };
        }

        protected override Action<SetupSuccessCriteria, IRepository<SuccessCriteriaAR>> CommandHandler()
        {
            return (cmd, repository) =>
            {
                var ar = new SuccessCriteriaAR(cmd.SuccessCriteriaId, cmd.SuccessCriteriaName);
                repository.Save(ar, ar.Version);
            };
        }

        [Then]
        public void SuccessCriteriaShouldRaiseEvent()
        {
            Assert.That(this.PublishedEvents.First(), Is.Not.Null);
        }

        [Then]
        public void SuccessCriteriaShouldRaiseSetupEvent()
        {
            Assert.That(this.PublishedEvents.First(), Is.InstanceOf<SuccessCriteriaSetup>());
        }

        [Then]
        public void SuccessCriteriaSetupEventShouldHaveCorrectProperties()
        {
            var @event = this.PublishedEvents.First() as SuccessCriteriaSetup;

            Assert.That(@event.SuccessCriteriaId , Is.EqualTo(_successCriteriaARId));
            Assert.That(@event.SuccessCriteriaName,Is.EqualTo(_successCriteriaName));
        }
    }
}
