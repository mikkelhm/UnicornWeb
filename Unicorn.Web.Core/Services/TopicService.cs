using System.Threading.Tasks;
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
            Task.Run(() => this.EnsureTopic()).Wait();
        }

        public async Task AddMessageToTopic(TopicMessageModel message)
        {
            TopicClient client = TopicClient.CreateFromConnectionString(_globalSettings.ServiceBusConnectionString,
                _globalSettings.DiscoTopicName);
            //
            var brokeredMessage = new BrokeredMessage(message);
            await client.SendAsync(brokeredMessage);
        }

        private async Task EnsureTopic()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(_globalSettings.ServiceBusConnectionString);

            if (await namespaceManager.TopicExistsAsync(_globalSettings.DiscoTopicName) == false)
            {
                await namespaceManager.CreateTopicAsync(_globalSettings.DiscoTopicName);
            }
            if (await namespaceManager.SubscriptionExistsAsync(_globalSettings.DiscoTopicName, "Gadgets") == false)
            {
                await namespaceManager.CreateSubscriptionAsync(_globalSettings.DiscoTopicName, "Gadgets");
            }
            if (await namespaceManager.SubscriptionExistsAsync(_globalSettings.DiscoTopicName, "Notifications") == false)
            {
                await namespaceManager.CreateSubscriptionAsync(_globalSettings.DiscoTopicName, "Notifications");
            }
        }
    }
}
