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
    using System;
    using System.Data.SqlClient;
    using System.Globalization;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    public class SqlInputInterval : IntervalInputAdapter, ISqlInputAdapter<IntervalEvent>
    {
        private SqlInputAdapter<SqlInputInterval, IntervalEvent> inputAdapter;

        private SqlInputConfig config;

        public SqlInputInterval(SqlInputConfig config, CepEventType eventType)
        {
            // check if the config provides the required timestamp fields
            if ((config.StartTimeColumnName == null) || (config.StartTimeColumnName.Length == 0))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    @"* * * StartTimeStamp field not specified for this input adapter * * *"));
            }

            if ((config.EndTimeColumnName == null) || (config.EndTimeColumnName.Length == 0))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    @"* * * EndTimeStamp field not specified for the input Interval adapter * * *"));
            }

            this.config = config;
            this.inputAdapter = new SqlInputAdapter<SqlInputInterval, IntervalEvent>(config, eventType, this);
        }

        public void SetEventTime(IntervalEvent evt, SqlDataReader reader)
        {
            evt.StartTime = reader.GetDateTime(reader.GetOrdinal(this.config.StartTimeColumnName));
            evt.EndTime = reader.GetDateTime(reader.GetOrdinal(this.config.EndTimeColumnName));
        }

        public override void Start()
        {
            this.inputAdapter.Start();
        }

        public override void Resume()
        {
            this.inputAdapter.Resume();
        }

        public override void Stop()
        {
            this.inputAdapter.Cleanup();
            this.Stopped();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.inputAdapter.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}