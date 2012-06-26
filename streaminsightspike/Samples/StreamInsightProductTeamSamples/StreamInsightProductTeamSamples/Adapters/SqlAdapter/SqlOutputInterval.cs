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

    public class SqlOutputInterval : IntervalOutputAdapter, ISqlOutputAdapter<IntervalEvent>
    {
        private SqlOutputAdapter<SqlOutputInterval, IntervalEvent> outputAdapter;

        private SqlOutputConfig config;

        public SqlOutputInterval(SqlOutputConfig config, CepEventType eventType)
        {
            // check if the config provides the required timestamp fields
            if ((config.StartTimeColumnName == null) || (config.StartTimeColumnName.Length == 0))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    @"* * * StartTimeStamp field not specified for this output adapter * * *"));
            }

            if ((config.EndTimeColumnName == null) || (config.EndTimeColumnName.Length == 0))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    @"* * * EndTimeStamp field not specified for the output Interval adapter * * *"));
            }

            this.config = config;
            this.outputAdapter = new SqlOutputAdapter<SqlOutputInterval, IntervalEvent>(config, eventType, this);
        }

        public void SetEventTime(SqlCommand command, IntervalEvent evt)
        {
            command.Parameters["@" + this.config.StartTimeColumnName].Value = evt.StartTime;
            command.Parameters["@" + this.config.EndTimeColumnName].Value = evt.EndTime;
        }

        public override void Start()
        {
            this.outputAdapter.Start();
        }

        public override void Resume()
        {
            this.outputAdapter.Resume();
        }

        public override void Stop()
        {
            this.outputAdapter.Cleanup();
            this.Stopped();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.outputAdapter.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}