using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Logging;

namespace JungleBus.Messaging
{
    /// <summary>
    /// Responsible for polling SQS queue and dispatching the events
    /// </summary>
    internal sealed class MessagePump : IDisposable
    {
        /// <summary>
        /// Instance of the logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(MessagePump));

        /// <summary>
        /// Message processor, handles calling the event handlers
        /// </summary>
        private readonly IMessageProcessor _messageProcessor;

        /// <summary>
        /// Queue to read messages from
        /// </summary>
        private readonly IMessageQueue _queue;

        /// <summary>
        /// Instance of the bus to pass to the event handlers
        /// </summary>
        private readonly IBus _bus;

        /// <summary>
        /// Token used to control when to stop the pump
        /// </summary>
        private CancellationTokenSource _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePump" /> class.
        /// </summary>
        /// <param name="queue">Queue to read messages from</param>
        /// <param name="messageProcessor">Class for calling out to event handlers</param>
        /// <param name="bus">Instance of the bus to pass to the event handlers</param>
        public MessagePump(IMessageQueue queue, IMessageProcessor messageProcessor, IBus bus)
        {
            _queue = queue;
            _messageProcessor = messageProcessor;
            _cancellationToken = new CancellationTokenSource();
            _bus = bus;
        }

        /// <summary>
        /// Starts the loop for polling the SQS queue
        /// </summary>
        public async void Run()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                Log.Trace("Starting receiving call");
                try
                {
                    IEnumerable<TransportMessage> recievedMessages = await _queue.GetMessages(_cancellationToken.Token);
                    Log.TraceFormat("Received {0} messages", recievedMessages.Count());
                    foreach (TransportMessage message in recievedMessages)
                    {
                        Log.InfoFormat("Received message of type '{0}'", message.MessageTypeName);
                        bool messageErrored = false;
                        if (message.MessageParsingSucceeded)
                        {
                            messageErrored = !_messageProcessor.ProcessMessage(message, _bus);
                        }
                        else
                        {
                            Log.ErrorFormat("Failed to parse message of type {0}", message.Exception, message.MessageTypeName);
                            messageErrored = true;
                        }

                        if (!messageErrored)
                        {
                            Log.Info("Removing message from the queue");
                            _queue.RemoveMessage(message);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    Log.Error("Error occurred in message pump run", ex);
                }
            }
        }

        /// <summary>
        /// Stops the loop for polling the SQS queue
        /// </summary>
        public void Stop()
        {
            _cancellationToken.Cancel();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources
        /// </summary>
        public void Dispose()
        {
            if (_cancellationToken != null)
            {
                _cancellationToken.Dispose();
                _cancellationToken = null;
            }
        }
    }
}
