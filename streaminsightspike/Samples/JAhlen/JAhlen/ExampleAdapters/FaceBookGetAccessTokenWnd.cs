// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2011. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AdvantIQ.ExampleAdapters
{
    public partial class FaceBookGetAccessTokenWnd : Form
    {
        private const string authorizeUrl = "https://graph.facebook.com/oauth/authorize?client_id={0}&redirect_uri=http://www.facebook.com/connect/login_success.html&type=user_agent&display=popup&scope=read_stream,manage_pages";
        private const string successUrl = "http://www.facebook.com/connect/login_success.html#access_token=";
        private string _clientId;
        public string AccessToken;

        public FaceBookGetAccessTokenWnd(string clientId)
        {
            _clientId = clientId;
            InitializeComponent();
        }

        private void FaceBookGetAccessTokenWnd_Load(object sender, EventArgs e)
        {
            webBrowser1.Url = new Uri(authorizeUrl.Replace("{0}", _clientId));
            DialogResult = DialogResult.None;
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            var url = webBrowser1.Url.ToString();
            if (url.StartsWith(successUrl))
            {
                var tmp = url.Substring(successUrl.Length);
                if (tmp.Contains('&'))
                    tmp = tmp.Substring(0, tmp.IndexOf('&'));

                AccessToken = tmp;
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
