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

    public class SqlOutputPoint : PointOutputAdapter, ISqlOutputAdapter<PointEvent>
    {
        private SqlOutputAdapter<SqlOutputPoint, PointEvent> outputAdapter;

        private SqlOutputConfig config;

        public SqlOutputPoint(SqlOutputConfig config, CepEventType eventType)
        {
            // check if the config provides the required timestamp fields
            if ((config.StartTimeColumnName == null) || (config.StartTimeColumnName.Length == 0))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    @"* * * StartTimeStamp field not specified for this output adapter * * *"));
            }

            this.config = config;
            this.outputAdapter = new SqlOutputAdapter<SqlOutputPoint, PointEvent>(config, eventType, this);
        }

        public void SetEventTime(SqlCommand command, PointEvent evt)
        {
            command.Parameters["@" + this.config.StartTimeColumnName].Value = evt.StartTime;
            command.Parameters["@" + this.config.EndTimeColumnName].Value = DBNull.Value;
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