// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ComplexEventProcessing;

namespace AdvantIQ.ExampleAdapters.Input.Twitter
{
    /// <summary>
    /// Payload class for stock quote events, see StreamInsight
    /// documentation for info about payload.
    /// </summary>
    public class Tweet
    {
        public Int64 ID { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Text { get; set; }
        public string Source { get; set; }
        public string UserLang { get; set; }
        public int UserFriendsCount { get; set; }
        public string UserDescription { get; set; }
        public string UserTimeZone { get; set; }
        public string UserURL { get; set; }
        public string UserName { get; set; }
        public Int64 UserID { get; set; }
        public Int64 DeleteStatusID { get; set; }
        public Int64 DeleteUserID { get; set; }
    }
}
