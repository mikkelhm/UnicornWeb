using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Unicorn.Web.Contracts.Messaging.ServiceBus
{
    public interface IMessageWrapper
    {
        /// <summary>
        /// Creates a topic if it doesn't already exist
        /// </summary>
        /// <param name="topicName">Name of the topic to create</param>
        /// <param name="sizeInMb">Size of the topic in MB</param>
        /// <param name="timeToLive">Time to live</param>
        void CreateTopicIfDoesntExistSilent(string topicName, long sizeInMb, TimeSpan timeToLive);

        /// <summary>
        /// Creates a topic if it doesn't already exist, and returns a TopicClient to utilize the topic
        /// </summary>
        /// <param name="topicName">Name of the topic to create</param>
        /// <param name="sizeInMb">Size of the topic in MB</param>
        /// <param name="timeToLive">Time to live</param>
        TopicClient CreateTopicIfDoesntExist(string topicName, long sizeInMb, TimeSpan timeToLive);

        ///// <summary>
        ///// Sends a message to the Topic, including the object
        ///// </summary>
        ///// <param name="serializedObject">object to send with the message</param>
        ///// <param name="timeToLive">Time to live</param>
        ///// <param name="topicName">Name of the topic</param>
        ///// <returns></returns>
        //Task SendMessageAsync(object serializedObject, TimeSpan timeToLive, string topicName);

        /// <summary>
        /// Creates a MessageReceiver, which is used as the message pump for a specific Service Bus Topic
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="receiveMode"></param>
        /// <returns></returns>
        Task<MessageReceiver> CreateMessageReceiverAsync(string topicName, string subscriptionName, ReceiveMode receiveMode);
    }
}