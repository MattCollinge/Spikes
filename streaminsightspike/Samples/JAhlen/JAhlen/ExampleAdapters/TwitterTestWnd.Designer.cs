// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

namespace AdvantIQ.ExampleAdapters
{
    partial class TwitterTestWnd
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
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.label1 = new System.Windows.Forms.Label();
            this.UserNameTbx = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.PasswordTbx = new System.Windows.Forms.TextBox();
            this.RunBtn = new System.Windows.Forms.Button();
            this.stackerCtl1 = new AdvantIQ.ExampleAdapters.Output.WinFormStacker.StackerCtl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.FilterParametersTbx = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.FilterModeRbn = new System.Windows.Forms.RadioButton();
            this.SampleModeRbn = new System.Windows.Forms.RadioButton();
            this.StopBtn = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Username";
            // 
            // UserNameTbx
            // 
            this.UserNameTbx.Location = new System.Drawing.Point(67, 27);
            this.UserNameTbx.Name = "UserNameTbx";
            this.UserNameTbx.Size = new System.Drawing.Size(140, 20);
            this.UserNameTbx.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Password";
            // 
            // PasswordTbx
            // 
            this.PasswordTbx.Location = new System.Drawing.Point(67, 53);
            this.PasswordTbx.Name = "PasswordTbx";
            this.PasswordTbx.Size = new System.Drawing.Size(140, 20);
            this.PasswordTbx.TabIndex = 3;
            this.PasswordTbx.UseSystemPasswordChar = true;
            // 
            // RunBtn
            // 
            this.RunBtn.Location = new System.Drawing.Point(358, 331);
            this.RunBtn.Name = "RunBtn";
            this.RunBtn.Size = new System.Drawing.Size(75, 23);
            this.RunBtn.TabIndex = 3;
            this.RunBtn.Text = "Run";
            this.RunBtn.UseVisualStyleBackColor = true;
            this.RunBtn.Click += new System.EventHandler(this.RunBtn_Click);
            // 
            // stackerCtl1
            // 
            this.stackerCtl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.stackerCtl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stackerCtl1.Location = new System.Drawing.Point(13, 170);
            this.stackerCtl1.Name = "stackerCtl1";
            this.stackerCtl1.NumberOfStacks = 4;
            this.stackerCtl1.PipeName = null;
            this.stackerCtl1.Size = new System.Drawing.Size(511, 155);
            this.stackerCtl1.TabIndex = 2;
            this.stackerCtl1.Text = "stackerCtl1";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.UserNameTbx);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.PasswordTbx);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(225, 138);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Your Twitter Account";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.FilterParametersTbx);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.FilterModeRbn);
            this.groupBox2.Controls.Add(this.SampleModeRbn);
            this.groupBox2.Location = new System.Drawing.Point(258, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(266, 138);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Mode";
            // 
            // FilterParametersTbx
            // 
            this.FilterParametersTbx.Enabled = false;
            this.FilterParametersTbx.Location = new System.Drawing.Point(10, 95);
            this.FilterParametersTbx.Name = "FilterParametersTbx";
            this.FilterParametersTbx.Size = new System.Drawing.Size(250, 20);
            this.FilterParametersTbx.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(191, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Keywords (e g football,baseball,soccer)";
            // 
            // FilterModeRbn
            // 
            this.FilterModeRbn.AutoSize = true;
            this.FilterModeRbn.Location = new System.Drawing.Point(7, 54);
            this.FilterModeRbn.Name = "FilterModeRbn";
            this.FilterModeRbn.Size = new System.Drawing.Size(47, 17);
            this.FilterModeRbn.TabIndex = 1;
            this.FilterModeRbn.Text = "Filter";
            this.FilterModeRbn.UseVisualStyleBackColor = true;
            this.FilterModeRbn.CheckedChanged += new System.EventHandler(this.FilterModeRbn_CheckedChanged);
            // 
            // SampleModeRbn
            // 
            this.SampleModeRbn.AutoSize = true;
            this.SampleModeRbn.Checked = true;
            this.SampleModeRbn.Location = new System.Drawing.Point(7, 30);
            this.SampleModeRbn.Name = "SampleModeRbn";
            this.SampleModeRbn.Size = new System.Drawing.Size(60, 17);
            this.SampleModeRbn.TabIndex = 0;
            this.SampleModeRbn.TabStop = true;
            this.SampleModeRbn.Text = "Sample";
            this.SampleModeRbn.UseVisualStyleBackColor = true;
            // 
            // StopBtn
            // 
            this.StopBtn.Location = new System.Drawing.Point(449, 331);
            this.StopBtn.Name = "StopBtn";
            this.StopBtn.Size = new System.Drawing.Size(75, 23);
            this.StopBtn.TabIndex = 4;
            this.StopBtn.Text = "Stop";
            this.StopBtn.UseVisualStyleBackColor = true;
            this.StopBtn.Click += new System.EventHandler(this.StopBtn_Click);
            // 
            // TwitterTestWnd
            // 
            this.AcceptButton = this.RunBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(536, 366);
            this.Controls.Add(this.StopBtn);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.stackerCtl1);
            this.Controls.Add(this.RunBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "TwitterTestWnd";
            this.Text = "Twitter Adapter Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TwitterTestWnd_FormClosing);
            this.Load += new System.EventHandler(this.MainWnd_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox UserNameTbx;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox PasswordTbx;
        private System.Windows.Forms.Button RunBtn;
        private AdvantIQ.ExampleAdapters.Output.WinFormStacker.StackerCtl stackerCtl1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox FilterParametersTbx;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton FilterModeRbn;
        private System.Windows.Forms.RadioButton SampleModeRbn;
        private System.Windows.Forms.Button StopBtn;

    }
}