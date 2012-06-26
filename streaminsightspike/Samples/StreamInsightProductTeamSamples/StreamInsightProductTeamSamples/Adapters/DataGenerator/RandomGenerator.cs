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

namespace StreamInsight.Samples.Adapters.DataGenerator
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// Class to generate random numbers, using the RNGCryptoServiceProvider.
    /// </summary>
    internal sealed class RandomGenerator
    {
        /// <summary>
        /// Memory to fill by the random generator.
        /// </summary>
        private static byte[] bytes = new byte[4];

        /// <summary>
        /// More accurate random number generator.
        /// </summary>
        private static RandomNumberGenerator rng = RNGCryptoServiceProvider.Create();

        /// <summary>
        /// Mutex for the random data generation.
        /// </summary>
        private static object thisLock = new object();

        /// <summary>
        /// Prevents a default instance of the RandomGenerator class from being created.
        /// </summary>
        private RandomGenerator()
        {
        }

        /// <summary>
        /// Creates an accurate randum number.
        /// </summary>
        /// <param name="maxValue">Upper bound for the random number.</param>
        /// <returns>A random number between 0 (inclusive) and maxValue (exclusive).</returns>
        public static int GetRandomNumber(int maxValue)
        {
            lock (thisLock)
            {
                if (maxValue == 0)
                    return 0;

                bytes = new byte[4];
                rng.GetBytes(bytes);
                int rndNum = BitConverter.ToInt32(bytes, 0);
                return Math.Abs(rndNum % maxValue);
            }
        }
    }
}
