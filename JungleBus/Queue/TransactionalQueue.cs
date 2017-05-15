// <copyright file="TransactionalQueue.cs">
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
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Transactions;
using Common.Logging;
using JungleBus.Aws.Sqs;
using JungleBus.Interfaces;
using Newtonsoft.Json;

namespace JungleBus
{
    public class TransactionalQueue : IQueue, IEnlistmentNotification
    {
        /// <summary>
        /// Instance of the logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(TransactionalQueue));

        /// <summary>
        /// List of messages to send when the transaction completes
        /// </summary>
        private readonly List<KeyValuePair<object, Type>> _transactionalMessages;

        /// <summary>
        /// The underlying SQS queue
        /// </summary>
        private readonly ISqsQueue _queue;

        /// <summary>
        /// Gets the common message metadata
        /// </summary>
        private Dictionary<string, string> _commonMessageMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionalQueue" /> class.
        /// </summary>
        /// <param name="queue">Underlying SQS queue</param>
        public TransactionalQueue(ISqsQueue queue)
        {
            _queue = queue;
        }

        /// <summary>
        /// Send a message to the bus's input queue
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="message">Message to send</param>
        public void Send<T>(T message)
        {
            if (Transaction.Current != null)
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                _transactionalMessages.Add(new KeyValuePair<object, Type>(message, typeof(T)));
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
        public void Send<T>(Action<T> messageBuilder)
            where T : new()
        {
            T message = new T();
            if (messageBuilder != null)
            {
                messageBuilder(message);
            }

            Send(message);
        }

        public void Commit(Enlistment enlistment)
        {
            Log.Trace("Committing transaction");

            foreach (var message in _transactionalMessages)
            {
                InternalSend(message.Key, message.Value);
            }

            _transactionalMessages.Clear();
            enlistment.Done();
            Log.Trace("Committed transaction");
        }

        public void InDoubt(Enlistment enlistment)
        {
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Rollback(Enlistment enlistment)
        {
            Log.Trace("Transaction rolled back");
            _transactionalMessages.Clear();
        }

        /// <summary>
        /// Performs the actual sending of a message to the bus
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="type">Type of the message</param>
        private void InternalSend(object message, Type type)
        {
            Log.TraceFormat("Sending message of type {0}", type);
            string messageString = JsonConvert.SerializeObject(message);
            List<KeyValuePair<string, string>> metadata = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("messageType", type.AssemblyQualifiedName)
            };
            metadata.AddRange(GetCommonMetadata());
            _queue.AddMessage(messageString, metadata);
            Log.TraceFormat("Sending message of type {0}", type);
        }

        private IEnumerable<KeyValuePair<string, string>> GetCommonMetadata()
        {
            if (_commonMessageMetadata == null)
            {
                _commonMessageMetadata = new Dictionary<string, string>()
                {
                    { "QueueVersion", typeof(TransactionalQueue).Assembly.GetName().Version.ToString(4) },
                    { "SenderIpAddress", GetLocalIPAddress() },
                };

                AddSenderVersion();
            }

            return _commonMessageMetadata;
        }

        private void AddSenderVersion()
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                _commonMessageMetadata["SenderVersion"] = entryAssembly.GetName().Version.ToString(4);
            }
        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            if (host != null)
            {
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }

            return "0.0.0.0";
        }
    }
}
