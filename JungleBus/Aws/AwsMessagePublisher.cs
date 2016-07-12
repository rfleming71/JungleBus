// <copyright file="AwsMessagePublisher.cs">
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
using Amazon;
using JungleBus.Aws.Sns;
using JungleBus.Exceptions;
using JungleBus.Interfaces.Serialization;
using JungleBus.Messaging;
using JungleBus.Serialization;

namespace JungleBus.Aws
{
    /// <summary>
    /// Controls the actual bus
    /// </summary>
    public class AwsMessagePublisher : IMessagePublisher
    {
        /// <summary>
        /// SNS Client to publish to
        /// </summary>
        private readonly ISnsClient _snsClient;

        /// <summary>
        /// Message serializer
        /// </summary>
        private readonly IMessageSerializer _messageSerializer = new JsonNetSerializer();

        /// <summary>
        /// Message logger
        /// </summary>
        private readonly IMessageLogger _messageLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AwsMessagePublisher" /> class.
        /// </summary>
        /// <param name="snsClient">SNS client to publish to</param>
        /// <param name="messageLogger">Message logger</param>
        public AwsMessagePublisher(ISnsClient snsClient, IMessageLogger messageLogger)
        {
            _snsClient = snsClient;
            _messageLogger = messageLogger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AwsMessagePublisher" /> class.
        /// </summary>
        /// <param name="endpoint">Amazon Endpoint we are connecting to</param>
        /// <param name="messageLogger">Message logger</param>
        public AwsMessagePublisher(RegionEndpoint endpoint, IMessageLogger messageLogger)
        {
            _snsClient = new SnsClient(endpoint);
            _messageLogger = messageLogger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AwsMessagePublisher" /> class.
        /// </summary>
        /// <param name="messageLogger">Message logger</param>
        public AwsMessagePublisher(IMessageLogger messageLogger)
        {
            _snsClient = null;
            _messageLogger = messageLogger;
        }

        /// <summary>
        /// Publishes the serialized message
        /// </summary>
        /// <param name="message">Serialized Message</param>
        /// <param name="type">Payload type</param>
        public void Publish(string message, Type type)
        {
            if (_snsClient == null)
            {
                throw new JungleBusException("Public publishing is disabled");
            }

            _snsClient.Publish(message, type);
            _messageLogger.OutboundLogMessage(message, type.AssemblyQualifiedName);
        }

        /// <summary>
        /// Sends a message to the given queue
        /// </summary>
        /// <param name="messageString">Message to publish</param>
        /// <param name="type">Type of message to send</param>
        /// <param name="localMessageQueue">Queue to send to</param>
        public void Send(string messageString, Type type, IMessageQueue localMessageQueue)
        {
            string messageType = type.AssemblyQualifiedName;
            SnsMessage fakeMessage = new SnsMessage()
            {
                Message = messageString,
                MessageAttributes = new Dictionary<string, MessageAttribute>()
                {
                    { "messageType", new MessageAttribute() { Type = "String", Value = messageType } },
                },
            };

            string messageBody = _messageSerializer.Serialize(fakeMessage);
            localMessageQueue.AddMessage(messageBody);
            _messageLogger.OutboundLogMessage(messageBody, messageType);
        }

        /// <summary>
        /// Setups the bus for publishing the given message types
        /// </summary>
        /// <param name="messageTypes">Message types</param>
        public void SetupMessagesForPublishing(IEnumerable<Type> messageTypes)
        {
            if (_snsClient == null)
            {
                throw new JungleBusException("Public publishing is disabled");
            }

            _snsClient.SetupMessagesForPublishing(messageTypes);
        }
    }
}
