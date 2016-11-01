// <copyright file="IMessageLogger.cs">
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
namespace JungleBus.Messaging
{
    /// <summary>
    /// Logs messages flowing in and out of the bus
    /// </summary>
    public interface IMessageLogger
    {
        /// <summary>
        /// Logs messages received by the bus
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <param name="messageType">Message type</param>
        /// <param name="messageId">Id of the message</param>
        /// <param name="attemptNumber">Number of times this message has been processed</param>
        void InboundLogMessage(string messageBody, string messageType, string messageId, int attemptNumber);

        /// <summary>
        /// Logs messages being sent by the bus
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <param name="messageType">Message type</param>
        void OutboundLogMessage(string messageBody, string messageType);
    }
}