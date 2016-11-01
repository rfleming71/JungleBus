// <copyright file="MessagePump.cs">
//     The MIT License (MIT)
//
// Copyright(c) 2016 Ryan Fleming
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// </copyright>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Logging;
using JungleBus.Interfaces;

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
        /// Number of times to retry a message
        /// </summary>
        private readonly int _messageRetryCount;

        /// <summary>
        /// Message logger
        /// </summary>
        private readonly IMessageLogger _messageLogger;

        /// <summary>
        /// Token used to control when to stop the pump
        /// </summary>
        private CancellationTokenSource _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePump" /> class.
        /// </summary>
        /// <param name="queue">Queue to read messages from</param>
        /// <param name="messageRetryCount">Number of times to retry a message</param>
        /// <param name="messageProcessor">Class for calling out to event handlers</param>
        /// <param name="messageLogger">Instance of the message logger</param>
        /// <param name="bus">Instance of the bus to pass to the event handlers</param>
        /// <param name="id">Id of the message pump</param>
        public MessagePump(IMessageQueue queue, int messageRetryCount, IMessageProcessor messageProcessor, IMessageLogger messageLogger, IBus bus, int id)
        {
            _queue = queue;
            _messageRetryCount = messageRetryCount;
            _messageProcessor = messageProcessor;
            _cancellationToken = new CancellationTokenSource();
            _bus = bus;
            _messageLogger = messageLogger;
        }

        /// <summary>
        /// Gets the id of the message pump
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Starts the loop for polling the SQS queue
        /// </summary>
        public async void Run()
        {
            Log.InfoFormat("[{0}] Starting message pump", Id);
            while (!_cancellationToken.IsCancellationRequested)
            {
                Log.TraceFormat("[{0}] Starting receiving call", Id);
                try
                {
                    IEnumerable<TransportMessage> recievedMessages = await _queue.GetMessages(_cancellationToken.Token);
                    Log.TraceFormat("[{1}] Received {0} messages", recievedMessages.Count(), Id);
                    foreach (TransportMessage message in recievedMessages)
                    {
                        Log.InfoFormat("[{1}] Received message of type '{0}'", message.MessageTypeName, Id);
                        _messageLogger.InboundLogMessage(message.Body, message.MessageTypeName, message.Id, message.AttemptNumber);
                        MessageProcessingResult result;
                        if (message.MessageParsingSucceeded)
                        {
                            Log.TraceFormat("[{0}] Processing message", Id);
                            result = _messageProcessor.ProcessMessage(message, _bus);
                            Log.TraceFormat("[{0}] Processed message - Error: {1}", Id, !result.WasSuccessful);
                        }
                        else
                        {
                            Log.ErrorFormat("[{1}] Failed to parse message of type {0}", message.Exception, message.MessageTypeName, Id);
                            result = new MessageProcessingResult() { WasSuccessful = false, Exception = new Exception("Message parse failure") };
                        }

                        if (result.WasSuccessful)
                        {
                            Log.InfoFormat("[{0}] Removing message from the queue", Id);
                            _queue.RemoveMessage(message);
                        }
                        else if (message.AttemptNumber == _messageRetryCount)
                        {
                            Log.InfoFormat("[{0}] Message faulted ", Id);
                            _messageProcessor.ProcessFaultedMessage(message, _bus, result.Exception);
                        }

                        MessageStatistics stats = new MessageStatistics()
                        {
                            FinalAttempt = message.AttemptNumber == _messageRetryCount,
                            HandlerRunTime = result.Runtime,
                            MessageLength = message.Body.Length,
                            MessageType = message.MessageTypeName,
                            Success = result.WasSuccessful,
                            PreviousRetryCount = message.AttemptNumber,
                        };
                        _messageProcessor.ProcessMessageStatistics(stats);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("[{0}] Error occurred in message pump run", ex, Id);
                }
            }
        }

        /// <summary>
        /// Stops the loop for polling the SQS queue
        /// </summary>
        public void Stop()
        {
            Log.InfoFormat("[{0}] Stop requested", Id);
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
