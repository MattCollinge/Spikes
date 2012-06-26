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
using Microsoft.ComplexEventProcessing;

namespace AdvantIQ.ExampleAdapters
{
    public partial class MainWnd : Form
    {
        public MainWnd()
        {
            InitializeComponent();
        }

        private void MainWnd_Load(object sender, EventArgs e)
        {
            InstanceNameTbx.Text = Properties.Settings.Default.InstanceName;
        }

        private void InstanceNameTbx_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.InstanceName = InstanceNameTbx.Text;
            Properties.Settings.Default.Save();
        }

        private void DetectBtn_Click(object sender, EventArgs e)
        {
            InstanceNameTbx.Text = StreamInsightSetupInfo.EnumerateInstances()[0];
        }

        private void TwitterBtn_Click(object sender, EventArgs e)
        {
            if (!TestInstanceName())
                return;

            var wnd = new TwitterTestWnd();
            wnd.ShowDialog();
        }

        private void FacebookBtn_Click(object sender, EventArgs e)
        {
            if (!TestInstanceName())
                return;

            var wnd = new FaceBookTestWnd();
            wnd.ShowDialog();
        }

        private void YahooFinanceBtn_Click(object sender, EventArgs e)
        {
            if (!TestInstanceName())
                return;

            var wnd = new YahooTestWnd();
            wnd.ShowDialog();
        }

        private bool TestInstanceName()
        {
            try
            {
                var server = Server.Create(Properties.Settings.Default.InstanceName);
                server.Dispose();
                return true;
            }
            catch
            {
                MessageBox.Show(this, "Invalid instance", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
