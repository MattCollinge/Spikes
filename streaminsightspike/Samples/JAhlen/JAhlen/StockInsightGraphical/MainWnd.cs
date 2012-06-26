// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;
using AdvantIQ.StockInsight.InputAdapters;
using AdvantIQ.StockInsight.OutputAdapters;

namespace AdvantIQ.StockInsight
{
    public partial class MainWnd : Form
    {
        /// <summary>
        /// SET THIS TO THE NAME OF YOUR STREAMINSIGHT INSTANCE, OR BLANK TO AUTODETECT
        /// </summary>
        private const string InstanceName = "Default";

        public List<Source> Sources;
        public Dictionary<string, QueryTemplate> Queries;

        private Microsoft.ComplexEventProcessing.Application application;

        public MainWnd()
        {
            InitializeComponent();

            var instanceName = !string.IsNullOrEmpty(InstanceName) ? InstanceName : StreamInsightSetupInfo.EnumerateInstances()[0];
            Server server;

            try
            {
                server = Server.Create(instanceName);
            }
            catch
            {
                Console.WriteLine("Could not create StreamInsight instance. Please open MainWnd.cs and check InstanceName.");
                return;
            }
            application = server.CreateApplication("StockInsightGraphical");

            InputAdapter inputAdapter = application.CreateInputAdapter<StockQuoteInputFactory>("StockQuoteInput", "Description...");
            OutputAdapter outputAdapter = application.CreateOutputAdapter<StockSignalOutputFactory>("StockSignalOutput", "Description...");

            // Initialize drop down lists and properties for queries and sources
            InitQueries();
            InitSources();

            stockGraph1.EnableWCF();
        }

        /// <summary>
        /// Inits the user selection of source
        /// </summary>
        private void InitSources()
        {
            Sources = new List<Source>();

            // Determine path for historical data
            var dataPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + "\\HistoricalData\\";

            Sources.Add(new Source
            {
                Name = "NASDAQ Composite",
                Config = new StockQuoteInputConfig
                {
                    Filename = dataPath + "nasdaq_2009.csv",
                    ID = "NASDAQ-USD",
                    ColumnNames = new string[] { "Adj Close" }
                },
                MinValue = 1200,
                MaxValue = 3000,
                GridValueSize = 200
            });

            Sources.Add(new Source
            {
                Name = "Nokia",
                Config = new StockQuoteInputConfig
                {
                    Filename = dataPath + "nokia_2009.csv",
                    ID = "NOKIA-USD",
                    ColumnNames = new string[] { "Adj Close" }
                },
                MinValue = 6,
                MaxValue = 20,
                GridValueSize = 2
            });

            Sources.Add(new Source
            {
                Name = "Ericsson",
                Config = new StockQuoteInputConfig
                {
                    Filename = dataPath + "eric_b_usd_2009.csv",
                    ID = "ERIC-USD",
                    ColumnNames = new string[] { "Adj Close" }
                },
                MinValue = 6,
                MaxValue = 14,
                GridValueSize = 1
            });

            SourceDpl.Items.Clear();
            foreach (var source in Sources)
                SourceDpl.Items.Add(source.Name);
            //SourceDpl.Items.Add("Browse...");
            SourceDpl.SelectedIndex = 0;
        }

        /// <summary>
        /// Inits the user selection of query
        /// </summary>
        private void InitQueries()
        {
            Queries = new Dictionary<string, QueryTemplate>();
            Queries.Add("Bollinger Bands", AddBollingerBandsQuery());
            Queries.Add("Follow the Trend", AddFollowTheTrendQuery());
            Queries.Add("Sell on Fridays", AddSellOnFridaysQuery());

            QueryDpl.Items.Clear();
            foreach (var query in Queries.Keys)
                QueryDpl.Items.Add(query);
            QueryDpl.SelectedIndex = 0;
        }

        /// <summary>
        /// Example query
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private QueryTemplate AddFollowTheTrendQuery()
        {
            CepStream<StockQuote> input = CepStream<StockQuote>.Create("input");

            var avg = (from w in input.AlterEventDuration(e => TimeSpan.FromDays(1)).HoppingWindow(TimeSpan.FromDays(5), TimeSpan.FromDays(1), HoppingWindowOutputPolicy.ClipToWindowEnd)
                               select new
                               {
                                   Value = w.Avg(e => e.Value),
                               }).AlterEventLifetime(e => e.StartTime.AddDays(4), e => TimeSpan.FromTicks(1));;

            var query = from e1 in input
                   from e2 in avg
                   select new StockSignal
                   {
                       StockID = e1.StockID,
                       Value = e1.Value,
                       BuySignal = e1.Value > e2.Value,
                       TimeStamp = e1.TimeStamp
                   };

            return application.CreateQueryTemplate<StockSignal>("FollowTheTrend", "...", query);
        }

