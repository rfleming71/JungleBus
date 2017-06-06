// <copyright file="JungleBus.cs">
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
using System.Threading.Tasks;
using Common.Logging;
using JungleBus.Configuration;
using JungleBus.Interfaces;
using JungleBus.Interfaces.IoC;
using JungleBus.Interfaces.Serialization;
using JungleBus.Messaging;
using JungleBus.Queue;

namespace JungleBus
{
    /// <summary>
    /// Main application bus for receiving messages from AWS
    /// </summary>
    internal class JungleBus : IRunJungleBus
    {
        /// <summary>
        /// Logger instance
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(JungleBus));

        /// <summary>
        /// Client for actually sending out messages
        /// </summary>
        private readonly IMessagePublisher _messagePublisher;

        /// <summary>
        /// Local message queue
        /// </summary>
        private readonly JungleQueue _localQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="JungleBus" /> class.
        /// </summary>
        /// <param name="configuration">Bus configuration settings</param>
        internal JungleBus(IBusConfiguration configuration)
        {
            if (configuration.Send != null)
            {
                _messagePublisher = configuration.Send.MessagePublisher;
            }

            if (configuration.InputQueueConfiguration != null)
            {
                configuration.InputQueueConfiguration.MessageLogger = configuration.MessageLogger;
                Action<IObjectBuilder> preHandler = x => x.RegisterInstance(CreateSendBus());
                _localQueue = new JungleQueue(configuration.InputQueueConfiguration, configuration.ObjectBuilder, preHandler);
                _localQueue.Subscribe(configuration.InputQueueConfiguration.Handlers.Keys.Select(x => configuration.SubscriptionFormatter(x)));
            }
        }

        /// <summary>
        /// Gets an instance of the bus that can publish messages
        /// </summary>
        /// <returns>Instance of the bus</returns>
        public IBus CreateSendBus()
        {
            if (_messagePublisher == null)
            {
                return null;
            }

            TransactionalBus sendBus = new TransactionalBus(_messagePublisher, _localQueue.CreateQueue());
            return sendBus;
        }

        /// <summary>
        /// Starts the bus receiving and processing messages
        /// </summary>
        public void StartReceiving()
        {
            if (_localQueue == null)
            {
                throw new InvalidOperationException("Bus is not configured for receive operations");
            }

            Log.Info("Starting queue receive");
            _localQueue.StartReceiving();
        }

        /// <summary>
        /// Triggers the bus to stop processing new messages
        /// </summary>
        public void StopReceiving()
        {
            Log.Info("Stopping the queue");
            _localQueue.StopReceiving();
        }
    }
}
