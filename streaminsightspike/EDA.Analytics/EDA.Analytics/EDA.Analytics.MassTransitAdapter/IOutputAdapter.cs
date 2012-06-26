using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ComplexEventProcessing.Adapters;

namespace EDA.Analytics.MassTransitAdapter
{
    public interface IOutputAdapter<TEvent>
    {
        AdapterState AdapterState { get; }

        DequeueOperationResult Dequeue(out TEvent eventInstance);

        void Ready();

        void ReleaseEvent(ref TEvent eventInstance);

        void Stopped();
    }
}
