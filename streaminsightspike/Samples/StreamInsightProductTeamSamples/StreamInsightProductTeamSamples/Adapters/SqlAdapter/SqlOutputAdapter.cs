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
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Threading;

    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Adapters;

    public sealed class SqlOutputAdapter<TOutputAdapter, TEvent> : IDisposable
        where TOutputAdapter : ISqlOutputAdapter<TEvent>
        where TEvent : UntypedEvent
    {
        /// <summary>
        /// Specific adapter object. Since it has to implement the IOutputAdapter
        /// interface, this object provides all necessary adapter interactions.
        /// </summary>
        private TOutputAdapter outputAdapter;

        /// <summary>
        /// The configuration object passed from the factory.
        /// </summary>
        private SqlOutputConfig config;

        /// <summary>
        /// Data Connection to output
        /// </summary>
        private SqlConnection connection;

        /// <summary>
        /// SQL Command object for the output
        /// </summary>
        private SqlCommand command;

        /// <summary>
        /// Event type to be produced by the adapter at runtime.
        /// </summary>
        private CepEventType bindTimeEventType;

        public SqlOutputAdapter(SqlOutputConfig configInfo, CepEventType eventType, TOutputAdapter outputAdapter)
        {
            this.outputAdapter = outputAdapter;
            this.config = configInfo;
            this.bindTimeEventType = eventType;

            ValidateConfigParameter(this.config.StartTimeColumnName);
            ValidateConfigParameter(this.config.EndTimeColumnName);
            ValidateConfigParameter(this.config.TableName);
        }

        private void ValidateConfigParameter(string config)
        {
            // Validate table name parameter
            char[] checks = { ';', '\'', '-', ']', '[' };
            if (-1 != config.IndexOfAny(checks)
                || config.Contains("/*") || config.Contains("*/") || config.Contains("xp_"))
            {
                throw new ArgumentException("Name must not contain any of the following: ; , ' - [ ] /* */ xp_");
            }
        }

        public void Dispose()
        {
            this.Cleanup();
        }

        internal void Start()
        {
            this.InitOutputData();
            this.ConsumeEvents();
        }

        internal void Resume()
        {
            this.ConsumeEvents();
        }

        /// <summary>
        /// Release event memory, release DB resources. Since Cleanup()
        /// will be called from multiple places in exception and non-exception
        /// conditions, nullify the released resource.
        /// </summary>
        internal void Cleanup()
        {
            if (this.outputAdapter.AdapterState == AdapterState.Stopped)
            {
                return;
            }

            if (this.command != null)
            {
                this.command.Dispose();
                this.command = null;
            }

            if ((this.connection != null) && (this.connection.State == ConnectionState.Open))
            {
                this.connection.Close();
                this.connection = null;
            }
        }

        /// <summary>
        /// Initialize DB connections and validates the output schema against event type
        /// </summary>
        private void InitOutputData()
        {
            string statement = default(string);
            IList<string> fields;
            IList<SqlDbType> fieldProviderTypes;
            bool tableExists = false;

            // Open connection to DB
            this.connection = new SqlConnection(this.config.ConnectionString);
            this.connection.Open();

            // check if the target output table exists
            DataTable dataTable = this.connection.GetSchema("Tables");
            foreach (DataRow row in dataTable.Rows)
            {
                if (this.config.TableName.Equals(row[2]))
                {
                    tableExists = true;
                    break;
                }
            }

            if (tableExists)
            {
                // If table exists, "poke" it to identify the schema (aka list of fields)
                statement = @"SET FMTONLY ON; SELECT * FROM [" + this.config.TableName + "]; SET FMTONLY OFF";
                this.command = new SqlCommand(statement, this.connection);

                using (SqlDataReader reader = this.command.ExecuteReader())
                {
                    // Retrieve the output table schema from the query (sans the timestamp column(s))
                    dataTable = reader.GetSchemaTable();
                    fields = new List<string>();
                    fieldProviderTypes = new List<SqlDbType>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string columnName = (string)row["ColumnName"];
                        if (!(columnName.Equals(this.config.StartTimeColumnName) || columnName.Equals(this.config.EndTimeColumnName)))
                        {
                            fields.Add(columnName);
                            fieldProviderTypes.Add((SqlDbType)(int)row["ProviderType"]);
                        }
                    }
                }

                // check if the event type has the same number of fields as the output table
                if (fields.Count != this.bindTimeEventType.Fields.Count)
                {
                    this.Cleanup();
                    throw new ArgumentException(string.Format(
                        CultureInfo.CurrentCulture,
                        @"* * * Column names/types in table do not match field names/types in output event type, OR column count ({0}) does not match field count ({1}) * * *",
                        fields.Count,
                        this.bindTimeEventType.Fields.Count));
                }
            }
            else
            {
                this.Cleanup();
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "Table ({0}) does not exist",
                    this.config.TableName));
            }

            // ensure that the destination table is empty
            // Important - This is being done to enable the sample to be run repeatedly
            // You can choose to comment this out if you do not mind the duplicate rows
            // from successive runs
            statement = "TRUNCATE TABLE [" + this.config.TableName + "]";
            this.command = new SqlCommand(statement, this.connection);
            this.command.ExecuteNonQuery();

            // prepare parameterized NonQuery (in this case, the INSERT) statement
            // map the provider type (in this case, SQL) rather than the CLR datatype to minimize conversion issues
            this.command = new SqlCommand(this.config.Statement, this.connection);
            this.command.Parameters.Add(new SqlParameter("@" + this.config.StartTimeColumnName, SqlDbType.DateTimeOffset));
            this.command.Parameters.Add(new SqlParameter("@" + this.config.EndTimeColumnName, SqlDbType.DateTimeOffset));

            for (int pos = 0; pos < fields.Count; pos++)
            {
                this.command.Parameters.Add(new SqlParameter("@" + fields[pos], fieldProviderTypes[pos]));
            }
        }

        /// <summary>
        /// Dequeues each event from StreamInsightReads, and writes it as a row into the SQL sink 
        /// </summary>
        private void ConsumeEvents()
        {
            TEvent currentEvent = default(TEvent);

            while (true)
            {
                try
                {
                    // if the engine asked the adapter to stop
                    if (this.outputAdapter.AdapterState == AdapterState.Stopping)
                    {
                        // clean up state
                        this.Cleanup();

                        // inform the engine that the adapter has stopped
                        this.outputAdapter.Stopped();

                        // and exit worker thread
                        return;
                    }

                    // NOTE: at any point in time during the execution of the code block below, the engine
                    // could change the Adapter state to Stopping. Then the engine will resume the adapter
                    // (i.e. call Resume()) just one more time, and the stopping condition will be
                    // trapped at the check above.

                    // Dequeue the event
                    DequeueOperationResult result = this.outputAdapter.Dequeue(out currentEvent);

                    // if the engine does not have any events, the adapter is Suspended; so do this ..
                    if (result == DequeueOperationResult.Empty)
                    {
                        // optionally invoke a method here for any housekeeping

                        // inform the engine that adapter is ready to be resumed
                        this.outputAdapter.Ready();

                        // exit the worker thread
                        return;
                    }

                    // write out event to output table
                    this.CreateRowFromEvent(currentEvent);
                }
                finally
                {
                    // IMPORTANT: Release the event always
                    if (currentEvent != null)
                    {
                        this.outputAdapter.ReleaseEvent(ref currentEvent);
                    }
                }
            }
        }

        /// <summary>
        /// Turns an event into a row
        /// </summary>
        /// <param name="currentEvent">current event</param>
        private void CreateRowFromEvent(TEvent currentEvent)
        {
            // Skip CTI events to console and return, rather than record them in the output table
            if (currentEvent.EventKind == EventKind.Cti)
            {
                return;
            }

            // populate the timestamps
            this.outputAdapter.SetEventTime(this.command, currentEvent);

            // Use an ordinal mapper to match fields in the CepEventType with the correct
            // column in the output table (see notes in SqlInputAdapter::CreateEventFromRow)
            foreach (var field in this.bindTimeEventType.Fields.Values)
            {
                this.command.Parameters["@" + field.Name].Value = currentEvent.GetField(field.Ordinal);
            }

            // INSERT the row into the table
            this.command.ExecuteNonQuery();
        }
    }
}