using System;
using System.Collections.Generic;

namespace CtM.EDA.Tests
{
    public class TestRepository<TAggregate> : IRepository<TAggregate>
        where TAggregate : AggregateRoot, new()
    {
        private readonly IEnumerable<Event> _givenEvents;

        public IEnumerable<Event> UncommittedChanges { get; private set; } 

        public TestRepository(IEnumerable<Event> given)
        {
            _givenEvents = given;
        }

        public void Save(AggregateRoot aggregate, int expectedVersion)
        {
            //Store uncommitted changes
            UncommittedChanges = aggregate.GetUncommittedChanges();
        }

        public TAggregate GetById(Guid id)
        {
            var sut = new TAggregate();
            sut.LoadsFromHistory(_givenEvents);
            return sut;
        }
    }
}