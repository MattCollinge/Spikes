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

namespace StreamInsight.Samples.Adapters.Sql
{
    using System.Data.SqlClient;

    /// <summary>
    /// Interface for a concrete adapter class.
    /// Contains necessary API aspects to implement a Sql input adapter.
    /// </summary>
    /// <typeparam name="TEvent">Event Type</typeparam>
    public interface ISqlInputAdapter<TEvent> : IInputAdapter<TEvent>
    {
        void SetEventTime(TEvent evt, SqlDataReader reader);
    }
}
