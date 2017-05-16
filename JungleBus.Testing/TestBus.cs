// <copyright file="TestBus.cs">
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
using JungleBus.Interfaces;

namespace JungleBus.Testing
{
    /// <summary>
    /// Implementation of the bus for unit test purposes
    /// </summary>
    public class TestBus : IBus
    {
        /// <summary>
        /// Collection of published messages
        /// </summary>
        private Dictionary<Type, List<object>> _publishedMessages = new Dictionary<Type, List<object>>();

        /// <summary>
        /// Collection of publish local messages
        /// </summary>
        private Dictionary<Type, List<object>> _publishedLocalMessages = new Dictionary<Type, List<object>>();

        /// <summary>
        /// Resets the sent messages
        /// </summary>
        public void Reset()
        {
            _publishedMessages.Clear();
            _publishedLocalMessages.Clear();
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
        /// Publishes a message to the bus
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="message">Message to publish</param>
        public void Publish<T>(T message)
        {
            Type messageType = typeof(T);
            if (!_publishedMessages.ContainsKey(messageType))
            {
                _publishedMessages[messageType] = new List<object>();
            }

            _publishedMessages[messageType].Add(message);
        }

        /// <summary>
        /// Send a message to the bus's input queue
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="messageBuilder">Function to initialize the message</param>
        public void PublishLocal<T>(Action<T> messageBuilder) where T : new()
        {
            T message = new T();
            if (messageBuilder != null)
            {
                messageBuilder(message);
            }

            PublishLocal(message);
        }

        /// <summary>
        /// Send a message to the bus's input queue
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="message">Message to send</param>
        public void PublishLocal<T>(T message)
        {
            Type messageType = typeof(T);
            if (!_publishedLocalMessages.ContainsKey(messageType))
            {
                _publishedLocalMessages[messageType] = new List<object>();
            }

            _publishedLocalMessages[messageType].Add(message);
        }

        /// <summary>
        /// Verifies that a message of the given type and data was not published
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="verificationMethod">Method used to check the message</param>
        public void VerifyNotPublished<T>(Func<T, bool> verificationMethod)
            where T : class
        {
            VerifyPublished<T>(verificationMethod, 0);
        }

        /// <summary>
        /// Verifies that a message of the given type and data was published
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="verificationMethod">Method used to check the message</param>
        /// <param name="expectedNumberOfTimes">Number of times the message should have been published</param>
        public void VerifyPublished<T>(Func<T, bool> verificationMethod, int expectedNumberOfTimes)
            where T : class
        {
            Type messageType = typeof(T);
            int publishCount = 0;
            if (_publishedMessages.ContainsKey(messageType))
            {
                publishCount = _publishedMessages[messageType].Count(x => verificationMethod(x as T));
            }

            if (publishCount != expectedNumberOfTimes)
            {
                throw new Exception(string.Format("Message of type {0} was expected to be published {1} times but was {2}", messageType, expectedNumberOfTimes, publishCount));
            }
        }
        
        /// <summary>
        /// Verifies that a message of the given type and data was published to the local queue
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="verificationMethod">Method used to check the message</param>
        /// <param name="expectedNumberOfTimes">Number of times the message should have been published</param>
        public void VerifyPublishedLocal<T>(Func<T, bool> verificationMethod, int expectedNumberOfTimes)
            where T : class
        {
            Type messageType = typeof(T);
            int publishCount = 0;
            if (_publishedLocalMessages.ContainsKey(messageType))
            {
                publishCount = _publishedLocalMessages[messageType].Count(x => verificationMethod(x as T));
            }

            if (publishCount != expectedNumberOfTimes)
            {
                throw new Exception(string.Format("Message of type {0} was expected to be published locally {1} times but was {2}", messageType, expectedNumberOfTimes, publishCount));
            }
        }
        
        /// <summary>
        /// Verifies that a message of the given type and data was not published to the local queue
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="verificationMethod">Method used to check the message</param>
        public void VerifyNotPublishedLocal<T>(Func<T, bool> verificationMethod)
            where T : class
        {
            VerifyPublishedLocal<T>(verificationMethod, 0);
        }
    }
}
