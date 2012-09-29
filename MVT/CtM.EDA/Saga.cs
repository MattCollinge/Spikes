using System;
using System.Collections.Generic;

namespace CtM.EDA
{
    public abstract class Saga<TInitiatingEvent> : AggregateRoot
        where TInitiatingEvent : Event
    {
        public abstract Guid CorrelationId { get; }

        private readonly List<Command> _unissuedCommands = new List<Command>();
        private bool _inReplayMode;

        public abstract void Begin(TInitiatingEvent initiatingEvent);

        public override void LoadsFromHistory(IEnumerable<Event> history)
        {
            _inReplayMode = true; 
           base.LoadsFromHistory(history);
            _inReplayMode = false;
        }

     
        protected  void IssueCommand(Command cmdToIssue)
        {
            if (!_inReplayMode)
            _unissuedCommands.Add(cmdToIssue);
        }

        public IEnumerable<Command> GetUnissuedCommands()
        {
            return _unissuedCommands;
        }



    }
}