using System;
using System.Collections.Generic;

namespace CtM.EDA.Tests
{
    [Specification]
    public abstract class DomainTestFixture<TCommand, TAggregate>
        where TCommand : Command
        where TAggregate : AggregateRoot, new() 
    {
        protected IEnumerable<Event> PublishedEvents { get; private set; }
        protected Exception CaughtException;
        protected abstract IEnumerable<Event> Given();
        protected abstract TCommand When();
        protected virtual void Finally() { }
        protected abstract Action<TCommand, IRepository<TAggregate>> CommandHandler();

        [Given]
        public void Setup()
        {

            var testRepository = new TestRepository<TAggregate>(Given());

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