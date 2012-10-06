using System;
using System.Collections.Generic;
using System.Linq;

namespace CtM.EDA.Tests
{
    [Specification]
    public abstract class DomainTestFixture<TCommand, TAggregate>
        where TCommand : Command
        where TAggregate : AggregateRoot, new()
    {
        private IEnumerable<Event> _publishedEvents;

        protected IEnumerable<Event> PublishedEvents
        {
            get { return _publishedEvents ?? new Event[0]; }
            private set { _publishedEvents = value; }
        }

        protected Exception CaughtException;
        protected abstract IEnumerable<Event> Given();
        protected abstract TCommand When();
        protected virtual void Finally() { }
        protected abstract Action<TCommand, IRepository<TAggregate>> CommandHandler();

        [Given]
        public void Setup()
        {

            var testRepository = new TestRepository<TAggregate>(Given().ToList());

            try
            {
                CommandHandler()(When(), testRepository);
                PublishedEvents = testRepository.UncommittedChanges;
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