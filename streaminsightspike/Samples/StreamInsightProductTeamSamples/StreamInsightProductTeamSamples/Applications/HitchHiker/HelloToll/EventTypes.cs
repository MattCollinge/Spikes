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
    using System.Text;

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
    }

    public class TollCount
    {
        public double Count { get; set; }
    }

    public class Toll
    {
        public string TollId { get; set; }
        public double TollAmount { get; set; }
        public long VehicleCount { get; set; }
    }

    public class TollAverage
    {
        public string TollId { get; set; }
        public double AverageToll { get; set; }
    }

    public class TollCountSum
    {
        public double Toll { get; set; }
    }

    public class TollCompare
    {
        public string TollId1 { get; set; }
        public string TollId2 { get; set; }
        public long VehicleCount { get; set; }
    }

    public class TopEvents
    {
        public int TollRank { get; set; }
        public string TollId { get; set; }
        public double TollAmount { get; set; }
        public long VehicleCount { get; set; }
    }

    public class AccountInfo
    {
        public string AccountId { get; set; }
        public string Name { get; set; }
        public string ZipCode { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
    }

    public class TagInfo
    {
        public string TagId { get; set; }
        public DateTime RenewalDate { get; set; }
        public bool IsReportedLostOrStolen { get; set; }
        public string AccountId { get; set; }
    }

    public class VehicleRatio
    {
        public float Value { get; set; }
    }

    public class TollViolation
    {
        public string TollId { get; set; }
        public string LicensePlate { get; set; }
        public string State { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Tag { get; set; }
    }

    public class TollOuterJoin
    {
        public string LicensePlate { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public float? Toll { get; set; }
        public string TollId { get; set; }
    }

    public class VehicleWeightInfo
    {
        public string LicensePlate { get; set; }
        public double? Weight { get; set; }
        public double? WeightCharge { get; set; }
    }

    public class VehicleWeightConfigInfo
    {
        public double WeightCharge { get; set; }
    }
}
