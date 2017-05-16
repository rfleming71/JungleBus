// <copyright file="JungleQueue.cs">
//     The MIT License (MIT)
//
// Copyright(c) 2017 Ryan Fleming
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
using System.Threading.Tasks;
using Common.Logging;
using JungleBus.Aws.Sqs;
using JungleBus.Interfaces;
using JungleBus.Interfaces.IoC;
using JungleBus.Queue.Messaging;

namespace JungleBus.Queue
{
    /// <summary>
    /// Main application queue for sending and receiving messages from AWS
    /// </summary>
    public class JungleQueue
    {
        /// <summary>
        /// Logger instance
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(JungleQueue));

        /// <summary>
        /// SQS queue
        /// </summary>
        private readonly ISqsQueue _queue;

        /// <summary>
        /// Receive event message pump
        /// </summary>
        private readonly List<MessagePump> _messagePumps;

        /// <summary>
        /// Tasks running the message pumps
        /// </summary>
        private readonly List<Task> _messagePumpTasks;

        /// <summary>
        /// Initializes a new instance of the <see cref="JungleQueue" /> class.
        /// </summary>
        /// <param name="configuration">Configuration object</param>
        /// <param name="objectBuilder">Object builder</param>
        /// <param name="preHandler">Function called before the handler is invoked</param>
        public JungleQueue(QueueConfiguration configuration, IObjectBuilder objectBuilder, Action<IObjectBuilder> preHandler = null)
        {
            Action<IObjectBuilder> queuePreHandler = x =>
            {
                x.RegisterInstance(CreateQueue());
                if (preHandler != null)
                {
                    preHandler(x);
                }
            };
            _queue = new SqsQueue(configuration.Region, configuration.QueueName, configuration.RetryCount);
            MessageProcessor messageProcessor = new MessageProcessor(configuration.Handlers, configuration.FaultHandlers, objectBuilder, queuePreHandler);
            if (configuration.NumberOfPollingInstances > 0)
            {
                _messagePumps = new List<MessagePump>();
                _messagePumpTasks = new List<Task>();
                for (int x = 0; x < configuration.NumberOfPollingInstances; ++x)
                {
                    MessagePump pump = new MessagePump(_queue, configuration.RetryCount, messageProcessor, x + 1);
                    _messagePumps.Add(pump);
                    _messagePumpTasks.Add(new Task(() => pump.Run()));
                }
            }
        }

        /// <summary>
        /// Gets an instance of the queue that can send messages
        /// </summary>
        /// <returns>Instance of the queue</returns>
        public IQueue CreateQueue()
        {
            return new TransactionalQueue(_queue);
        }

        /// <summary>
        /// Subscribe the queue to the given sns topics
        /// </summary>
        /// <param name="snsTopics">Message topics to subscribe to</param>
        public void Subscribe(IEnumerable<string> snsTopics)
        {
            _queue.Subscribe(snsTopics);
        }

        /// <summary>
        /// Starts the queue receiving and processing messages
        /// </summary>
        public void StartReceiving()
        {
            if (_messagePumpTasks == null || !_messagePumpTasks.Any())
            {
                throw new InvalidOperationException("Queue is not configured for receive operations");
            }

            Log.Info("Starting message pumps");
            _messagePumpTasks.ForEach(x => x.Start());
        }

        /// <summary>
        /// Triggers the bus to stop processing new messages
        /// </summary>
        public void StopReceiving()
        {
            Log.Info("Stopping the queue");
            _messagePumps.ForEach(x => x.Stop());
            Task.WaitAll(_messagePumpTasks.ToArray());
            _messagePumps.ForEach(x => x.Dispose());
            _messagePumps.Clear();
            _messagePumpTasks.Clear();
        }
    }
}
