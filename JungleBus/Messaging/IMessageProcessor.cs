// <copyright file="IMessageProcessor.cs">
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
using JungleBus.Interfaces;
using JungleBus.Interfaces.Statistics;

namespace JungleBus.Messaging
{
    /// <summary>
    /// Processes inbound messages and call the event handlers
    /// </summary>
    public interface IMessageProcessor
    {
        /// <summary>
        /// Processes inbound message and call the event handlers
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="busInstance">Instance of the bus to pass to event handlers</param>
        /// <returns>True is all event handles succeeded</returns>
        MessageProcessingResult ProcessMessage(TransportMessage message, IBus busInstance);

        /// <summary>
        /// Processes inbound message that have faulted more than the retry limit
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="busInstance">Instance of the bus to pass to event handlers</param>
        /// <param name="ex">Exception thrown by the message</param>
        void ProcessFaultedMessage(TransportMessage message, IBus busInstance, Exception ex);

        /// <summary>
        /// Processes inbound message statistics
        /// </summary>
        /// <param name="statistics">Message statistics</param>
        void ProcessMessageStatistics(IMessageStatistics statistics);
    }
}