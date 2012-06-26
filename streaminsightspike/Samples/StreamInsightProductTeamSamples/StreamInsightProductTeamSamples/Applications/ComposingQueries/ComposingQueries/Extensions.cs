//*********************************************************
//
//  Copyright (c) Microsoft. All rights reserved.
//  This code is licensed under the Apache 2.0 License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OR
//  CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED,
//  INCLUDING, WITHOUT LIMITATION, ANY IMPLIED WARRANTIES
//  OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

namespace StreamInsight.Samples.ComposingQueries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Extensibility;
    using Microsoft.ComplexEventProcessing.Linq;

    /// <summary>
    /// A UDA to compute the delta on a given event field between the first
    /// and the last event in the window.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Uda", Justification = "StreamInsight specific terminology")]
    public class DeltaUda : CepTimeSensitiveAggregate<float, float>
    {
        /// <summary>
        /// Computes the aggregation over a window.
        /// </summary>
        /// <param name="events">Set of events contained in the window.</param>
        /// <param name="windowDescriptor">Window definition.</param>
        /// <returns>Aggregation result.</returns>
        public override float GenerateOutput(IEnumerable<IntervalEvent<float>> events, WindowDescriptor windowDescriptor)
        {
            // Make sure that the events are ordered in time.
            var orderedEvent = events.OrderBy(e => e.StartTime);

            return orderedEvent.Last().Payload - orderedEvent.First().Payload;
        }
    }

    /// <summary>
    /// Extension methods to expose the UDAs/UDOs in LINQ.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Uda", Justification = "StreamInsight specific terminology")]
    public static class UdaExtensionMethods
    {
        /// <summary>
        /// Extension method for the UDA.
        /// </summary>
        /// <typeparam name="T">Payload type of the stream.</typeparam>
        /// <param name="window">Window to be passed to the UDA</param>
        /// <param name="map">Mapping from the payload to a float field in the payload.</param>
        /// <returns>Aggregation result.</returns>
        [CepUserDefinedAggregate(typeof(DeltaUda))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "window", Justification = "Extension method is not executed.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "map", Justification = "Extension method is not executed.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Extension method design.")]
        public static float Delta<T>(this CepWindow<T> window, Expression<Func<T, float>> map)
        {
            throw CepUtility.DoNotCall();
        }
    }
}