// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2010. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace AdvantIQ.ExampleAdapters.Input.Twitter
{
    /// <summary>
    /// Simple input adapter that reads stock quotes
    /// Future development:
    /// - Check that the stock market is open
    /// - Check date/time of the source
    /// </summary>
    public class TwitterInput : TypedPointInputAdapter<Tweet>
    {
        private readonly static IFormatProvider DateFormatProvider = CultureInfo.GetCultureInfo("en-us").DateTimeFormat;
        private const string DateFormatString = "ddd MMM dd HH:mm:ss yyyy";
        private PointEvent<Tweet> pendingEvent;
        private TwitterConfig _config;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Configuration for this adapter</param>
        public TwitterInput(TwitterConfig config)
        {
            _config = config;
        }

        public override void Start()
        {
            ProduceEvents();
        }

        public override void Resume()
        {
            ProduceEvents();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// Main loop
        /// </summary>
        private void ProduceEvents()
        {
            var url = GetURL();
            var currEvent = default(PointEvent<Tweet>);
            var request = HttpWebRequest.Create(url);
            request.Timeout = _config.Timeout;
            
            request.Credentials = new NetworkCredential(_config.Username, _config.Password);
            var response = request.GetResponse();

            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                EnqueueCtiEvent(DateTimeOffset.Now);
                try
                {
                    // Loop until stop signal
                    while (AdapterState != AdapterState.Stopping)
                    {
                        if (pendingEvent != null)
                        {
                            currEvent = pendingEvent;
                            pendingEvent = null;
                        }
                        else
                        {
                            try
                            {
                                // Read from source
                                var line = streamReader.ReadLine();

                                // Parse
                                var jObject = JObject.Parse(line);
                                var tweet = new Tweet();
                                if (jObject["delete"] != null)
                                {
                                    tweet.DeleteStatusID = jObject.SelectToken("delete.status.id").Value<Int64>();
                                    tweet.DeleteUserID = jObject.SelectToken("delete.status.user_id").Value<Int64>();
                                }
                                else
                                {
                                    tweet.ID = jObject.SelectToken("id").Value<Int64>();
                                    tweet.Text = Unquote(jObject.SelectToken("text").Value<string>());
                                    tweet.CreatedAt = ParseTwitterDateTime(jObject.SelectToken("created_at").Value<string>());
                                    tweet.Source = Unquote(jObject.SelectToken("source").Value<string>());
                                    tweet.UserLang = Unquote(jObject.SelectToken("user.lang").Value<string>());
                                    tweet.UserFriendsCount = jObject.SelectToken("user.friends_count").Value<int>();
                                    tweet.UserDescription = Unquote(jObject.SelectToken("user.description").TryGetValue());
                                    tweet.UserTimeZone = Unquote(jObject.SelectToken("user.time_zone").TryGetValue());
                                    tweet.UserURL = Unquote(jObject.SelectToken("user.url").TryGetValue());
                                    tweet.UserName = Unquote(jObject.SelectToken("user.name").Value<string>());
                                    tweet.UserID = jObject.SelectToken("user.id").Value<Int64>();
                                }

                                // Produce INSERT event
                                currEvent = CreateInsertEvent();
                                currEvent.StartTime = DateTimeOffset.Now;
                                currEvent.Payload = tweet;
                                pendingEvent = null;
                                //PrintEvent(currEvent);
                                Enqueue(ref currEvent);

                                // Also send a CTI event
                                EnqueueCtiEvent(DateTimeOffset.Now);

                            }
                            catch (Exception ex)
                            {
                                // Error handling should go here
                            }
                        }
                    }

                    if (pendingEvent != null)
                    {
                        currEvent = pendingEvent;
                        pendingEvent = null;
                    }

                    PrepareToStop(currEvent);
                    Stopped();
                }
                catch (AdapterException e)
                {
                    Console.WriteLine(this.GetType().ToString() + ".ProduceEvents - " + e.Message + e.StackTrace);
                }
            }
        }

        private string GetURL()
        {
            string result;

            switch (_config.Mode)
            {
                case TwitterMode.Sample:
                    result = "https://stream.twitter.com/1/statuses/sample.json";
                    break;
                case TwitterMode.Firehose:
                    result = "https://stream.twitter.com/1/statuses/firehose.json";
                    break;
                case TwitterMode.Filter:
                    result = "https://stream.twitter.com/1/statuses/filter.json";
                    break;
                default:
                    throw new Exception("Invalid TwitterMode");
            }

            if (!string.IsNullOrEmpty(_config.Parameters))
                result += "?" + _config.Parameters;

            return result;
        }

        private DateTime ParseTwitterDateTime(string p)
        {
            p = p.Replace("+0000 ", "");
            DateTimeOffset result;

            if (DateTimeOffset.TryParseExact(p, DateFormatString, DateFormatProvider, DateTimeStyles.AssumeUniversal, out result))
                return result.DateTime;
            else
                return DateTime.Now;
        }

        private void PrepareToStop(PointEvent<Tweet> currEvent)
        {
            //EnqueueCtiEvent(DateTime.Now);
            if (currEvent != null)
            {
                // Do this to avoid memory leaks
                ReleaseEvent(ref currEvent);
            }
        }

        private void PrepareToResume(PointEvent<Tweet> currEvent)
        {
            pendingEvent = currEvent;
        }

        private string Unquote(string str)
        {
            return str.Trim('"');
        }

        /// <summary>
        /// Debugging function
        /// </summary>
        /// <param name="evt"></param>
        private void PrintEvent(PointEvent<Tweet> evt)
        {
        }
    }
}
