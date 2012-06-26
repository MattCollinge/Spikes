// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace AdvantIQ.ExampleAdapters
{
    public class StreamInsightSetupInfo
    {
        public const string RegKeyPrefix = "MSSI.";

        public static string[] EnumerateInstances()
        {
            RegistryKey masterKey;

            try
            {
                masterKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft StreamInsight");
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new Exception("Access denied to StreamInsight registry key. Make sure you run with high enough permissions. If executing from Visual Studio you may have to run Visual Studio as administrator.");
            }

            var lst = new List<string>();

            foreach (var key in masterKey.GetSubKeyNames())
            {
                if (key.StartsWith(RegKeyPrefix))
                {
                    lst.Add(key.Substring(RegKeyPrefix.Length));
                }
            }

            return lst.ToArray();
        }

        public static string GetHostPath(string instanceName)
        {
            var masterKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft StreamInsight\" + RegKeyPrefix + instanceName);
            var dumperPath = masterKey.GetValue("StreamInsightDumperPath").ToString();

            var streamInsightRootPath = dumperPath.Substring(0, dumperPath.LastIndexOf('\\'));
            streamInsightRootPath = dumperPath.Substring(0, streamInsightRootPath.LastIndexOf('\\') + 1);

            return streamInsightRootPath + @"Host\";
        }

        public static string GetServiceName(string instanceName)
        {
            return "MSSI$" + instanceName;
        }

        public static string GetEndpointName(string instanceName)
        {
            return "http://localhost/StreamInsight/" + instanceName;
        }
    }
}
