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
   public class PauseMVTestSpec : DomainTestFixture<PauseMVTest, MVTestAR>
    {
       private Guid _testARId;
       private string _pauseRequestedBy;

       protected override IEnumerable<Event> Given()
       {
           _testARId = Guid.NewGuid();

           yield return new NewMVTestSetup()
                            {
                                TestId=_testARId,
                                TestName="Test Name"
                            };
       }

       protected override PauseMVTest When()
       {
            _pauseRequestedBy = "Test user";

           return new PauseMVTest()
                     {
                         TestId=_testARId,
                         PauseRequestBy=_pauseRequestedBy
                     };
       }

       protected override Action<PauseMVTest, IRepository<MVTestAR>> CommandHandler()
       {
           return (cmd, repository) =>
           {
               var ar = repository.GetById(cmd.TestId);
               ar.Pause(cmd.PauseRequestBy);
               repository.Save(ar, ar.Version);
           };
       }

       [Then]
       public void MVTestShouldCreateEvent()
       {
           Assert.That(this.PublishedEvents.First(), Is.Not.Null);
       }

       [Then]
       public void MVTestShouldCreateMVTestPausedEvent()
       {
           Assert.That(this.PublishedEvents.First() as MVTestPaused, Is.Not.Null);
       }

       [Then]
       public void MVTestShouldCreateMVTestPausedEventWithExpectedProperties()
       {
           var @event = this.PublishedEvents.First() as MVTestPaused;

           Assert.That(@event.TestId, Is.EqualTo(_testARId));
           Assert.That(@event.PausedBy, Is.EqualTo(_pauseRequestedBy));
       }
    }

    public class PauseMVTest : Command
    {
        public Guid TestId;
        public string PauseRequestBy;
    }
}
