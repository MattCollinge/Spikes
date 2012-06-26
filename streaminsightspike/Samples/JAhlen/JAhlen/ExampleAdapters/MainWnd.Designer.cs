namespace AdvantIQ.ExampleAdapters
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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.InstanceNameTbx = new System.Windows.Forms.TextBox();
            this.DetectBtn = new System.Windows.Forms.Button();
            this.TwitterBtn = new System.Windows.Forms.Button();
            this.FacebookBtn = new System.Windows.Forms.Button();
            this.YahooFinanceBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Instance name";
            // 
            // InstanceNameTbx
            // 
            this.InstanceNameTbx.Location = new System.Drawing.Point(97, 13);
            this.InstanceNameTbx.Name = "InstanceNameTbx";
            this.InstanceNameTbx.Size = new System.Drawing.Size(184, 20);
            this.InstanceNameTbx.TabIndex = 1;
            this.InstanceNameTbx.TextChanged += new System.EventHandler(this.InstanceNameTbx_TextChanged);
            // 
            // DetectBtn
            // 
            this.DetectBtn.Location = new System.Drawing.Point(289, 11);
            this.DetectBtn.Name = "DetectBtn";
            this.DetectBtn.Size = new System.Drawing.Size(75, 23);
            this.DetectBtn.TabIndex = 2;
            this.DetectBtn.Text = "Detect";
            this.DetectBtn.UseVisualStyleBackColor = true;
            this.DetectBtn.Click += new System.EventHandler(this.DetectBtn_Click);
            // 
            // TwitterBtn
            // 
            this.TwitterBtn.Location = new System.Drawing.Point(16, 62);
            this.TwitterBtn.Name = "TwitterBtn";
            this.TwitterBtn.Size = new System.Drawing.Size(348, 55);
            this.TwitterBtn.TabIndex = 3;
            this.TwitterBtn.Text = "Twitter Demo";
            this.TwitterBtn.UseVisualStyleBackColor = true;
            this.TwitterBtn.Click += new System.EventHandler(this.TwitterBtn_Click);
            // 
            // FacebookBtn
            // 
            this.FacebookBtn.Location = new System.Drawing.Point(16, 124);
            this.FacebookBtn.Name = "FacebookBtn";
            this.FacebookBtn.Size = new System.Drawing.Size(348, 55);
            this.FacebookBtn.TabIndex = 4;
            this.FacebookBtn.Text = "Facebook Demo";
            this.FacebookBtn.UseVisualStyleBackColor = true;
            this.FacebookBtn.Click += new System.EventHandler(this.FacebookBtn_Click);
            // 
            // YahooFinanceBtn
            // 
            this.YahooFinanceBtn.Location = new System.Drawing.Point(16, 186);
            this.YahooFinanceBtn.Name = "YahooFinanceBtn";
            this.YahooFinanceBtn.Size = new System.Drawing.Size(348, 55);
            this.YahooFinanceBtn.TabIndex = 5;
            this.YahooFinanceBtn.Text = "Yahoo Finance Demo";
            this.YahooFinanceBtn.UseVisualStyleBackColor = true;
            this.YahooFinanceBtn.Click += new System.EventHandler(this.YahooFinanceBtn_Click);
            // 
            // MainWnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 262);
            this.Controls.Add(this.YahooFinanceBtn);
            this.Controls.Add(this.FacebookBtn);
            this.Controls.Add(this.TwitterBtn);
            this.Controls.Add(this.DetectBtn);
            this.Controls.Add(this.InstanceNameTbx);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainWnd";
            this.Text = "StreamInsight JAhlen samples";
            this.Load += new System.EventHandler(this.MainWnd_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox InstanceNameTbx;
        private System.Windows.Forms.Button DetectBtn;
        private System.Windows.Forms.Button TwitterBtn;
        private System.Windows.Forms.Button FacebookBtn;
        private System.Windows.Forms.Button YahooFinanceBtn;
    }
}