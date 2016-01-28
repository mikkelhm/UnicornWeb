using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;
using Unicorn.Web.Contracts;
using Unicorn.Web.Contracts.Models;
using Unicorn.Web.Contracts.Services;

namespace Unicorn.Web.WebHooks
{
    public class SlackWebHookHandler : WebHookHandler
    {
        private readonly ITopicService _topicService;
        private readonly IGlobalSettings _globalSettings;

        public SlackWebHookHandler(ITopicService topicService, IGlobalSettings globalSettings)
        {
            _topicService = topicService;
            _globalSettings = globalSettings;
            this.Receiver = SlackWebHookReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string generator, WebHookHandlerContext context)
        {
            // For more information about Slack WebHook payloads, please see 
            // 'https://api.slack.com/outgoing-webhooks'
            NameValueCollection entry = context.GetDataOrDefault<NameValueCollection>();

            // We can trace to see what is going on.
            Trace.WriteLine(entry.ToString());

            // Switch over the IDs we used when configuring this WebHook 
            switch (context.Id)
            {
                case "disco":
                    var text = entry.Get("text");
                    if (string.IsNullOrEmpty(text) == false)
                    {
                        text = text.Trim().ToLowerInvariant();
                        if (_globalSettings.DiscoTriggers.Any(discoTrigger => text.Contains(discoTrigger)))
                        {
                            _topicService.AddMessageToTopic(new TopicMessageModel()
                            {
                                Message = "disco",
                                Sender = entry.Get("user_name")
                            });
                        }
                    }
                    break;
            }
            return Task.FromResult(true);
        }
    }
}
