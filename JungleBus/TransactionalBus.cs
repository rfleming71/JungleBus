// <copyright file="TransactionalBus.cs">
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
using System.Transactions;
using Common.Logging;
using JungleBus.Interfaces;
using JungleBus.Interfaces.Serialization;
using JungleBus.Messaging;
using Newtonsoft.Json;

namespace JungleBus
{
    /// <summary>
    /// Publishes messages to an SNS topic
    /// </summary>
    internal class TransactionalBus : IBus, IEnlistmentNotification
    {
        /// <summary>
        /// Instance of the logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(IBus));

        /// <summary>
        /// Topic this bus publishes to
        /// </summary>
        private readonly IMessagePublisher _messagePublisher;

        /// <summary>
        /// List of messages to public when the transaction completes
        /// </summary>
        private readonly List<KeyValuePair<object, Type>> _transactionalPublishMessages;

        /// <summary>
        /// Instance of the local message queue
        /// </summary>
        private readonly IQueue _localMessageQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionalBus" /> class.
        /// </summary>
        /// <param name="messagePublisher">How to publish messages</param>
        /// <param name="messageSerializer">How to serialize the outbound messages</param>
        /// <param name="messageQueue">Local message queue</param>
        public TransactionalBus(IMessagePublisher messagePublisher, IQueue messageQueue)
        {
            _transactionalPublishMessages = new List<KeyValuePair<object, Type>>();
            _messagePublisher = messagePublisher;
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
            _localMessageQueue.Send(message);
        }

        /// <summary>
        /// Send a message to the bus's input queue
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="messageBuilder">Function to initialize the message</param>
        public void PublishLocal<T>(Action<T> messageBuilder) 
            where T : new()
        {
            _localMessageQueue.Send(messageBuilder);
        }

        /// <summary>
        /// Notifies an enlisted object that a transaction is being committed.
        /// </summary>
        /// <param name="enlistment">An System.Transactions.Enlistment object used to send a response to the transaction manager.</param>
        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            Log.Trace("Committing transaction");
            foreach (var message in _transactionalPublishMessages)
            {
                InternalPublish(message.Key, message.Value);
            }

            _transactionalPublishMessages.Clear();
            enlistment.Done();
            Log.Trace("Committed transaction");
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
            Log.Trace("Transaction rolled back");
            _transactionalPublishMessages.Clear();
        }

        /// <summary>
        /// Performs the actual publishing of a message to the bus
        /// </summary>
        /// <param name="message">Message to publish</param>
        /// <param name="type">Type of the message</param>
        private void InternalPublish(object message, Type type)
        {
            Log.TraceFormat("Publishing message of type {0}", type);
            string messageString = JsonConvert.SerializeObject(message);
            _messagePublisher.Publish(messageString, type);
            Log.TraceFormat("Published message of type {0}", type);
        }
    }
}
