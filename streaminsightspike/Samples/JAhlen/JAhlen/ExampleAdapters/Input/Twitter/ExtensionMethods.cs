using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace AdvantIQ.ExampleAdapters.Input.Twitter
{
    internal static class ExtensionMethods
    {
        internal static string TryGetValue(this IEnumerable<JToken> obj)
        {
            if (obj != null && ((JToken)obj).HasValues)
                return obj.Value<string>();
            else
                return "";
        }
    }
}
