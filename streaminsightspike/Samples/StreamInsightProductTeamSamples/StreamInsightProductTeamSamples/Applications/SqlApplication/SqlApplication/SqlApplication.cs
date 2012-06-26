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

namespace StreamInsight.Samples.SqlApplication
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;
    using StreamInsight.Samples.Adapters.SimpleTextFileWriter;
    using StreamInsight.Samples.Adapters.Sql;
    using System.Data.SqlClient;
    using System.Data;

    internal class SqlApplication
    {
        internal static void Main()
        {
            using (Server server = Server.Create("Default"))
            {
                Application application = server.CreateApplication("SqlApplication");
                SqlInputConfig inputConfig = new SqlInputConfig
                                             {
                                                 ConnectionString = @"integrated security = true; database = AdventureWorks",
                                                 Statement = // see here for schema: http://msdn.microsoft.com/en-us/library/ms124879.aspx
                                                         @"SELECT [SalesOrderID]
                                                                ,[OrderDate]
                                                                ,[ShipDate]
                                                                ,[TerritoryID]
                                                        FROM [AdventureWorks].[Sales].[SalesOrderHeader]
                                                        WHERE [OrderDate] IS NOT NULL AND [ShipDate] IS NOT NULL AND [TerritoryID] IS NOT NULL
                                                        ORDER BY [OrderDate]",
                                                 StartTimeColumnName = "OrderDate",
                                                 EndTimeColumnName = "ShipDate",
                                                 CultureName = "en-US"
                                             };

                AdvanceTimeSettings inputAdvaceTimeSettings = new AdvanceTimeSettings(
                                                                    new AdvanceTimeGenerationSettings((uint)1, TimeSpan.FromSeconds(0), true),
                                                                    default(AdvanceTimeImportSettings),
                                                                    default(AdvanceTimePolicy));

                // define a source stream of interval events from SQL input data,
                // the interval defined as the duration between the OrderDate and ShipDate
                var streamSource = CepStream<SqlInput>.Create(
                                        "SqlInputStream",
                                        typeof(SqlInputFactory),
                                        inputConfig,
                                        EventShape.Interval,
                                        inputAdvaceTimeSettings);

                // find time intervals during which more than 3 orders are processed within a territory
                // The result of this query is ""Between time T1 and T2, X many orders were processed in Y territory"
                var kpiQuery = from o in streamSource
                               group o by o.TerritoryID into g
                               from window in g.SnapshotWindow(SnapshotWindowOutputPolicy.Clip)
                               select new { OrderCount = window.Count(), TerritoryID = g.Key }
                                   into agg
                                   where agg.OrderCount > 3
                                   select agg;


                // define a sink stream of interval events reporting territories and order count.
                SqlOutputConfig outputConfig = new SqlOutputConfig
                                               {
                                                   ConnectionString = @"integrated security = true; database = AdventureWorks",
                                                   Statement = @"INSERT INTO [AdventureWorks].[Dbo].[PeakSalesByTerritory]
                                                                     ( OrderDate
                                                                     , ShipDate
                                                                     , OrderCount
                                                                     , TerritoryID)
                                                                     VALUES (@OrderDate, @ShipDate, @OrderCount, @TerritoryID)",
                                                   StartTimeColumnName = "OrderDate",
                                                   EndTimeColumnName = "ShipDate",
                                                   TableName = "PeakSalesByTerritory"
                                               };

                // Create the table if not present already
                CreateExampleTable(outputConfig.ConnectionString, outputConfig.TableName);

                // set this to false to output to the table
                bool outputToFile = false;

                // Write output to a file (console) or to the output table itself
                var query = outputToFile ?
                            kpiQuery.ToQuery(
                                      application,
                                      "KPIQuery",
                                      @"Time intervals with order count > 3 and territories",
                                      typeof(TextFileWriterFactory),
                                      new TextFileWriterConfig { OutputFileName = string.Empty, Delimiter = '\t' },
                                      EventShape.Interval,
                                      StreamEventOrder.FullyOrdered) :

                            kpiQuery.ToQuery(
                                        application,
                                        "KPIQuery",
                                        @"Time intervals with order count > 3 and territories",
                                        typeof(SqlOutputFactory),
                                        outputConfig,
                                        EventShape.Interval,
                                        // EventShape.Point,
                                        StreamEventOrder.FullyOrdered);

                Console.WriteLine("Start query");

                // Start the query
                query.Start();

                // Wait for the query to be suspended - that is the state
                // it will be in as soon as the output adapter stops due to
                // the end of the input stream. Then retrieve diagnostics.
                DiagnosticView dv = server.GetDiagnosticView(query.Name);
                while ((string)dv[DiagnosticViewProperty.QueryState] == "Running")
                {
                    // Sleep for 1s and check again
                    Thread.Sleep(1000);
                    dv = server.GetDiagnosticView(query.Name);
                }

                // Retrieve diagnostic information from the CEP server about the query.
                Console.WriteLine(string.Empty);
                RetrieveDiagnostics(server.GetDiagnosticView(new Uri("cep:/Server/Application/SqlApplication/Query/KPIQuery")), Console.Out);

                query.Stop();

                Console.WriteLine("\nPress Enter to quit the application");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Checks whether the output table already exists
        /// </summary>
        /// <param name="connection">Connection to access the database</param>
        /// <param name="tableName">Name of the output table to be created</param>
        private static bool CheckExampleTable(SqlConnection connection, string tableName)
        {

            // check if the target output table exists
            DataTable dataTable = connection.GetSchema("Tables");
            foreach (DataRow row in dataTable.Rows)
            {
                if (tableName.Equals(row[2]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether the output table already exists and creates it if not
        /// </summary>
        /// <param name="connectionString">Connection string to access the database</param>
        /// <param name="tableName">Name of the output table to be created</param>
        private static void CreateExampleTable(string connectionString, string tableName)
        {
            // Validate table name parameter
            char [] checks = { ';', '\'', '-', ']', '[' };
            if (-1 != tableName.IndexOfAny(checks)
                || tableName.Contains("/*") || tableName.Contains("*/") || tableName.Contains("xp_"))
            {
                throw new ArgumentException("Table name must not contain any of the following: ; , ' - [ ] /* */ xp_");
            }

            // Open connection to DB
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            if (!CheckExampleTable(connection, tableName))
            {
                // If table does not exists, create one with the name in the config and the schema desired.
                string statement = @"CREATE TABLE [" + tableName + "] " +
                            "(" +
                            "[OrderDate] DateTime, " +
                            "[ShipDate]  DateTime DEFAULT NULL, " +
                            "[OrderCount] integer, " +
                            "[TerritoryID] integer" +
                            ");";
                SqlCommand command = new SqlCommand(statement, connection);
                command.ExecuteNonQuery();
            }

            connection.Close();
        }

        /// <summary>
        /// Takes a diagnostic view and outputs all its entries (properties) to the given text writer.
        /// </summary>
        /// <param name="dv">Diagnostic view to output</param>
        /// <param name="traceListener">Tracer to receive the diagnostic data.</param>
        private static void RetrieveDiagnostics(DiagnosticView dv, System.IO.TextWriter traceListener)
        {
            traceListener.WriteLine("Diagnostic View for '" + dv.ObjectName + "':");
            foreach (KeyValuePair<string, object> dp in dv)
            {
                traceListener.WriteLine(" " + dp.Key + ": " + dp.Value);
            }
        }
    }
}