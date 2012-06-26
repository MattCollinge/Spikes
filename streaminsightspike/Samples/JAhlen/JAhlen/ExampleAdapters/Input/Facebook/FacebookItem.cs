// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2011. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvantIQ.ExampleAdapters.Input.Facebook
{
    public class FacebookItem
    {
        public string Id = "";
        public DateTime CreatedTime;
        public string FromId = "";
        public string FromName = "";
        public string Caption = "";
        public string Message = "";
        public string Description = "";
        public int LikesCount;
        public string ItemType = "";
    }
}
