using System;
using System.Collections.Generic;

namespace CtM.EDA.Tests
{
    //[Specification]
    //public abstract class SimpleSagaTestFixture<TEvent, TSaga>
    //    where TEvent : Event
    //    where TSaga : SimpleSaga, new() 
    //{
    //    protected IEnumerable<Command> IssuedCommands { get; private set; }
    //    protected IEnumerable<Event> PublishedEvents { get; private set; }
    //    protected Exception CaughtException;
    //    protected abstract IEnumerable<Event> Given();
    //    protected abstract TEvent When();
    //    protected virtual void Finally() { }
    //    protected abstract Action<TEvent, TSaga> EventHandler();

    //    [Given]
    //    public void Setup()
    //    {
    //        var sut = new TSaga();

    //        sut.InPlaybackMode = true;
    //        foreach (var givenEvent in Given())
    //        {
    //            sut.Handle(givenEvent);
    //        }
    //        sut.InPlaybackMode = false;
            
    //        try
    //        {
    //            EventHandler()(When(), sut);
    //            PublishedEvents = sut.GetUncommittedChanges();
    //            IssuedCommands = sut.GetUnissuedCommands();
    //        }
    //        catch (Exception exception)
    //        {
    //            CaughtException = exception;
    //        }
    //        finally
    //        {
    //            Finally();
    //        }
    //    }

    //}

    //public class SimpleSaga
    //{
    //}
}