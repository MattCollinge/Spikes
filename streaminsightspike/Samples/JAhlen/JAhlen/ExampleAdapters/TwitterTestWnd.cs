// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;
using AdvantIQ.ExampleAdapters.Input.Twitter;
using AdvantIQ.ExampleAdapters.Output.WinFormStacker;

namespace AdvantIQ.ExampleAdapters
{
    public partial class TwitterTestWnd : Form
    {
        Query query;
        bool stopFlag;

        public TwitterTestWnd()
        {
            InitializeComponent();
        }

        public void TestTwitter()
        {
            var streamInsightInstanceName = Properties.Settings.Default.InstanceName;
            var server = Server.Create(streamInsightInstanceName);
            var application = server.CreateApplication("twittertest");

            var inputConfig = new TwitterConfig
                {
                    Username = UserNameTbx.Text,
                    Password = PasswordTbx.Text
                };

            if (FilterModeRbn.Checked)
            {
                inputConfig.Mode = TwitterMode.Filter;
                inputConfig.Parameters = "track=" + FilterParametersTbx.Text;
            }
            else if (SampleModeRbn.Checked)
                inputConfig.Mode = TwitterMode.Sample;

            var input = CepStream<Tweet>.Create("twitter", typeof(TwitterFactory), inputConfig, EventShape.Point);

            // Configure output adapter
            var outputConfig = new StackerConfig
                {
                    StackerCtlPipeName = stackerCtl1.PipeName,
                    StackerCtlHostName = "localhost"
                };

            // Create query and bind to the output adapter
            query = input.ToQuery(application, "query", "...", typeof(StackerFactory), outputConfig, EventShape.Point, StreamEventOrder.ChainOrdered);

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

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            TestTwitter();
        }

        private void MainWnd_Load(object sender, EventArgs e)
        {
        }

        private void RunBtn_Click(object sender, EventArgs e)
        {
            if (UserNameTbx.Text.Length == 0 || PasswordTbx.Text.Length == 0)
            {
                MessageBox.Show(this, "You must enter your Twitter username and password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(stackerCtl1.PipeName))
                stackerCtl1.PipeName = "TwitterTest";

            stackerCtl1.Clear();

            backgroundWorker1.RunWorkerAsync();
        }

        private void TwitterTestWnd_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopFlag = true;
        }

        private void StopBtn_Click(object sender, EventArgs e)
        {
            stopFlag = true;
        }

        private void FilterModeRbn_CheckedChanged(object sender, EventArgs e)
        {
            FilterParametersTbx.Enabled = FilterModeRbn.Enabled;
        }
    }
}
