// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace AdvantIQ.ExampleAdapters.Output.WinFormStacker
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class StackerCtl : Control, IStackerCtl
    {
        public int NumberOfStacks 
        {
            get
            {
                return StackMessages.Length;
            }
            set
            {
                StackMessages = new string[value];
                StackBackgrounds = new Brush[value];
                StackPosition = 0;
                for (int i = 0; i < value; i++)
                    StackBackgrounds[i] = Brushes.White;
            }
        }

        public string PipeName 
        {
            get
            {
                return pipeName;
            }
            set
            {
                pipeName = value;
                if (string.IsNullOrEmpty(pipeName))
                    return;

                if (serviceHost != null && serviceHost.State != CommunicationState.Closed)
                {
                    serviceHost.Close();
                }
                // Create a WCF Service to listen for output from the output adapter and display on screen
                serviceHost = new ServiceHost(this, new Uri("net.pipe://localhost"));
                serviceHost.AddServiceEndpoint(typeof(IStackerCtl), new NetNamedPipeBinding(), PipeName);
                serviceHost.Open();
            }
        }

        private string pipeName = "";
        private string[] StackMessages;
        private Brush[] StackBackgrounds;
        private int StackPosition;
        private Random random = new Random();
        private ServiceHost serviceHost;

        public StackerCtl()
        {
            NumberOfStacks = 4;
            Font = SystemFonts.DefaultFont;
        }

        public void Push(string message)
        {
            StackMessages[StackPosition] = message;
            StackBackgrounds[StackPosition] = new SolidBrush(Color.FromArgb(128 + random.Next(128),
                128 + random.Next(128), 128 + random.Next(128)));

            StackPosition = (StackPosition + 1) % NumberOfStacks;

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                for (int i = 0; i < NumberOfStacks; i++)
                {
                    var y1 = i * this.Height / NumberOfStacks;
                    var y2 = (i + 1) * this.Height / NumberOfStacks;
                    var rect = new RectangleF(0, y1, this.Width, y2 - y1);

                    e.Graphics.FillRectangle(StackBackgrounds[i], rect);
                    e.Graphics.DrawString(StackMessages[i], Font, Brushes.Black, rect);
                }
            }
            catch
            {
                e.Graphics.Clear(Color.White);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        public void Clear()
        {
            for (int i = 0; i < NumberOfStacks; i++)
            {
                StackMessages[i] = "";
                StackBackgrounds[i] = Brushes.White;
            }

            StackPosition = 0;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (serviceHost != null)
                serviceHost.Close();
        }
    }
}
