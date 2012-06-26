// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2011. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AdvantIQ.ExampleAdapters.Input.Facebook;
using AdvantIQ.ExampleAdapters.Output.WinFormStacker;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;

namespace AdvantIQ.ExampleAdapters
{
    public partial class FaceBookTestWnd : Form
    {
        private const string DeveloperSetupURL = "http://developers.facebook.com/setup/";
        private const string DeveloperPortalURL = "http://www.facebook.com/developers";

        Query query;
        bool stopFlag;

        public FaceBookTestWnd()
        {
            InitializeComponent();
        }

        private void DeveloperSetupLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(DeveloperSetupURL);
        }

        private void DeveloperExistingLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(DeveloperPortalURL);
        }

        private void GetAccessTokenBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ClientIdTbx.Text))
                return;

            var dlg = new FaceBookGetAccessTokenWnd(ClientIdTbx.Text);
            if (dlg.ShowDialog() == DialogResult.OK)
                AccessTokenTbx.Text = dlg.AccessToken;
        }

        public void TestFacebook()
        {
            var streamInsightInstanceName = Properties.Settings.Default.InstanceName;
            var server = Server.Create(streamInsightInstanceName);
            var application = server.CreateApplication("facebooktest");

            var inputConfig = new FacebookConfig
            {
                AccessToken = AccessTokenTbx.Text,
                UsernameOrUniqueId = UsernameOrUniqueIdTbx.Text,
                RefreshPeriod = int.Parse(RefreshIntervalTbx.Text) * 1000
            };


            var input = CepStream<FacebookItem>.Create("facebook", typeof(FacebookFactory), inputConfig, EventShape.Point);

            // Configure output adapter
            var outputConfig = new StackerConfig
            {
                StackerCtlPipeName = stackerCtl1.PipeName,
                StackerCtlHostName = "localhost"
            };

            // Create query and bind to the output adapter
            var expr = from e in input
                       select new {e.Id, e.FromName, Message = !string.IsNullOrEmpty(e.Message) ? e.Message : e.Description };

            query = expr.ToQuery(application, "query", "...", typeof(StackerFactory), outputConfig, EventShape.Point, StreamEventOrder.FullyOrdered);

            // Start query
            query.Start();

            // Wait until query change state
            DiagnosticView diagnosticView;
            stopFlag = false;
            do
            {
                Thread.Sleep(100);
                diagnosticView = query.Application.Server.GetDiagnosticView(query.Name);
            } while (!stopFlag && (string)diagnosticView["QueryState"] == "Running");

            // Stop query
            query.Stop();

            application.Delete();
            server.Dispose();
        }

        private void RunBtn_Click(object sender, EventArgs e)
        {
            if (AccessTokenTbx.Text.Length == 0 || UsernameOrUniqueIdTbx.Text.Length == 0)
            {
                MessageBox.Show(this, "You must enter an access token and a username/unique id.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!FacebookInput.TestParameters(AccessTokenTbx.Text, UsernameOrUniqueIdTbx.Text))
            {
                MessageBox.Show(this, "Unable to retrieve Facebook data. Check your access token and username/unique id.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(stackerCtl1.PipeName))
                stackerCtl1.PipeName = "FacebookTest";

            stackerCtl1.Clear();

            backgroundWorker1.RunWorkerAsync();
        }

        private void StopBtn_Click(object sender, EventArgs e)
        {
            stopFlag = true;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            TestFacebook();
        }

        private void FaceBookTestWnd_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopFlag = true;
        }
    }
}
