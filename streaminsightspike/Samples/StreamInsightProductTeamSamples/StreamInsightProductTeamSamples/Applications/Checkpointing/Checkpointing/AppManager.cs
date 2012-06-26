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

namespace StreamInsight.Samples.Checkpointing
{
    using System;
    using System.Threading;
    using Microsoft.ComplexEventProcessing;

    /// <summary>
    /// A method used by the AppManager to create a query.
    /// </summary>
    /// <param name="app">The application to contain the query. This will be provided by the AppManager.</param>
    /// <param name="queryName">The name of the query. This will be provided by the AppManager.</param>
    /// <param name="queryDescription">The description of the query. This will be provided by the AppManager.</param>
    /// <returns>A query of the given name and description created in the given application.</returns>
    public delegate Query QueryCreator(Application app, string queryName, string queryDescription);

    /// <summary>
    /// A class that manages the content of a StreamInsight application. Can be used to start, restart,
    /// and checkpoint queries.
    /// </summary>
    public class AppManager : IDisposable
    {
        /// <summary>
        /// Are we currently checkpointing?
        /// </summary>
        private bool checkpointing = false;

        /// <summary>
        /// Are we in the process of disposing?
        /// </summary>
        private bool disposing = false;

        /// <summary>
        /// The thread used by the AppManager for checkpointing and other query management.
        /// </summary>
        private Thread housekeepingThread;

        /// <summary>
        /// Initializes a new instance of the AppManager class for the given server and application.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="applicationName">The name of an application, either existing or new, to manage.</param>
        public AppManager(Server server, string applicationName)
        {
            Util.Log("AppManager init", "Created for " + applicationName + ".");

            this.Server = server; 

            // Fetch the app if it exists; create it if not.
            if (Server.Applications.ContainsKey(applicationName))
            {
                Util.Log("AppManager init", "Creating application " + applicationName + ".");
                this.App = Server.Applications[applicationName];
                this.StartAll();
            }
            else
            {
                Util.Log("AppManager init", "Connecting to application.");
                this.App = Server.CreateApplication(applicationName);
            }

            // Start up housekeeping.
            this.housekeepingThread = new Thread(this.HousekeepingLoop);
            this.housekeepingThread.Name = "Housekeeping";
            this.housekeepingThread.Start();
        }

        /// <summary>
        /// Gets the AppManager's server object.
        /// </summary>
        public Server Server { get; private set; }

        /// <summary>
        /// Gets the Application object this AppManager is managing.
        /// </summary>
        public Application App { get; private set; }

        /// <summary>
        /// Check whether the app has a query of the given name.
        /// </summary>
        /// <param name="shortName">The short name of the query to check for.</param>
        /// <returns>True if the query is present, false otherwise.</returns>
        public bool ContainsQuery(string shortName)
        {
            return this.App.Queries.ContainsKey(shortName);
        }

        /// <summary>
        /// Add a query to the app and run it.
        /// </summary>
        /// <param name="shortName">The short name the query should take. This method will throw an ArgumentException if this already exists.</param>
        /// <param name="description">A description of the query.</param>
        /// <param name="creator">A QueryCreator that the AppManager can use to build the query.</param>
        public void RunQuery(string shortName, string description, QueryCreator creator)
        { 
            Util.Log("AppManager (" + this.App.ShortName + ")", "Adding query: " + shortName);
            if (this.App.Queries.Keys.Contains(shortName))
            {
                throw new ArgumentException("Query already exists.");
            }

            Query q = creator(this.App, shortName, description);
            q.Start();
        }

        /// <summary>
        /// Stop and remove the given query along with its template.
        /// </summary>
        /// <param name="shortName">The short name of the query to remove.</param>
        public void RemoveQuery(string shortName)
        {
            Util.Log("AppManager (" + this.App.ShortName + ")", "Removing query: " + shortName);
            Query q = this.App.Queries[shortName];
            q.Stop();
            var template = q.Template();
            q.Delete();
            template.Delete();
        }

        /// <summary>
        /// Ask the AppManager to begin checkpointing of all queries.
        /// </summary>
        public void BeginCheckpointing()
        {
            Util.Log("AppManager (" + this.App.ShortName + ")", "Starting checkpointing");
            this.checkpointing = true;
        }

        /// <summary>
        /// Ask the AppManager to stop taking checkpoints.
        /// </summary>
        public void StopCheckpointing()
        {
            Util.Log("AppManager (" + this.App.ShortName + ")", "Stopping checkpointing");
            this.checkpointing = false;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// We're through. Signal that our housekeeping thread should terminate and wait until it finishes.
        /// </summary>
        /// <param name="disposing">Dispose manage state?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.disposing = true;
                while (this.housekeepingThread.IsAlive)
                {
                    Thread.Sleep(Util.ThreadSpinDelay);
                }
            }
        }

        /// <summary>
        /// Starts all queries in the manager. Used during initialization to (re)start all managed queries, whether
        /// resilient or not.
        /// </summary>
        private void StartAll()
        {
            Util.Log("AppManager (" + this.App.ShortName + ")", "Starting queries...");
            foreach (Query q in this.App.Queries.Values)
            {
                Util.Log("AppManager (" + this.App.ShortName + ")", "Starting: " + q.Name);
                q.Start();
            }
        }

        /// <summary>
        /// The main housekeeping loop. Essentially, this loops over all of the queries and:
        ///  - Removes the query if it is completed or aborted.
        ///  - Takes a checkpoint if it is running and resilient, and if checkpointing is on for the app.
        /// This will quit once the AppManger is disposed.
        /// </summary>
        private void HousekeepingLoop()
        {
            while (!this.disposing)
            {
                // Don't churn too much if there are no queries or if all we're doing is looking at state.
                if (this.App.Queries.Count == 0)
                {
                    Thread.Sleep(Util.ThreadSpinDelay);
                }
                else
                {
                    try
                    {
                        foreach (var q in this.App.Queries.Values)
                        {
                            // Quit out if we're disposing.
                            if (this.disposing)
                            {
                                return;
                            }

                            // Remove queries that have been completed or aborted.
                            if (q.State() == "Completed" || q.State() == "Aborted")
                            {
                                Util.Log("AppManager (" + this.App.ShortName + ")", q.Name + " is in state '" + q.State());
                                this.RemoveQuery(q.ShortName);
                            }

                            // Take a checkpoint. This may fail, e.g., if the query is being taken down.
                            if (this.checkpointing && q.IsResilient && q.State() == "Running")
                            {
                                try
                                {
                                    // This will begin a checkpoint on the query and wait for it to complete. It effectively
                                    // turns the asynchronous call into a synchronous one.
                                    Server.EndCheckpoint(Server.BeginCheckpoint(q, null, null));
                                }
                                catch (Exception)
                                {
                                    // Swallow the error, but log the fact we failed.
                                    Util.Log("AppManager (" + this.App.ShortName + ")", "Checkpoint failed for " + q.Name);
                                }
                            }
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // The query list was modified. Just restart our traversal. This isn't necessarily fair---restarting
                        // the traversal may favor some queries---but in the usual case, we assume that there isn't much
                        // churn in the list, and so this should rarely be hit.
                    }
                }
            }
        }
    }
}
