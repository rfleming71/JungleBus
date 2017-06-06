// <copyright file="TransportMessage.cs">
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

namespace JungleBus.Queue.Messaging
{
    /// <summary>
    /// Object for holding parsed message information
    /// </summary>
    public class TransportMessage
    {
        /// <summary>
        /// Gets or sets the message id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the body value
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the message was published to a bus or
        /// send via direct queue calls
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the deserialized message
        /// </summary>
        /// <remarks>Will be null if parsing failed</remarks>
        public object Message { get; set; }

        /// <summary>
        /// Gets or sets the type name passed via the message
        /// </summary>
        public string MessageTypeName { get; set; }

        /// <summary>
        /// Gets or sets the type name passed via the message
        /// </summary>
        public Type MessageType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the message parsing succeeded
        /// </summary>
        public bool MessageParsingSucceeded { get; set; }

        /// <summary>
        /// Gets or sets the exception that was raised during the message parsing
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the message handle
        /// </summary>
        public string ReceiptHandle { get; set; }

        /// <summary>
        /// Gets or sets the number of times this message has already been processed
        /// </summary>
        public int AttemptNumber { get; set; }
    }
}
