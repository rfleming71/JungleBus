// <copyright file="MessageParser.cs">
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
using Amazon.SQS.Model;
using JungleBus.Exceptions;
using JungleBus.Interfaces.Serialization;

namespace JungleBus.Messaging
{
    /// <summary>
    /// Parses out SNS messages from the input queue
    /// </summary>
    internal class MessageParser : IMessageParser
    {
        /// <summary>
        /// Instance of the message serializer
        /// </summary>
        private readonly IMessageSerializer _messageSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageParser" /> class.
        /// </summary>
        /// <param name="serializer">Instance of the message serializer</param>
        public MessageParser(IMessageSerializer serializer)
        {
            _messageSerializer = serializer;
        }

        /// <summary>
        /// Parse the Amazon SQS message
        /// </summary>
        /// <param name="message">Message to parse</param>
        /// <returns>Parsed message</returns>
        public TransportMessage ParseMessage(Message message)
        {
            TransportMessage parsedMessage = new TransportMessage()
            {
                ReceiptHandle = message.ReceiptHandle
            };

            try
            {
                if (message.Attributes != null && message.Attributes.ContainsKey("ApproximateReceiveCount"))
                {
                    parsedMessage.AttemptNumber = int.Parse(message.Attributes["ApproximateReceiveCount"]);
                }

                SnsMessage snsMessage = _messageSerializer.Deserialize(message.Body, typeof(SnsMessage)) as SnsMessage;
                parsedMessage.Body = snsMessage.Message;

                parsedMessage.MessageTypeName = snsMessage.MessageAttributes["messageType"].Value;
                parsedMessage.MessageType = Type.GetType(parsedMessage.MessageTypeName, false, true);
                if (parsedMessage.MessageType == null)
                {
                    parsedMessage.MessageParsingSucceeded = false;
                    parsedMessage.Exception = new JungleBusException("Unable to find message type " + parsedMessage.MessageTypeName);
                }
                else
                {
                    parsedMessage.Message = _messageSerializer.Deserialize(parsedMessage.Body, parsedMessage.MessageType);
                    parsedMessage.MessageParsingSucceeded = true;
                }
            }
            catch (Exception ex)
            {
                parsedMessage.MessageParsingSucceeded = false;
                parsedMessage.Exception = new JungleBusException("Failed to parse message", ex);
            }

            return parsedMessage;
        }
    }
}
