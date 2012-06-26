// StreamInsight example adapters 
// - Microsoft StreamInsight application examples
// (C) Johan Åhlén, 2011. Released under Apache License 2.0 (http://www.apache.org/licenses/)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;
using Newtonsoft.Json.Linq;

namespace AdvantIQ.ExampleAdapters.Input.Facebook
{
    public class FacebookInput : TypedPointInputAdapter<FacebookItem>
    {
        private const string urlTemplate = "https://graph.facebook.com/{0}/home?access_token={1}";
        private FacebookConfig _config;
        private HashSet<string> _previousItems = new HashSet<string>();
        private DateTimeOffset _lastCti = DateTimeOffset.MinValue;

        public FacebookInput(FacebookConfig config)
        {
            _config = config;
        }

        public static bool TestParameters(string accessToken, string usernameOrUniqueId)
        {
            try
            {
                ReadItems(accessToken, usernameOrUniqueId, 10000);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void Resume()
        {
            ProduceEvents();
        }

        public override void Start()
        {
            ProduceEvents();
        }

        private void ProduceEvents()
        {
            while (AdapterState != AdapterState.Stopping)
            {
                IEnumerable<FacebookItem> items;

                try
                {
                    items = ReadItems(_config.AccessToken, _config.UsernameOrUniqueId, _config.Timeout);
                }
                catch (Exception)
                {
                    // Error handling should go here...
                    Stopped();
                    return;
                }
                var newItems = items.Where(item => !_previousItems.Contains(item.Id)).OrderBy(item => item.CreatedTime).ToArray();

                foreach (var item in newItems)
                {
                    var evt = CreateInsertEvent();
                    evt.StartTime = item.CreatedTime > _lastCti ? item.CreatedTime : _lastCti;
                    evt.Payload = item;
                    Enqueue(ref evt);
                    _previousItems.Add(item.Id);
                }

                EnqueueCtiEvent(DateTimeOffset.Now);
                _lastCti = DateTimeOffset.Now;

                Thread.Sleep(_config.RefreshPeriod);
            }

            Stopped();
        }

        private static IEnumerable<FacebookItem> ReadItems(string accessToken, string usernameOrUniqueId, int timeout)
        {
            var url = urlTemplate.Replace("{0}", usernameOrUniqueId).Replace("{1}", accessToken);
            var request = WebRequest.Create(url);
            request.Timeout = timeout;

            var response = request.GetResponse();

            try
            {
                var streamReader = new StreamReader(response.GetResponseStream());
                var data = streamReader.ReadToEnd();
                var jObject = JObject.Parse(data);
                var results = new List<FacebookItem>();

                foreach (var msg in jObject["data"])
                {
                    var id = msg["id"].Value<string>();
                    var createdTime = DateTime.Parse(msg["created_time"].Value<string>());
                    var fromid = msg["from"]["id"].Value<string>();
                    var fromname = msg["from"]["name"].Value<string>();
                    var caption = msg["caption"] != null ? msg["caption"].Value<string>() : "";
                    var message = msg["message"] != null ? msg["message"].Value<string>() : "";
                    var description = msg["description"] != null ? msg["description"].Value<string>() : "";
                    var likesCount = msg["likes"] != null && msg["likes"]["count"] != null
                                         ? msg["likes"]["count"].Value<int>()
                                         : 0;
                    var type = msg["type"].Value<string>();

                    results.Add(new FacebookItem
                                    {
                                        Id = id,
                                        CreatedTime = createdTime,
                                        FromId = fromid,
                                        FromName = fromname,
                                        Caption = caption,
                                        Message = message,
                                        Description = description,
                                        LikesCount = likesCount,
                                        ItemType = type
                                    });

                    if (msg["comments"] != null && msg["comments"]["data"] != null)
                    {
                        foreach (var comment in msg["comments"]["data"])
                        {
                            var commentId = comment["id"].Value<string>();
                            var commentMessage = comment["message"].Value<string>();
                            var commentCreatedTime = DateTime.Parse(comment["created_time"].Value<string>());

                            results.Add(new FacebookItem
                                            {
                                                Id = commentId,
                                                CreatedTime = commentCreatedTime,
                                                Message = commentMessage,
                                                ItemType = "comment"
                                            });
                        }
                    }
                }

                return results;
            }
            finally
            {
                response.Close();
            }
        }
    }
}
