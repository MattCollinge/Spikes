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

namespace StreamInsight.Samples.SequenceIntegration
{
    using System;

    public class TollReading
    {
        public string TollId { get; set; }
        public string LicensePlate { get; set; }
        public string State { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int VehicleType { get; set; } // 1 for passenger, 2 for commercial
        public float VehicleWeight { get; set; } // vehicle weight in tons, null for passenger vehicles
        public float Toll { get; set; }
        public string Tag { get; set; }

        public override string ToString()
        {
            return new { TollId, LicensePlate, State, Make, Model, VehicleType, VehicleWeight, Toll, Tag }.ToString();
        }
    }

    public class VehicleWeightInfo
    {
        public string LicensePlate { get; set; }
        public double? Weight { get; set; }
        public double? WeightCharge { get; set; }

        public override string ToString()
        {
            return new { LicensePlate, Weight, WeightCharge }.ToString();
        }
    }

    public class TagInfo
    {
        public string TagId { get; set; }
        public DateTime RenewalDate { get; set; }
        public bool IsReportedLostOrStolen { get; set; }
        public string AccountId { get; set; }

        public override string ToString()
        {
            return new { TagId, RenewalDate, IsReportedLostOrStolen, AccountId }.ToString();
        }
    }
}
