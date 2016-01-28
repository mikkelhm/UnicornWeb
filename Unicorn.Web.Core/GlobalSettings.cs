using System;
using System.Collections.Generic;
using System.Linq;
using Unicorn.Web.Contracts;

namespace Unicorn.Web.Core
{
    public class GlobalSettings : IGlobalSettings
    {
        public GlobalSettings(string serviceBusConnectionString, string discoTopicName, string discoTriggers)
        {
            ServiceBusConnectionString = serviceBusConnectionString;
            DiscoTopicName = discoTopicName;
            DiscoTriggers = discoTriggers.Trim().ToLowerInvariant().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public string ServiceBusConnectionString { get; }
        public string DiscoTopicName { get; }
        public List<string> DiscoTriggers { get; }
    }
}
