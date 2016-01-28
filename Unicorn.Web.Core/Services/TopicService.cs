﻿using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Unicorn.Web.Contracts;
using Unicorn.Web.Contracts.Models;
using Unicorn.Web.Contracts.Services;

namespace Unicorn.Web.Core.Services
{
    public class TopicService : ITopicService
    {
        private readonly IGlobalSettings _globalSettings;

        public TopicService(IGlobalSettings globalSettings)
        {
            _globalSettings = globalSettings;
            EnsureTopic().GetAwaiter().GetResult();
        }

        public async Task AddMessageToTopic(TopicMessageModel message)
        {
            TopicClient client = TopicClient.CreateFromConnectionString(_globalSettings.ServiceBusConnectionString,
                _globalSettings.DiscoTopicName);
            var brokeredMessage = new BrokeredMessage(message);
            await client.SendAsync(brokeredMessage);
        }

        private async Task EnsureTopic()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(_globalSettings.ServiceBusConnectionString);

            if (!namespaceManager.TopicExists(_globalSettings.DiscoTopicName))
            {
                await namespaceManager.CreateTopicAsync(_globalSettings.DiscoTopicName);
            }
            if (!namespaceManager.SubscriptionExists(_globalSettings.DiscoTopicName, "Gadgets"))
            {
                await namespaceManager.CreateSubscriptionAsync(_globalSettings.DiscoTopicName, "Gedgets");
            }
            if (!namespaceManager.SubscriptionExists(_globalSettings.DiscoTopicName, "Notifications"))
            {
                await namespaceManager.CreateSubscriptionAsync(_globalSettings.DiscoTopicName, "Notifications");
            }
        }
    }
}