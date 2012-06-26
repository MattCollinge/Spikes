// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvantIQ.ExampleAdapters.Input.Twitter
{
    public enum TwitterMode
    {
        Firehose,
        Sample,
        Filter
    }

    /// <summary>
    /// Configuration for input adapter
    /// </summary>
    public class TwitterConfig
    {
        public TwitterMode Mode { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Parameters { get; set; }

        public int Timeout { get; set; }

        public TwitterConfig()
        {
            Timeout = 300000;
            Mode = TwitterMode.Sample;
            Parameters = "";
        }
    }
}
