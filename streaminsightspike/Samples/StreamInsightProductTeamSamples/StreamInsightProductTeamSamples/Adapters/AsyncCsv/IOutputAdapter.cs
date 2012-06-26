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
    /// </summary>
    public interface IOutputAdapter<TEvent>
    {
        AdapterState AdapterState { get; }

        DequeueOperationResult Dequeue(out TEvent eventInstance);

        void Ready();

        void ReleaseEvent(ref TEvent eventInstance);

        void Stopped();
    }
}
