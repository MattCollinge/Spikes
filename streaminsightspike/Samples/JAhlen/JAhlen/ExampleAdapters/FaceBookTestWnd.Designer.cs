namespace AdvantIQ.ExampleAdapters
{
    partial class FaceBookTestWnd
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.DeveloperExistingLink = new System.Windows.Forms.LinkLabel();
            this.DeveloperSetupLink = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.GetAccessTokenBtn = new System.Windows.Forms.Button();
            this.ClientIdTbx = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.AccessTokenTbx = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.UsernameOrUniqueIdTbx = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.RunBtn = new System.Windows.Forms.Button();
            this.StopBtn = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.label8 = new System.Windows.Forms.Label();
            this.RefreshIntervalTbx = new System.Windows.Forms.TextBox();
            this.stackerCtl1 = new AdvantIQ.ExampleAdapters.Output.WinFormStacker.StackerCtl();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.DeveloperExistingLink);
            this.groupBox1.Controls.Add(this.DeveloperSetupLink);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.GetAccessTokenBtn);
            this.groupBox1.Controls.Add(this.ClientIdTbx);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.AccessTokenTbx);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(300, 189);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Your account";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(7, 163);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(141, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "(Client ID is same as App ID)";
            // 
            // DeveloperExistingLink
            // 
            this.DeveloperExistingLink.AutoSize = true;
            this.DeveloperExistingLink.Location = new System.Drawing.Point(6, 141);
            this.DeveloperExistingLink.Name = "DeveloperExistingLink";
            this.DeveloperExistingLink.Size = new System.Drawing.Size(250, 13);
            this.DeveloperExistingLink.TabIndex = 8;
            this.DeveloperExistingLink.TabStop = true;
            this.DeveloperExistingLink.Text = "Existing developer - visit Facebook developer portal";
            this.DeveloperExistingLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DeveloperExistingLink_LinkClicked);
            // 
            // DeveloperSetupLink
            // 
            this.DeveloperSetupLink.AutoSize = true;
            this.DeveloperSetupLink.Location = new System.Drawing.Point(6, 128);
            this.DeveloperSetupLink.Name = "DeveloperSetupLink";
            this.DeveloperSetupLink.Size = new System.Drawing.Size(185, 13);
            this.DeveloperSetupLink.TabIndex = 7;
            this.DeveloperSetupLink.TabStop = true;
            this.DeveloperSetupLink.Text = "New developer - register at Facebook";
            this.DeveloperSetupLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DeveloperSetupLink_LinkClicked);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(6, 111);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(91, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Need a Client ID?";
            // 
            // GetAccessTokenBtn
            // 
            this.GetAccessTokenBtn.Location = new System.Drawing.Point(216, 74);
            this.GetAccessTokenBtn.Name = "GetAccessTokenBtn";
            this.GetAccessTokenBtn.Size = new System.Drawing.Size(75, 23);
            this.GetAccessTokenBtn.TabIndex = 5;
            this.GetAccessTokenBtn.Text = "Get token";
            this.GetAccessTokenBtn.UseVisualStyleBackColor = true;
            this.GetAccessTokenBtn.Click += new System.EventHandler(this.GetAccessTokenBtn_Click);
            // 
            // ClientIdTbx
            // 
            this.ClientIdTbx.Location = new System.Drawing.Point(88, 74);
            this.ClientIdTbx.Name = "ClientIdTbx";
            this.ClientIdTbx.Size = new System.Drawing.Size(122, 20);
            this.ClientIdTbx.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Enter Client ID";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(121, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Need an access token?";
            // 
            // AccessTokenTbx
            // 
            this.AccessTokenTbx.Location = new System.Drawing.Point(89, 17);
            this.AccessTokenTbx.Name = "AccessTokenTbx";
            this.AccessTokenTbx.Size = new System.Drawing.Size(187, 20);
            this.AccessTokenTbx.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Access Token";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.RefreshIntervalTbx);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.UsernameOrUniqueIdTbx);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Location = new System.Drawing.Point(320, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(290, 189);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Monitoring";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(7, 77);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(280, 26);
            this.label7.TabIndex = 2;
            this.label7.Text = "Your username can be found/set on the account settings \r\npage in Facebook.";
            // 
            // UsernameOrUniqueIdTbx
            // 
            this.UsernameOrUniqueIdTbx.Location = new System.Drawing.Point(7, 37);
            this.UsernameOrUniqueIdTbx.Name = "UsernameOrUniqueIdTbx";
            this.UsernameOrUniqueIdTbx.Size = new System.Drawing.Size(277, 20);
            this.UsernameOrUniqueIdTbx.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(194, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Your Facebook Username or Unique ID";
            // 
            // RunBtn
            // 
            this.RunBtn.Location = new System.Drawing.Point(453, 343);
            this.RunBtn.Name = "RunBtn";
            this.RunBtn.Size = new System.Drawing.Size(75, 23);
            this.RunBtn.TabIndex = 3;
            this.RunBtn.Text = "Run";
            this.RunBtn.UseVisualStyleBackColor = true;
            this.RunBtn.Click += new System.EventHandler(this.RunBtn_Click);
            // 
            // StopBtn
            // 
            this.StopBtn.Location = new System.Drawing.Point(535, 343);
            this.StopBtn.Name = "StopBtn";
            this.StopBtn.Size = new System.Drawing.Size(75, 23);
            this.StopBtn.TabIndex = 4;
            this.StopBtn.Text = "Stop";
            this.StopBtn.UseVisualStyleBackColor = true;
            this.StopBtn.Click += new System.EventHandler(this.StopBtn_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 128);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(131, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "Refresh Interval (seconds)";
            // 
            // RefreshIntervalTbx
            // 
            this.RefreshIntervalTbx.Location = new System.Drawing.Point(7, 145);
            this.RefreshIntervalTbx.Name = "RefreshIntervalTbx";
            this.RefreshIntervalTbx.Size = new System.Drawing.Size(277, 20);
            this.RefreshIntervalTbx.TabIndex = 4;
            this.RefreshIntervalTbx.Text = "10";
            // 
            // stackerCtl1
            // 
            this.stackerCtl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stackerCtl1.Location = new System.Drawing.Point(13, 209);
            this.stackerCtl1.Name = "stackerCtl1";
            this.stackerCtl1.NumberOfStacks = 4;
            this.stackerCtl1.PipeName = "";
            this.stackerCtl1.Size = new System.Drawing.Size(597, 128);
            this.stackerCtl1.TabIndex = 2;
            this.stackerCtl1.Text = "stackerCtl1";
            // 
            // FaceBookTestWnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 373);
            this.Controls.Add(this.StopBtn);
            this.Controls.Add(this.RunBtn);
            this.Controls.Add(this.stackerCtl1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FaceBookTestWnd";
            this.Text = "Facebook Adapter Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FaceBookTestWnd_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.LinkLabel DeveloperExistingLink;
        private System.Windows.Forms.LinkLabel DeveloperSetupLink;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button GetAccessTokenBtn;
        private System.Windows.Forms.TextBox ClientIdTbx;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox AccessTokenTbx;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox UsernameOrUniqueIdTbx;
        private System.Windows.Forms.Label label6;
        private Output.WinFormStacker.StackerCtl stackerCtl1;
        private System.Windows.Forms.Button RunBtn;
        private System.Windows.Forms.Button StopBtn;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.TextBox RefreshIntervalTbx;
        private System.Windows.Forms.Label label8;

    }
}