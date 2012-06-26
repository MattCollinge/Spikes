using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ComplexEventProcessing.Adapters;

namespace EDA.Analytics.MassTransitAdapter
{
    public interface IInputAdapter<TEvent>
    {
        AdapterState AdapterState { get; }

        TEvent CreateInsertEvent();

        EnqueueOperationResult Enqueue(ref TEvent eventInstance);

        void ReleaseEvent(ref TEvent eventInstance);

        void Ready();

        void Stopped();
    }
}