        /// <summary>
        /// Example query
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private QueryTemplate AddSellOnFridaysQuery()
        {
            CepStream<StockQuote> input = CepStream<StockQuote>.Create("input");

            var query = from e in input
                   select new StockSignal
                   {
                       StockID = e.StockID,
                       Value = e.Value,
                       BuySignal = ((e.TimeStamp - new DateTime(2010,05,14,0,0,0,0,DateTimeKind.Utc)).Days % 7 != 0), // The 14th of May 2010 is a Friday
                       TimeStamp = e.TimeStamp
                   };

            return application.CreateQueryTemplate<StockSignal>("SellOnFridays", "...", query);
        }

        /// <summary>
        /// Example query
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private QueryTemplate AddBollingerBandsQuery()
        {
            CepStream<StockQuote> input = CepStream<StockQuote>.Create("input");

            var bb = (from w in input.AlterEventDuration(e => TimeSpan.FromDays(1)).HoppingWindow(TimeSpan.FromDays(20), TimeSpan.FromDays(1), HoppingWindowOutputPolicy.ClipToWindowEnd)
                       select w.BollingerBands()).AlterEventLifetime(e => e.StartTime.AddDays(19), e => TimeSpan.FromTicks(1));

            var query = from e1 in input
                        from e2 in bb
                        select new StockSignal
                        {
                            StockID = e1.StockID,
                            Value = e1.Value,
                            BuySignal = e2.BandWidth * 100 > 5,
                            TimeStamp = e1.TimeStamp
                        };

            return application.CreateQueryTemplate<StockSignal>("BollingerBands", "...", query);
        }

        /// <summary>
        /// Event handler for Run button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void RunBtn_Click(object sender, EventArgs eventArgs)
        {
            // Get selected source and query
            Source source;
            if (SourceDpl.SelectedItem.ToString() == "Browse...")
            {
                source = BrowseAndReadSource();
            }
            else
            {
                source = Sources.Single(s => s.Name == SourceDpl.SelectedItem.ToString());
            }
            var queryTemplate = Queries[QueryDpl.SelectedItem.ToString()];

            // Initialize display properties
            stockGraph1.StartDate = new DateTime(2009, 01, 01);
            stockGraph1.EndDate = new DateTime(2009, 12, 31);
            stockGraph1.GridDateSize = 30;
            stockGraph1.ValueLabelFormat = "f0";
            stockGraph1.DateLabelFormat = "dd MMM";

            stockGraph1.MaxValue = source.MaxValue;
            stockGraph1.MinValue = source.MinValue;
            stockGraph1.GridValueSize = source.GridValueSize;

            // Clear previous stock signals
            stockGraph1.Clear();

            // Instantiate query
            var queryBinder = new QueryBinder(queryTemplate);
            queryBinder.BindProducer<StockQuote>("input", application.InputAdapters["StockQuoteInput"], source.Config, EventShape.Point);
            queryBinder.AddConsumer<StockQuote>("output", application.OutputAdapters["StockSignalOutput"], new StockSignalOutputConfig(), EventShape.Point, StreamEventOrder.ChainOrdered);

            var query = application.CreateQuery(queryTemplate.ShortName + " " + Guid.NewGuid().ToString(), "Description...", queryBinder);

            backgroundWorker1.RunWorkerAsync(query);
        }

        private Source BrowseAndReadSource()
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK)
                return null;

            return new Source
            {
                Name = Path.GetFileNameWithoutExtension(dlg.FileName),
                Config = new StockQuoteInputConfig { Filename = dlg.FileName,
                                ID = Path.GetFileNameWithoutExtension(dlg.FileName),
                                ColumnNames = new string[] { "Adj Close" }
                },
                MinValue = 0,
                MaxValue = 3000,
                GridValueSize = 300
            };
        }

        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWnd_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWnd_SizeChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var query = e.Argument as Query;

            query.Start();

            DiagnosticView dv;
            do
            {
                System.Threading.Thread.Sleep(100);
                dv = application.Server.GetDiagnosticView(query.Name);
            } while (dv[DiagnosticViewProperty.QueryState] as string == "Running");

            query.Stop();
            query.Delete();

            ChannelFactory<IStockGraphCtl> stockGraphCtlFactory =
                new ChannelFactory<IStockGraphCtl>(new NetNamedPipeBinding(),
                    new EndpointAddress("net.pipe://localhost/StockGraphCtl"));
            var stockGraphCtl = stockGraphCtlFactory.CreateChannel();
            stockGraphCtl.Display();
        }
    }
}
