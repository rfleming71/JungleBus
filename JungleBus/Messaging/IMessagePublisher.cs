// <copyright file="IMessagePublisher.cs">
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

namespace JungleBus.Messaging
{
    /// <summary>
    /// Controls the actual bus
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Setups the bus for publishing the given message types
        /// </summary>
        /// <param name="messageTypes">Message types</param>
        void SetupMessagesForPublishing(IEnumerable<Type> messageTypes);

        /// <summary>
        /// Publishes the serialized message
        /// </summary>
        /// <param name="message">Serialized Message</param>
        /// <param name="type">Payload type</param>
        void Publish(string message, Type type);

        /// <summary>
        /// Sends a message to the given queue
        /// </summary>
        /// <param name="messageString">Message to publish</param>
        /// <param name="type">Type of message to send</param>
        /// <param name="localMessageQueue">Queue to send to</param>
        void Send(string messageString, Type type, IMessageQueue localMessageQueue);
    }
}
