using System;
using System.Collections.Generic;

namespace CtM.EDA.Tests
{
    [Specification]
    public abstract class SagaTestFixture<TInitiatingEvent, TEvent, TSaga>
        where TEvent : Event
        where TInitiatingEvent : Event 
        where TSaga : Saga<TInitiatingEvent>, new() 
    {
        protected IEnumerable<Command> IssuedCommands { get; private set; }
        protected IEnumerable<Event> PublishedEvents { get; private set; }
        protected Exception CaughtException;
        protected abstract IEnumerable<Event> Given();
        protected abstract TEvent When();
        protected virtual void Finally() { }
        protected abstract Action<TEvent, TSaga> EventHandler();

        [Given]
        public void Setup()
        {
            var sut = new TSaga();
            sut.LoadsFromHistory(Given());

            try
            {
               EventHandler()(When(), sut);
               PublishedEvents = sut.GetUncommittedChanges();
               IssuedCommands = sut.GetUnissuedCommands();
            }
            catch (Exception exception)
            {
                CaughtException = exception;
            }
            finally
            {
                Finally();
            }
        }

    }
}