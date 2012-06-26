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
using System.Threading;
using System.Windows.Forms;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;
using AdvantIQ.ExampleAdapters.Input.YahooFinance;
using AdvantIQ.ExampleAdapters.Output.WinFormStacker;

namespace AdvantIQ.ExampleAdapters
{
    public partial class YahooTestWnd : Form
    {
        Query query;
        bool stopFlag;

        public YahooTestWnd()
        {
            InitializeComponent();
        }

        private void YahooTestWnd_Load(object sender, EventArgs e)
        {
            stackerCtl1.PipeName = "YahooTestPipe";
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var streamInsightInstanceName = Properties.Settings.Default.InstanceName;
            var server = Server.Create(streamInsightInstanceName);
            var application = server.CreateApplication("yahoofinancetest");

            // Configure output adapter
            var outputConfig = new StackerConfig
            {
                StackerCtlPipeName = stackerCtl1.PipeName,
                StackerCtlHostName = "localhost"
            };

            var inputConfig = new YahooFinanceConfig
            {
                StockSymbol = "^GSPC",
                StockName = "S&P 500",
                Timeout = 10000,
                Interval = 1000
            };

            var input = CepStream<StockQuote>.Create("yahoo", typeof(YahooFinanceFactory), inputConfig, EventShape.Point);

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

            Thread.Sleep(1000);

            application.Delete();
            server.Dispose();
        }

        private void YahooTestWnd_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopFlag = true;
        }
    }
}
