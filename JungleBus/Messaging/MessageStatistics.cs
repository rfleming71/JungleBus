// <copyright file="MessageProcessor.cs">
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
using JungleBus.Interfaces.Statistics;

namespace JungleBus.Messaging
{
    /// <summary>
    /// Contains statistics about the completed message
    /// </summary>
    internal class MessageStatistics : IMessageStatistics
    {
        /// <summary>
        /// Gets a value indicating whether this the final attempt to process
        /// this message
        /// </summary>
        public bool FinalAttempt { get; set; }

        /// <summary>
        /// Gets the run time of the message 
        /// </summary>
        public TimeSpan HandlerRunTime { get; set; }

        /// <summary>
        /// Gets the number of bytes in the message
        /// </summary>
        public int MessageLength { get; set; }

        /// <summary>
        /// Gets the type of the message
        /// </summary>
        public string MessageType { get; set; }

        /// <summary>
        /// Get the number of times this message has been tried before this
        /// attempt
        /// </summary>
        public int PreviousRetryCount { get; set; }

        /// <summary>
        /// Gets a value indicating whether this message was successful
        /// </summary>
        public bool Success { get; set; }
    }
}
