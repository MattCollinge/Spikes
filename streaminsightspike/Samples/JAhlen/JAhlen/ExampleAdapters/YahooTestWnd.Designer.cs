// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

namespace AdvantIQ.ExampleAdapters
{
    partial class YahooTestWnd
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.stackerCtl1 = new AdvantIQ.ExampleAdapters.Output.WinFormStacker.StackerCtl();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // stackerCtl1
            // 
            this.stackerCtl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.stackerCtl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stackerCtl1.Location = new System.Drawing.Point(13, 13);
            this.stackerCtl1.Name = "stackerCtl1";
            this.stackerCtl1.NumberOfStacks = 4;
            this.stackerCtl1.PipeName = null;
            this.stackerCtl1.Size = new System.Drawing.Size(513, 241);
            this.stackerCtl1.TabIndex = 0;
            this.stackerCtl1.Text = "stackerCtl1";
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // YahooTestWnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 266);
            this.Controls.Add(this.stackerCtl1);
            this.Name = "YahooTestWnd";
            this.Text = "Yahoo Adapter Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.YahooTestWnd_FormClosing);
            this.Load += new System.EventHandler(this.YahooTestWnd_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private AdvantIQ.ExampleAdapters.Output.WinFormStacker.StackerCtl stackerCtl1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}