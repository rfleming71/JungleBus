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
using JungleBus.Interfaces.Exceptions;
using Newtonsoft.Json;

namespace JungleBus.Queue.Messaging
{
    /// <summary>
    /// Parses out SNS messages from the input queue
    /// </summary>
    internal class MessageParser : IMessageParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageParser" /> class.
        /// </summary>
        public MessageParser()
        {
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

                parsedMessage.Body = message.Body;
                parsedMessage.Published = message.MessageAttributes.ContainsKey("fromSns");

                if (message.MessageAttributes.ContainsKey("messageType"))
                {
                    parsedMessage.MessageTypeName = message.MessageAttributes["messageType"].StringValue;
                }
                else
                {
                    Sns.SnsMessage snsMessage = JsonConvert.DeserializeObject<Sns.SnsMessage>(message.Body);
                    if (snsMessage.MessageAttributes == null || !snsMessage.MessageAttributes.ContainsKey("messageType"))
                    {
                        parsedMessage.MessageParsingSucceeded = false;
                        parsedMessage.Exception = new JungleBusException("Invalid message format");
                    }
                    else
                    {
                        parsedMessage.Body = snsMessage.Message;
                        parsedMessage.Published = true;
                        parsedMessage.MessageTypeName = snsMessage.MessageAttributes["messageType"].Value;
                    }
                }

                parsedMessage.MessageType = Type.GetType(parsedMessage.MessageTypeName, false, true);
                if (parsedMessage.MessageType == null)
                {
                    parsedMessage.MessageParsingSucceeded = false;
                    parsedMessage.Exception = new JungleBusException("Unable to find message type " + parsedMessage.MessageTypeName);
                }
                else
                {
                    parsedMessage.Message = JsonConvert.DeserializeObject(parsedMessage.Body, parsedMessage.MessageType);
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
