// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2011. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvantIQ.ExampleAdapters.Input.Facebook
{
    public class FacebookConfig
    {
        public string AccessToken;
        public string UsernameOrUniqueId;
        public int Timeout = 60000;
        public int RefreshPeriod = 10000;
    }
}
