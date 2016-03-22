using System;
using System.Diagnostics;
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
    public class DiscoProcessor : BaseProcessor
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly IMessageWrapper _messageWrapper;
        private MessageReceiver _receiver;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public DiscoProcessor(IGlobalSettings globalSettings, IMessageWrapper messageWrapper)
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

            _receiver = await _messageWrapper.CreateMessageReceiverAsync(_globalSettings.DiscoTopicName, "Gadgets", ReceiveMode.PeekLock);
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
                Trace.TraceWarning("ProcessMessageAsync was called with a null BrokeredMessage");
                return;
            }

            if (cancellationToken.IsCancellationRequested == false)
            {
                Trace.TraceInformation("Receiving BrokeredMessage with Correlation Id {0}", message.CorrelationId);
                try
                {
                    // Do actual processing of the message
                    var topicMessage = message.GetBody<TopicMessageModel>();
                    Trace.TraceInformation("Processing message for {0}, sent by {1}, disco? {2}", topicMessage.Message, topicMessage.Sender, topicMessage.Disco);
                    // start disco
                    var context = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
                    context.Clients.All.addDisco();

                    // after 10 mins, stop it
                    

                    // Complete the message
                    await message.CompleteAsync();
                }
                catch (MessageLockLostException e)
                {
                    Trace.TraceError("Completing a BrokeredMessage in ProcessMessageAsync throw a MessageLockLostException");
                }
                catch (MessagingException e)
                {
                    Trace.TraceError("Completing a BrokeredMessage in ProcessMessageAsync throw a MessagingException");
                }
                catch (Exception e)
                {
                    Trace.TraceError("An exception occured while trying to process a deployment request message");
                }

                message.Dispose();
            }
        }

        private void OnExceptionReceived(object sender, ExceptionReceivedEventArgs e)
        {
            Trace.TraceError("An exception occured while trying to receive a message from the '{0}' Service Bus Topic", _globalSettings.DiscoTopicName);
        }

        public override void DisposeTheRest()
        {
            Trace.TraceInformation("Disposing the Message processor");

            _cancellationTokenSource.Cancel();

            if (_receiver != null)
            {
                _receiver.Abort();
                _receiver = null;
            }
        }
    }
}
