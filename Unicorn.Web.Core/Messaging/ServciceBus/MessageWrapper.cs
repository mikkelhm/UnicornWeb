using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Unicorn.Web.Contracts.Messaging.ServiceBus;

namespace Unicorn.Web.Core.Messaging.ServciceBus
{
    public class MessageWrapper : IMessageWrapper
    {
        private readonly NamespaceManager _namespaceManager;
        private readonly MessagingFactory _messagingFactory;

        public MessageWrapper(NamespaceManager namespaceManager, MessagingFactory messagingFactory)
        {
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
        }

        public void CreateTopicIfDoesntExistSilent(string topicName, long sizeInMb, TimeSpan timeToLive)
        {
            if (!_namespaceManager.TopicExists(topicName))
            {
                var availabilityQd = new TopicDescription(topicName)
                {
                    MaxSizeInMegabytes = sizeInMb,
                    DefaultMessageTimeToLive = timeToLive
                };

                _namespaceManager.CreateTopic(availabilityQd);
            }
        }

        public TopicClient CreateTopicIfDoesntExist(string topicName, long sizeInMb, TimeSpan timeToLive)
        {
            if (_namespaceManager.TopicExists(topicName) == false)
            {
                var qd = new TopicDescription(topicName)
                {
                    MaxSizeInMegabytes = sizeInMb,
                    DefaultMessageTimeToLive = timeToLive
                };
                _namespaceManager.CreateTopic(qd);
            }
            return _messagingFactory.CreateTopicClient(topicName);
        }

        //public async Task SendMessageAsync(object serializedObject, TimeSpan timeToLive, string topicName)
        //{
        //    // create a message
        //    var message =
        //        new BrokeredMessage(serializedObject)
        //        {
        //            CorrelationId = Guid.NewGuid().ToString(),
        //            MessageId = Guid.NewGuid().ToString(),
        //            TimeToLive = timeToLive
        //        };
        //    // get the client, ensuring that the queue exists
        //    var client = CreateTopicIfDoesntExist(topicName, 1024, timeToLive);

        //    // send the message
        //    await client.SendAsync(message);
        //}

        public Task<MessageReceiver> CreateMessageReceiverAsync(string topicName, string subscriptionName, ReceiveMode receiveMode)
        {
            return _messagingFactory.CreateMessageReceiverAsync(string.Format("{0}/subscriptions/{1}", topicName, subscriptionName), receiveMode);
        }
    }
}
