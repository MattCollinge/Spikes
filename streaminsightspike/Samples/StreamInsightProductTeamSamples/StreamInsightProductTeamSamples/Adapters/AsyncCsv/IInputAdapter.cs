// *********************************************************
//
//  Copyright (c) Microsoft. All rights reserved.
//  This code is licensed under the Apache 2.0 License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OR
//  CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
//  INCLUDING, WITHOUT LIMITATION, ANY IMPLIED WARRANTIES
//  OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
// *********************************************************

namespace StreamInsight.Samples.Adapters.AsyncCsv
{
    using Microsoft.ComplexEventProcessing.Adapters;

    /// <summary>
    /// Interface for a concrete adapter class.
    /// Contains all necessary API aspects to implement an input adapter.
    /// </summary>
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
