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

namespace StreamInsight.Samples.SequenceIntegration.HitchhikersGuide
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Extensibility;
    using Microsoft.ComplexEventProcessing.Linq;

    /// <summary> method templates for extension methods </summary>
    public static partial class Extensibility
    {
        /// <summary>
        /// Simulation of a reference database; for the user defined function to search against this.
        /// In reality, this could be a database, or an in-memory cache, or another input stream.
        /// </summary>
        public static IList<TagInfo> TagList = new List<TagInfo>
        {
           new TagInfo { TagId = "123456789", RenewalDate = new DateTime(2009, 2, 20), IsReportedLostOrStolen = false, AccountId = "NJ100001JET1109" },
           new TagInfo { TagId = "234567891", RenewalDate = new DateTime(2008, 12, 6), IsReportedLostOrStolen = true, AccountId = "NY100002GNT0109" },
           new TagInfo { TagId = "345678912", RenewalDate = new DateTime(2008, 9, 1), IsReportedLostOrStolen = true, AccountId = "CT100003YNK0210" }
        }.AsReadOnly();

        /// <summary>
        /// User-defined function indicating whether a tag is lost or stolen.
        /// </summary>
        /// <param name="tagId">Tag ID</param>
        /// <returns>true if the tag is lost or stolen; false otherwise</returns>
        public static bool IsLostOrStolen(this string tagId)
        {
            foreach (var tag in TagList)
            {
                if (tag.TagId.Equals(tagId) && tag.IsReportedLostOrStolen)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// User-defined function indicating whether a tag is expired.
        /// </summary>
        /// <param name="tagId">Tag ID</param>
        /// <returns>true if the tag is expired; false otherwise</returns>
        public static bool IsExpired(this string tagId)
        {
            foreach (var tag in TagList)
            {
                if (tag.TagId.Equals(tagId) && (DateTime.Now - tag.RenewalDate) > TimeSpan.FromDays(365))
                {
                    return true;
                }
            }

            return false;
        }

        [CepUserDefinedAggregate(typeof(OutOfStateVehicleRatio))]
        public static float OutOfStateVehicleRatio<TPayload>(this CepWindow<TPayload> window)
        {
            throw CepUtility.DoNotCall();
        }

        [CepUserDefinedAggregate(typeof(OutOfStateVehicleRatio2))]
        public static float OutOfStateVehicleRatio2<TPayload>(this CepWindow<TPayload> window, Expression<Func<TPayload, string>> expression)
        {
            throw CepUtility.DoNotCall();
        }

        [CepUserDefinedOperator(typeof(VehicleWeightOperator))]
        public static VehicleWeightInfo VehicleWeights(this CepWindow<TollReading> window)
        {
            throw CepUtility.DoNotCall();
        }
    }

    public class OutOfStateVehicleRatio : CepAggregate<TollReading, float>
    {
        public override float GenerateOutput(IEnumerable<TollReading> payloads)
        {
            float tempCount = 0;
            float totalCount = 0;
            foreach (var tollReading in payloads)
            {
                totalCount++;
                if (!tollReading.State.Equals("NY"))
                {
                    tempCount++;
                }
            }

            return tempCount / totalCount;
        }
    }

    public class OutOfStateVehicleRatio2 : CepAggregate<string, float>
    {
        public override float GenerateOutput(IEnumerable<string> payloads)
        {
            float tempCount = 0;
            float totalCount = 0;
            foreach (var state in payloads)
            {
                totalCount++;
                if (!state.Equals("NY"))
                {
                    tempCount++;
                }
            }

            return tempCount / totalCount;
        }
    }

    public class VehicleWeightOperator : CepTimeSensitiveOperator<TollReading, VehicleWeightInfo>
    {
        private double weightcharge = 0.5; // defined as a constant within the UDO; this could be passed as a config if required

        public override IEnumerable<IntervalEvent<VehicleWeightInfo>> GenerateOutput(IEnumerable<IntervalEvent<TollReading>> events, WindowDescriptor windowDescriptor)
        {
            List<IntervalEvent<VehicleWeightInfo>> output = new List<IntervalEvent<VehicleWeightInfo>>();

            // Identify any commercial vehicles in this window for the given window duration
            var commercial = events.Where(e => e.StartTime.Hour >= 0 && e.Payload.VehicleType == 2);
            if (commercial.Any())
            {
                foreach (var e in commercial)
                {
                    // create an output interval event
                    IntervalEvent<VehicleWeightInfo> vehicleWeightEvent = CreateIntervalEvent();

                    // populate the output interval event
                    vehicleWeightEvent.StartTime = e.StartTime;
                    vehicleWeightEvent.EndTime = e.EndTime;
                    vehicleWeightEvent.Payload = new VehicleWeightInfo
                    {
                        LicensePlate = e.Payload.LicensePlate,
                        Weight = e.Payload.VehicleWeight,

                        // here is the interesting part; note how the output is dependent on
                        // the start and end timestamps of the input event. The weight charge
                        // is a function of the rush hour definition, the weigh charge factor
                        // and the vehicle tonnage itself
                        WeightCharge = ((e.StartTime.Hour >= 7 && e.StartTime.Hour <= 14) ? 2 : 1) * this.weightcharge * e.Payload.VehicleWeight
                    };

                    // output the event via the IEnumerable interface
                    output.Add(vehicleWeightEvent);
                }
            }

            return output;
        }
    }
}
