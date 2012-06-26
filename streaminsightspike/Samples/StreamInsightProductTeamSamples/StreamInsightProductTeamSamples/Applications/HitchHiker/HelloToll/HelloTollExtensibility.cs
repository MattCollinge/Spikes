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

namespace StreamInsight.Samples.HelloToll
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Extensibility;
    using Microsoft.ComplexEventProcessing.Linq;

    /// <summary> method templates for extension methods </summary>
    public static partial class UserDefinedExtensionMethods
    {
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

        [CepUserDefinedOperator(typeof(VehicleWeights))]
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

    public class VehicleWeights : CepTimeSensitiveOperator<TollReading, VehicleWeightInfo>
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
