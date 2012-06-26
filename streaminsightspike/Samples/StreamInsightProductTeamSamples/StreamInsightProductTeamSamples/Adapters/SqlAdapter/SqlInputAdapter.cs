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

    /// <summary>
    /// Generic implementation of a synchronous SQL input adapter.
    /// This class contains code that is common for all SQL input adapters.
    /// </summary>
    /// <typeparam name="TInputAdapter">The type of the concrete adapter to use (Point, Interval).</typeparam>
    /// <typeparam name="TEvent">The concrete event shape (Point, Interval).</typeparam>
    public sealed class SqlInputAdapter<TInputAdapter, TEvent> : IDisposable
        where TInputAdapter : ISqlInputAdapter<TEvent>
        where TEvent : UntypedEvent
    {
        /// <summary>
        /// Specific adapter object. Since it has to implement the IInputAdapter
        /// interface, this object provides all necessary adapter interactions.
        /// </summary>
        private TInputAdapter inputAdapter;

        /// <summary>
        /// The configuration object passed from the factory.
        /// </summary>
        private SqlInputConfig config;

        /// <summary>
        /// Data Connection
        /// </summary>
        private SqlConnection connection;

        /// <summary>
        /// SQL Command object for the input SQL query.
        /// </summary>
        private SqlCommand command;

        /// <summary>
        /// Data reader for the input SQL query.
        /// </summary>
        private SqlDataReader dataReader;

        /// <summary>
        /// Event type to be produced by the adapter at runtime.
        /// </summary>
        private CepEventType bindTimeEventType;

        /// <summary>
        /// Current Event
        /// </summary>
        private TEvent currentEvent = default(TEvent);

        public SqlInputAdapter(SqlInputConfig configInfo, CepEventType eventType, TInputAdapter inputAdapter)
        {
            this.inputAdapter = inputAdapter;
            this.config = configInfo;
            this.bindTimeEventType = eventType;

            ValidateConfigParameter(this.config.StartTimeColumnName);
            ValidateConfigParameter(this.config.EndTimeColumnName);
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
            this.InitInputData();
            this.ProduceEvents();
        }

        internal void Resume()
        {
            this.ProduceEvents();
        }

        /// <summary>
        /// Release event memory, release DB resources. Since Cleanup()
        /// will be called from multiple places in exception and non-exception
        /// conditions, nullify the released resource.
        /// </summary>
        internal void Cleanup()
        {
            if (this.currentEvent != null)
            {
                this.inputAdapter.ReleaseEvent(ref this.currentEvent);
            }

            if (this.dataReader != null)
            {
                this.dataReader.Close();
                this.dataReader = null;
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
        /// Initialize DB connections and validates the input schema against event type
        /// </summary>
        private void InitInputData()
        {
            // Open connection to DB
            this.connection = new SqlConnection(this.config.ConnectionString);
            this.connection.Open();

            // Set up the SQL query for reads
            this.command = new SqlCommand(this.config.Statement, this.connection);
            this.dataReader = this.command.ExecuteReader();

            // Catch any schema to eventtype mismatches right here, rather than at runtime

            // Retrieve the input schema from the query (sans the timestamp column(s))
            DataTable dataTable = this.dataReader.GetSchemaTable();
            IList<string> fields = new List<string>();
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    if (col.ColumnName.Equals("ColumnName"))
                    {
                        string columnName = row[col].ToString();
                        if (!(columnName.Equals(this.config.StartTimeColumnName) || columnName.Equals(this.config.EndTimeColumnName)))
                        {
                            fields.Add(columnName);
                        }
                    }
                }
            }

            // check if the event type has the same number of fields as the columns in the SQL query (sans timestamp column(s))
            if (fields.Count != this.bindTimeEventType.Fields.Count)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    @"* * * Column names/types in query do not match field names/types in input event type, OR column count ({0}) does not match field count ({1}) * * *",
                    fields.Count,
                    this.bindTimeEventType.Fields.Count));
            }
        }

        /// <summary>
        /// Reads rows from the SQL source, and enqueues each row as an event into StreamInsight
        /// </summary>
        private void ProduceEvents()
        {
            while (true)
            {
                // if the engine asked the adapter to stop
                if (this.inputAdapter.AdapterState == AdapterState.Stopping)
                {
                    // cleanup state
                    this.Cleanup();

                    // inform the engine that the adapter has stopped
                    this.inputAdapter.Stopped();

                    // exit the worker thread
                    return;
                }

                // prepare the event - either saved from previous iteration, or a new one from the input
                if (this.currentEvent == null)
                {
                    // read a row
                    // IMPORTANT: If a runtime exception during Read inhibits a call to Cleanup(),
                    // resources will be released by Dispose() during adapter shutdown.
                    if (!this.dataReader.Read())
                    {
                        this.Cleanup();

                        // inform the engine that the adapter has stopped
                        this.inputAdapter.Stopped();
                        
                        return;
                    }

                    // create an event from the row
                    this.currentEvent = this.CreateEventFromRow(this.dataReader);
                    if (this.currentEvent == null)
                    {
                        this.Cleanup();

                        // inform the engine that the adapter has stopped
                        this.inputAdapter.Stopped();

                        return;
                    }
                }

                // Enqueue the created event into the engine
                EnqueueOperationResult result = this.inputAdapter.Enqueue(ref this.currentEvent);

                // If the engine pushes back, the adapter is Suspended; so do this ..
                if (result == EnqueueOperationResult.Full)
                {
                    // retain the currentEvent to be handled in the next iteration

                    // inform the engine that the adapter is Ready to be Resumed
                    this.inputAdapter.Ready();

                    // exit the worker thread
                    return;
                }
            }
        }

        /// <summary>
        /// Turns a row into an event.
        /// </summary>
        /// <param name="reader">data reader</param>
        /// <returns>new event instance</returns>
        private TEvent CreateEventFromRow(SqlDataReader reader)
        {
            // create a new event
            TEvent newEvent = this.inputAdapter.CreateInsertEvent();
            if (newEvent != null)
            {
                try
                {
                    CultureInfo culture = new CultureInfo(this.config.CultureName);

                    // populate the timestamps
                    this.inputAdapter.SetEventTime(newEvent, this.dataReader);

                    // populate the payload fields
                    // Assume you input event type has fields {b, a, c}, which maps to the
                    // way the input row provides these values. StreamInsight understands
                    // CepEventType in alphabetical order of fields ({a, b, c}) - which may
                    // cause a mismatch in types or assignment of values to files. Unlike
                    // the CSV/Text file adapter, here the SqlDataReader fetches the column
                    // by name, and we use SetField() to set the value to a field by ordinal.
                    object value = null;
                    foreach (var field in this.bindTimeEventType.Fields.Values)
                    {
                        // set the value for the field, handling NULLs (byte array and GUID not covered)
                        value = reader[field.Name];
                        if (null != value)
                        {
                            value = Convert.ChangeType(value, field.Type.ClrType, culture);
                        }
                        else
                        {
                            value = this.GetDefaultValue(field.Type.ClrType, culture);
                        }

                        newEvent.SetField(field.Ordinal, value);
                    }
                }
                catch (Exception)
                {
                    // newEvent is locally allocated and MUST be cleaned up here
                    this.inputAdapter.ReleaseEvent(ref newEvent);

                    // re-throw the exception; the adapter should NEVER mask/swallow exceptions
                    throw;
                }
            }

            return newEvent;
        }

        /// <summary>
        /// Get the default value of a field based on its CLR type
        /// </summary>
        /// <param name="clrType">CLR type of the field</param>
        /// <param name="culture">Culture specification</param>
        /// <returns>default value for the type</returns>
        private object GetDefaultValue(Type clrType, CultureInfo culture)
        {
            if (clrType == typeof(string))
            {
                return string.Empty;
            }
            else if (clrType == typeof(byte[]))
            {
                return new byte[] { };
            }
            else if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return null;
            }
            else
            {
                return Convert.ChangeType(0, clrType, culture);
            }
        }
    }
}