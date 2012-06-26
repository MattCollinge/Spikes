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
    using System.Globalization;
    using System.IO;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;

    /// <summary>
    /// Defines inputs to Hitchhiker's Guide sample queries.
    /// </summary>
    static class InputData
    {
        /// <summary>
        /// Creates a stream source based on an IEnumerable.
        /// </summary>
        /// <param name="application">The StreamInsight application hosting the stream.</param>
        /// <returns>Stream endpoint that can be used to compose StreamInsight queries.</returns>
        public static CepStream<TollReading> CreateTollDataInput(Application application)
        {
            return CreateTollDataSource().ToStream(application, AdvanceTimeSettings.IncreasingStartTime, "TollStream");
        }

        /// <summary>
        /// Reads the contents of the TollInput.txt data file to an IEnumerable source.
        /// </summary>
        /// <returns>Toll data.</returns>
        public static IEnumerable<IntervalEvent<TollReading>> CreateTollDataSource()
        {
            foreach (string line in File.ReadLines(@"Data\TollInput.txt"))
            {
                string[] fields = line.Split(',');

                yield return IntervalEvent.CreateInsert(
                    DateTime.Parse(fields[0], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                    DateTime.Parse(fields[1], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                    new TollReading
                    {
                        TollId = fields[2],
                        LicensePlate = fields[3],
                        State = fields[4],
                        Make = fields[5],
                        Model = fields[6],
                        VehicleType = int.Parse(fields[7], CultureInfo.InvariantCulture),
                        VehicleWeight = string.IsNullOrWhiteSpace(fields[8])
                            ? default(float)
                            : float.Parse(fields[8], CultureInfo.InvariantCulture),
                        Toll = float.Parse(fields[9], CultureInfo.InvariantCulture),
                        Tag = fields[10],
                    });
            }
        }
    }
}
