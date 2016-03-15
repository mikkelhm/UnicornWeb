using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.ServiceBus.Messaging;
using Unicorn.Web.Contracts;
using Unicorn.Web.Contracts.Messaging.ServiceBus;
using Unicorn.Web.Contracts.Models;
using Unicorn.Web.Contracts.Processes;
using Unicorn.Web.Core.Messaging.Hubs;

namespace Unicorn.Web.Core.Processes
{
    public class MessageProcessor : BaseProcessor
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly IMessageWrapper _messageWrapper;
        private MessageReceiver _receiver;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public MessageProcessor(IGlobalSettings globalSettings, IMessageWrapper messageWrapper)
        {
            _globalSettings = globalSettings;
            _messageWrapper = messageWrapper;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public override async Task StartAsyncCore(CancellationToken cancellationToken)
        {

            cancellationToken.ThrowIfCancellationRequested();

            _messageWrapper.CreateTopicIfDoesntExistSilent(_globalSettings.DiscoTopicName, 1024, new TimeSpan(2, 0, 0, 0));

            var eventDrivenMessagingOptions = new OnMessageOptions { AutoComplete = false, MaxConcurrentCalls = 5 };
            eventDrivenMessagingOptions.ExceptionReceived += OnExceptionReceived;

            _receiver = await _messageWrapper.CreateMessageReceiverAsync(_globalSettings.DiscoTopicName, "Notifications", ReceiveMode.PeekLock);
            _receiver.OnMessageAsync(ProcessMessageAsync, eventDrivenMessagingOptions);
        }

        public override async Task StopAsyncCore(CancellationToken cancellationToken)
        {

            cancellationToken.ThrowIfCancellationRequested();
            await _receiver.CloseAsync();
            _receiver = null;
        }

        private Task ProcessMessageAsync(BrokeredMessage message)
        {
            return ProcessMessageAsync(message, _cancellationTokenSource.Token);
        }

        internal async Task ProcessMessageAsync(BrokeredMessage message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                //LogHelper.Info<DeploymentProcessor>("ProcessMessageAsync was called with a null BrokeredMessage");
                return;
            }

            if (cancellationToken.IsCancellationRequested == false)
            {
                //LogHelper.Info<DeploymentProcessor>(string.Format("Receiving BrokeredMessage with Correlation Id {0}", message.CorrelationId));
                try
                {
                    // Do actual processing of the message
                    var topicMessage = message.GetBody<TopicMessageModel>();
                    var context = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
                    context.Clients.All.addMessage(topicMessage.Sender, topicMessage.Message, topicMessage.Disco);
                    //LogHelper.Info<DeploymentProcessor>(
                    //    string.Format("Starting to deploy from {0} to {1} (Correlation Id {2})",
                    //        deploymentMessage.RepositoryUrl, deploymentMessage.RemoteRepositoryUrl, message.CorrelationId));

                    if (true) // validation
                    {
                        // do stuff to message

                        await message.CompleteAsync();
                    }
                    else
                    {
                        // dead letter the message
                        //await message.DeadLetterAsync("Deployment failed validation", validateResult.ErrorMessage);

                    }
                }
                catch (MessageLockLostException e)
                {
                    //LogHelper.Error<DeploymentProcessor>(
                    //    "Completing a BrokeredMessage in ProcessMessageAsync throw a MessageLockLostException", e);
                }
                catch (MessagingException e)
                {
                    //LogHelper.Error<DeploymentProcessor>(
                    //    "Completing a BrokeredMessage in ProcessMessageAsync throw a MessagingException", e);
                }
                catch (Exception e)
                {
                    //LogHelper.Error<DeploymentProcessor>(
                    //    "An exception occured while trying to process a deployment request message", e);
                }

                message.Dispose();
            }
        }

        private void OnExceptionReceived(object sender, ExceptionReceivedEventArgs e)
        {
            //LogHelper.Error<DeploymentProcessor>(
            //    string.Format(
            //        "An exception occured while trying to receive a message from the '{0}' Service Bus Queue",
            //        _queueName), e.Exception);
        }

        public override void DisposeTheRest()
        {
            //LogHelper.Info<DeploymentProcessor>("Disposing the Deployment processor");

            _cancellationTokenSource.Cancel();

            if (_receiver != null)
            {
                _receiver.Abort();
                _receiver = null;
            }
        }
    }
}
