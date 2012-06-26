// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace AdvantIQ.StockInsight
{
    [Serializable]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public partial class StockGraphCtl : UserControl, IStockGraphCtl
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double GridValueSize { get; set; }
        public double GridDateSize { get; set; }
        public string ValueLabelFormat { get; set; }
        public string DateLabelFormat { get; set; }
        public List<StockSignal> StockSignals { get; set; }

        public Pen BaseLinePen = (Pen)Pens.Black.Clone();
        public Pen ModelLinePen = (Pen)Pens.Blue.Clone();
        public Pen SolidGridPen = Pens.Black;
        public Pen DashedGridPen = new Pen(Brushes.Gray) { DashStyle = DashStyle.Custom, DashPattern = new float[] { 3, 3 } };
        public Font LabelFont = SystemFonts.DefaultFont;
        public Font ResultFont = SystemFonts.DefaultFont;
        public Brush LabelBrush = Brushes.Black;
        public Brush ResultBrush = Brushes.DarkViolet;
        public Brush BuyBrush = Brushes.LightGreen;
        public Brush SellBrush = Brushes.LightSalmon;
        public const int GridMargin = 30;

        private ServiceHost serviceHost;

        /// <summary>
        /// Constructor
        /// </summary>
        public StockGraphCtl()
        {
            InitializeComponent();

            BaseLinePen.Width = 2;
            ModelLinePen.Width = 2;

            StockSignals = new List<StockSignal>();
        }

        /// <summary>
        /// OnLoad event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StockGraph_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Draws the control on screen
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Exit if not initialized
            if (MinValue == MaxValue || StartDate == EndDate)
                return;

            try
            {
                var g = e.Graphics;
                DrawGrid(g);
                DrawSeries(g);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Draw the signals
        /// </summary>
        /// <param name="g"></param>
        private void DrawSeries(Graphics g)
        {
            // Clip to inside of grid
            g.SetClip(new Rectangle(GridMargin, GridMargin, this.Width - 2 * GridMargin, this.Height - 2 * GridMargin));

            // Lock StockSignals to avoid problems with multi-threading
            lock (StockSignals)
            {
                StockSignal prevStockSignal = null;
                double prevModelValue = 0;

                foreach (var stockSignal in StockSignals)
                {
                    // Exit if outside of date range
                    if (stockSignal.TimeStamp > EndDate)
                        break;

                    if (prevStockSignal != null)
                    {
                        // Calculate (x, y) coordinates for current signal and previous signal
                        var x = (stockSignal.TimeStamp - StartDate).TotalDays / (EndDate - StartDate).TotalDays * (this.Width - GridMargin * 2) + GridMargin;
                        var y = this.Height - (stockSignal.Value - MinValue) / (MaxValue - MinValue) * (this.Height - GridMargin * 2) - GridMargin;

                        var prevX = (prevStockSignal.TimeStamp - StartDate).TotalDays / (EndDate - StartDate).TotalDays * (this.Width - GridMargin * 2) + GridMargin;
                        var prevY = this.Height - (prevStockSignal.Value - MinValue) / (MaxValue - MinValue) * (this.Height - GridMargin * 2) - GridMargin;

                        // Set brush color depending on buy/sell signal
                        var brush = prevStockSignal.BuySignal ? BuyBrush : SellBrush;

                        // Define polygon
                        var points = new Point[] { 
                            new Point((int)Math.Round(prevX), this.Height - GridMargin),
                            new Point((int)Math.Round(prevX), (int)Math.Round(prevY)),
                            new Point((int)Math.Round(x), (int)Math.Round(y)),
                            new Point((int)Math.Round(x), this.Height - GridMargin)
                            };

                        //g.SmoothingMode = SmoothingMode.HighSpeed;
                        g.FillPolygon(brush, points);
                        //g.SmoothingMode = SmoothingMode.HighQuality;
                        g.DrawLine(BaseLinePen, points[1], points[2]);

                        // Calculate value for model (only adjust if a previous buy signal)
                        var modelValue = prevStockSignal.BuySignal ?
                            prevModelValue * (stockSignal.Value / prevStockSignal.Value) :
                            prevModelValue;

                        // Calculate coordinates for the model signal
                        y = this.Height - (modelValue - MinValue) / (MaxValue - MinValue) * (this.Height - GridMargin * 2) - GridMargin;
                        prevY = this.Height - (prevModelValue - MinValue) / (MaxValue - MinValue) * (this.Height - GridMargin * 2) - GridMargin;

                        g.DrawLine(ModelLinePen,
                            new Point((int)Math.Round(prevX), (int)Math.Round(prevY)),
                            new Point((int)Math.Round(x), (int)Math.Round(y))
                            );

                        // Copy to prev value holder
                        prevModelValue = modelValue;
                    }
                    else
                    {
                        // Initialize model at same level as base
                        prevModelValue = stockSignal.Value;
                    }

                    prevStockSignal = stockSignal;
                }

                // Allow drawing of text outside grid
                g.ResetClip();

                // Calculate percentage changes
                var basePercent = 100 * (StockSignals.Last().Value - StockSignals.First().Value) / StockSignals.First().Value;
                var modelPercent = 100 * (prevModelValue - StockSignals.First().Value) / StockSignals.First().Value;

                // Convert to strings
                var basePercentStr = String.Format("Base ROI: {0:f2} %", basePercent);
                var modelPercentStr = String.Format("Model ROI: {0:f2} %", modelPercent);
                var diffPercentStr = String.Format("Difference: {0:f2} %", modelPercent - basePercent);

                // Display at top of window
                g.DrawString(basePercentStr, ResultFont, ResultBrush, 0, 0);
                g.DrawString(modelPercentStr, ResultFont, ResultBrush, this.Width/2 - g.MeasureString(modelPercentStr, ResultFont).Width/2, 0);
                g.DrawString(diffPercentStr, ResultFont, ResultBrush, this.Width - g.MeasureString(diffPercentStr, ResultFont).Width, 0);
            }

        }

        /// <summary>
        /// Draw the grid background
        /// </summary>
        /// <param name="g"></param>
        private void DrawGrid(Graphics g)
        {
            // Calculate x and y coordinates for grid
            var gridXcoordinates = new Dictionary<int, DateTime>();
            for (DateTime d = StartDate; d < EndDate; d = d.AddDays(GridDateSize))
            {
                var x = (d - StartDate).TotalDays / (EndDate - StartDate).TotalDays * (this.Width - GridMargin * 2) + GridMargin;
                gridXcoordinates.Add((int)(Math.Round(x)), d);
            }
            gridXcoordinates.Add(this.Width - GridMargin, EndDate);

            var gridYcoordinates = new Dictionary<int, double>();
            for (double v = MinValue; v < MaxValue; v += GridValueSize)
            {
                var y = this.Height - (v - MinValue) / (MaxValue - MinValue) * (this.Height - GridMargin * 2) - GridMargin;
                gridYcoordinates.Add((int)(Math.Round(y)), v);
            }
            gridYcoordinates.Add(GridMargin, MaxValue);

            // Draw inner gridlines
            foreach (var y in gridYcoordinates.Keys)
                g.DrawLine(DashedGridPen, GridMargin, y, this.Width - GridMargin, y);
            foreach (var x in gridXcoordinates.Keys)
                g.DrawLine(DashedGridPen, x, GridMargin, x, this.Height - GridMargin);

            // Draw outer gridlines (border)
            g.DrawRectangle(SolidGridPen, GridMargin, GridMargin, this.Width - GridMargin * 2, this.Height - GridMargin * 2);

            // Draw y labels
            var labelHeight = g.MeasureString("0", LabelFont).Height;
            foreach (var y in gridYcoordinates)
            {
                var labelWidth = g.MeasureString(y.Value.ToString(ValueLabelFormat), LabelFont).Width;
                g.DrawString(y.Value.ToString(ValueLabelFormat), LabelFont, LabelBrush, GridMargin / 2 - labelWidth / 2, (float)y.Key - labelHeight / 2);
            }

            // Draw x labels (only a every second grid line)
            var flag = false;
            foreach (var x in gridXcoordinates)
            {
                flag = !flag;
                if (!flag)
                    continue;
                var labelWidth = g.MeasureString(x.Value.ToString(DateLabelFormat), LabelFont).Width;
                g.DrawString(x.Value.ToString(DateLabelFormat), LabelFont, LabelBrush, (float)x.Key - labelWidth / 2, this.Height - GridMargin / 2 - labelHeight / 2);
            }
        }

        public void Clear()
        {
            lock (StockSignals)
            {
                StockSignals.Clear();
            }
        }

        /// <summary>
        /// Observer interface method. Display another StockSignal.
        /// </summary>
        /// <param name="value"></param>
        public void Add(StockSignal value)
        {
            lock (StockSignals)
            {
                StockSignals.Add(value);
            }
        }

        public void Display()
        {
            this.Invalidate();
        }

        public void EnableWCF()
        {
            // Create a WCF Service to listen for output from the output adapter and display on screen
            serviceHost = new ServiceHost(this, new Uri("net.pipe://localhost"));
            serviceHost.AddServiceEndpoint(typeof(IStockGraphCtl), new NetNamedPipeBinding(), "StockGraphCtl");
            serviceHost.Open();
        }

        public void DisableWCF()
        {
            serviceHost.Close();
        }
    }
}
