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

namespace StreamInsight.Samples.Checkpointing.RandomDataGenerator
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// This produces CSV files of the form:
    ///     date,x,y
    /// Where the dates increase in one-second increments from a user-defined start,
    /// and x and y values are random in [0,1].
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Generate random data into a CSV file.
        /// </summary>
        /// <param name="args">Command line arguments: the date to start at, the number of events to generate, and a target file.</param>
        private static void Main(string[] args)
        {
            DateTimeOffset curTime;
            int count;
            string path;

            // Get the command-line arguments.
            if (!ReadCommandLine(args, out curTime, out count, out path))
            {
                PrintUsage();
                return;
            }

            // Just produce data every second.
            TimeSpan interval = TimeSpan.FromSeconds(1.0);

            Random r = new Random();

            using (StreamWriter writer = new StreamWriter(path))
            {
                // produce the data
                for (int i = 0; i < count; i++, curTime += interval)
                {
                    writer.WriteLine(string.Format("{0},{1},{2}", curTime, r.NextDouble(), r.NextDouble()));
                }
            }
        }

        /// <summary>
        /// Read the command line.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="startDate">Output the starting date.</param>
        /// <param name="count">Output the number of lines to generate.</param>
        /// <param name="path">Output the path to which we should write.</param>
        /// <returns>True if the read was successful; false otherwise.</returns>
        private static bool ReadCommandLine(string[] args, out DateTimeOffset startDate, out int count, out string path)
        {
            // Set some default values...
            startDate = DateTimeOffset.MinValue;
            count = 0;
            path = null;

            // Check number of arguments.
            if (args.Length != 3)
            {
                Console.WriteLine(string.Format("Incorrect number of arguments: expected 3, got {0}.", count));
                return false;
            }

            // Get the start date.
            try
            {
                startDate = DateTimeOffset.Parse(args[0]);
            }
            catch (Exception)
            {
                Console.WriteLine("Could not parse start date: '{0}'.", args[0]);
                return false;
            }
            
            // Get the count.
            try
            {
                count = int.Parse(args[1]);
            }
            catch (Exception)
            {
                Console.WriteLine("Could not parse count: '{0}'.", args[1]);
                return false;
            }

            // Get the path.
            path = args[2];
            return true;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: RandomDataGenerator <start date> <count> <target file>");
        }
    }
}
