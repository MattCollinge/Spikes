//*********************************************************
//
// Copyright Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of
// the License at
//
// http://www.apache.org/licenses/LICENSE-2.0 
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES
// OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES
// OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache 2 License for the specific language
// governing permissions and limitations under the License.
//
//*********************************************************

namespace StreamInsight.Samples.Checkpointing
{
    using System;
    using System.Linq;
    using Microsoft.ComplexEventProcessing;

    /// <summary>
    /// A few extension methods for making working with queries a little easier.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Get the state of a query.
        /// </summary>
        /// <param name="q">The query.</param>
        /// <returns>The state (as a string).</returns>
        public static string State(this Query q)
        {
            try
            {
                return q.Application.Server.GetDiagnosticView(q.Name).GetProperty<string>(DiagnosticViewProperty.QueryState);
            }
            catch (ManagementException)
            {
                // It's possible that the query has, at this point, been removed. Returning null here makes it a little
                // simpler to deal with.
                return null;
            }
        }

        /// <summary>
        /// Get the query template for a query.
        /// </summary>
        /// <param name="q">The query.</param>
        /// <returns>The QueryTemplate object for the query.</returns>
        public static QueryTemplate Template(this Query q)
        {
            return q.Application.QueryTemplates[q.QueryTemplateName.Segments.Last()];
        }
    }
}
