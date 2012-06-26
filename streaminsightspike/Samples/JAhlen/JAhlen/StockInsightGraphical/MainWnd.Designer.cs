// StockInsight 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

namespace AdvantIQ.StockInsight
{
    partial class MainWnd
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
            if (disposing)
            {
                var server = application.Server;
                application.Delete();
                server.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWnd));
            this.RunBtn = new System.Windows.Forms.Button();
            this.SourceDpl = new System.Windows.Forms.ComboBox();
            this.QueryDpl = new System.Windows.Forms.ComboBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.stockGraph1 = new AdvantIQ.StockInsight.StockGraphCtl();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // RunBtn
            // 
            this.RunBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RunBtn.Location = new System.Drawing.Point(502, 361);
            this.RunBtn.Name = "RunBtn";
            this.RunBtn.Size = new System.Drawing.Size(75, 23);
            this.RunBtn.TabIndex = 4;
            this.RunBtn.Text = "Run";
            this.RunBtn.UseVisualStyleBackColor = true;
            this.RunBtn.Click += new System.EventHandler(this.RunBtn_Click);
            // 
            // SourceDpl
            // 
            this.SourceDpl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SourceDpl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SourceDpl.FormattingEnabled = true;
            this.SourceDpl.Location = new System.Drawing.Point(318, 363);
            this.SourceDpl.Name = "SourceDpl";
            this.SourceDpl.Size = new System.Drawing.Size(156, 21);
            this.SourceDpl.TabIndex = 3;
            // 
            // QueryDpl
            // 
            this.QueryDpl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.QueryDpl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.QueryDpl.FormattingEnabled = true;
            this.QueryDpl.Location = new System.Drawing.Point(83, 363);
            this.QueryDpl.Name = "QueryDpl";
            this.QueryDpl.Size = new System.Drawing.Size(136, 21);
            this.QueryDpl.TabIndex = 1;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(190, 6);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(136, 21);
            this.comboBox1.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(355, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Run";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.RunBtn_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(248, 366);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Select Input";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 366);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select Query";
            // 
            // stockGraph1
            // 
            this.stockGraph1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.stockGraph1.BackColor = System.Drawing.SystemColors.Window;
            this.stockGraph1.Location = new System.Drawing.Point(12, 12);
            this.stockGraph1.Name = "stockGraph1";
            this.stockGraph1.Size = new System.Drawing.Size(565, 342);
            this.stockGraph1.TabIndex = 5;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // MainWnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 395);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.QueryDpl);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SourceDpl);
            this.Controls.Add(this.RunBtn);
            this.Controls.Add(this.stockGraph1);
            this.MinimumSize = new System.Drawing.Size(605, 431);
            this.Name = "MainWnd";
            this.Text = "StockInsight Graphical Demo";
            this.Load += new System.EventHandler(this.MainWnd_Load);
            this.SizeChanged += new System.EventHandler(this.MainWnd_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private StockGraphCtl stockGraph1;
        private System.Windows.Forms.Button RunBtn;
        private System.Windows.Forms.ComboBox SourceDpl;
        private System.Windows.Forms.ComboBox QueryDpl;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}

