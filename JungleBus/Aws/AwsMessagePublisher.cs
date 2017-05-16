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
using System.Net;
using System.Net.Sockets;
using System.Reflection;
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
        /// Gets the common message metadata
        /// </summary>
        private Dictionary<string, string> _commonMessageMetadata;

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

            _snsClient.Publish(message, type, GetCommonMetadata());
            _messageLogger.OutboundLogMessage(message, type.AssemblyQualifiedName);
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

        /// <summary>
        /// Gets the local machine's IP Address
        /// </summary>
        /// <returns>IP Address</returns>
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

        /// <summary>
        /// Gets metadata common to all messages sent
        /// </summary>
        /// <returns>Metadata values</returns>
        private Dictionary<string, string> GetCommonMetadata()
        {
            if (_commonMessageMetadata == null)
            {
                _commonMessageMetadata = new Dictionary<string, string>()
                {
                    { "BusVersion", typeof(JungleBus).Assembly.GetName().Version.ToString(4) },
                    { "SenderIpAddress", GetLocalIPAddress() },
                };

                AddSenderVersion();
            }

            return _commonMessageMetadata;
        }

        /// <summary>
        /// Adds the sender application version to the common metadata
        /// </summary>
        private void AddSenderVersion()
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                _commonMessageMetadata["SenderVersion"] = entryAssembly.GetName().Version.ToString(4);
            }
        }
    }
}
