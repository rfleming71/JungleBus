using System;
using System.Collections.Generic;
using System.Transactions;
using JungleBus.Messaging;
using JungleBus.Serialization;

namespace JungleBus
{
    /// <summary>
    /// Publishes messages to an SNS topic
    /// </summary>
    internal class TransactionalBus : IBus, IEnlistmentNotification
    {
        /// <summary>
        /// Topic this bus publishes to
        /// </summary>
        private readonly IMessagePublisher _messagePublisher;

        /// <summary>
        /// How to serialize the outbound messages
        /// </summary>
        private readonly IMessageSerializer _messageSerializer;

        /// <summary>
        /// List of messages to public when the transaction completes
        /// </summary>
        private readonly List<KeyValuePair<object, Type>> _transactionalPublishMessages;

        /// <summary>
        /// List of messages to send when the transaction completes
        /// </summary>
        private readonly List<KeyValuePair<object, Type>> _transactionalSendMessages;

        /// <summary>
        /// Instance of the local message queue
        /// </summary>
        private readonly IMessageQueue _localMessageQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionalBus" /> class.
        /// </summary>
        /// <param name="messagePublisher">How to publish messages</param>
        /// <param name="messageSerializer">How to serialize the outbound messages</param>
        /// <param name="messageQueue">Local message queue</param>
        public TransactionalBus(IMessagePublisher messagePublisher, IMessageSerializer messageSerializer, IMessageQueue messageQueue)
        {
            _transactionalPublishMessages = new List<KeyValuePair<object, Type>>();
            _transactionalSendMessages = new List<KeyValuePair<object, Type>>();
            _messagePublisher = messagePublisher;
            _messageSerializer = messageSerializer;
            _localMessageQueue = messageQueue;
        }

        /// <summary>
        /// Publishes a message to the bus
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="message">Message to publish</param>
        public void Publish<T>(T message)
        {
            if (Transaction.Current != null)
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                _transactionalPublishMessages.Add(new KeyValuePair<object, Type>(message, typeof(T)));
            }
            else
            {
                InternalPublish(message, typeof(T));
            }
        }

        /// <summary>
        /// Publishes a message to the bus
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="messageBuilder">Function to initialize the message</param>
        public void Publish<T>(Action<T> messageBuilder) where T : new()
        {
            T message = new T();
            if (messageBuilder != null)
            {
                messageBuilder(message);
            }

            Publish(message);
        }

        /// <summary>
        /// Send a message to the bus's input queue
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="message">Message to send</param>
        public void PublishLocal<T>(T message)
        {
            if (Transaction.Current != null)
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                _transactionalSendMessages.Add(new KeyValuePair<object, Type>(message, typeof(T)));
            }
            else
            {
                InternalSend(message, typeof(T));
            }
        }

        /// <summary>
        /// Send a message to the bus's input queue
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="messageBuilder">Function to initialize the message</param>
        public void PublishLocal<T>(Action<T> messageBuilder) 
            where T : new()
        {
            T message = new T();
            if (messageBuilder != null)
            {
                messageBuilder(message);
            }

            PublishLocal(message);
        }

        /// <summary>
        /// Notifies an enlisted object that a transaction is being committed.
        /// </summary>
        /// <param name="enlistment">An System.Transactions.Enlistment object used to send a response to the transaction manager.</param>
        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            foreach (var message in _transactionalPublishMessages)
            {
                InternalPublish(message.Key, message.Value);
            }
            
            foreach (var message in _transactionalSendMessages)
            {
                InternalSend(message.Key, message.Value);
            }

            _transactionalPublishMessages.Clear();
            _transactionalSendMessages.Clear();
            enlistment.Done();
        }

        /// <summary>
        /// Notifies an enlisted object that the status of a transaction is in doubt.
        /// </summary>
        /// <param name="enlistment">An System.Transactions.Enlistment object used to send a response to the transaction manager.</param>
        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
        }

        /// <summary>
        /// Notifies an enlisted object that a transaction is being prepared for commitment.
        /// </summary>
        /// <param name="preparingEnlistment">A System.Transactions.PreparingEnlistment object used to send a response to the transaction manager</param>
        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        /// <summary>
        /// Notifies an enlisted object that a transaction is being rolled back (aborted).
        /// </summary>
        /// <param name="enlistment">A System.Transactions.Enlistment object used to send a response to the transaction manager.</param>
        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            _transactionalPublishMessages.Clear();
            _transactionalSendMessages.Clear();
        }

        /// <summary>
        /// Performs the actual publishing of a message to the bus
        /// </summary>
        /// <param name="message">Message to publish</param>
        /// <param name="type">Type of the message</param>
        private void InternalPublish(object message, Type type)
        {
            string messageString = _messageSerializer.Serialize(message);
            _messagePublisher.Publish(messageString, type);
        }

        /// <summary>
        /// Performs the actual sending of a message to the bus
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="type">Type of the message</param>
        private void InternalSend(object message, Type type)
        {
            string messageString = _messageSerializer.Serialize(message);
            _messagePublisher.Send(messageString, type, _localMessageQueue);
        }
    }
}
