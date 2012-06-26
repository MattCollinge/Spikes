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

    public class SqlInputPoint : PointInputAdapter, ISqlInputAdapter<PointEvent>
    {
        private SqlInputAdapter<SqlInputPoint, PointEvent> inputAdapter;

        private SqlInputConfig config;

        public SqlInputPoint(SqlInputConfig config, CepEventType eventType)
        {
            // check if the config provides the required timestamp fields
            if ((config.StartTimeColumnName == null) || (config.StartTimeColumnName.Length == 0))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    @"* * * StartTimeStamp field not specified for this input adapter * * *"));
            }

            this.config = config;
            this.inputAdapter = new SqlInputAdapter<SqlInputPoint, PointEvent>(config, eventType, this);
        }

        public void SetEventTime(PointEvent evt, SqlDataReader reader)
        {
            evt.StartTime = reader.GetDateTime(reader.GetOrdinal(this.config.StartTimeColumnName));
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