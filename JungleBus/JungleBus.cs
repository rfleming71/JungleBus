using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using JungleBus.Configuration;
using JungleBus.Interfaces;
using JungleBus.Interfaces.Serialization;
using JungleBus.Messaging;
using JungleBus.Serialization;

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
        /// The configured message serializer
        /// </summary>
        private readonly IMessageSerializer _messageSerializer;

        /// <summary>
        /// Client for actually sending out messages
        /// </summary>
        private readonly IMessagePublisher _messagePublisher;

        /// <summary>
        /// Receive event message pump
        /// </summary>
        private readonly List<MessagePump> _messagePumps;

        /// <summary>
        /// Tasks running the message pumps
        /// </summary>
        private readonly List<Task> _messagePumpTasks;

        /// <summary>
        /// Local message queue
        /// </summary>
        private readonly IMessageQueue _localQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="JungleBus" /> class.
        /// </summary>
        /// <param name="configuration">Bus configuration settings</param>
        internal JungleBus(IBusConfiguration configuration)
        {
            if (configuration.Send != null)
            {
                _messageSerializer = configuration.ObjectBuilder.GetValue<IMessageSerializer>();
                _messagePublisher = configuration.Send.MessagePublisher;
            }

            if (configuration.Receive != null)
            {
                _localQueue = configuration.Receive.InputQueue;
                MessageProcessor messageProcessor = new MessageProcessor(configuration.ObjectBuilder, configuration.Receive.Handlers, configuration.Receive.FaultHandlers);
                _messagePumps = new List<MessagePump>();
                _messagePumpTasks = new List<Task>();
                for (int x = 0; x < configuration.Receive.NumberOfPollingInstances; ++x)
                {
                    MessagePump pump = new MessagePump(configuration.Receive.InputQueue, configuration.Receive.MessageRetryCount, messageProcessor, configuration.MessageLogger, CreateSendBus(), x + 1);
                    _messagePumps.Add(pump);
                    _messagePumpTasks.Add(new Task(() => pump.Run()));
                }
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

            TransactionalBus sendBus = new TransactionalBus(_messagePublisher, _messageSerializer, _localQueue);
            return sendBus;
        }

        /// <summary>
        /// Starts the bus receiving and processing messages
        /// </summary>
        public void StartReceiving()
        {
            if (_messagePumpTasks == null || !_messagePumpTasks.Any())
            {
                throw new InvalidOperationException("Bus is not configured for receive operations");
            }

            Log.Info("Starting message pumps");
            _messagePumpTasks.ForEach(x => x.Start());
        }

        /// <summary>
        /// Triggers the bus to stop processing new messages
        /// </summary>
        public void StopReceiving()
        {
            Log.Info("Stopping the bus");
            _messagePumps.ForEach(x => x.Stop());
            Task.WaitAll(_messagePumpTasks.ToArray());
            _messagePumps.ForEach(x => x.Dispose());
            _messagePumps.Clear();
            _messagePumpTasks.Clear();
        }
    }
}
