// <copyright file="MessageProcessingResult.cs">
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
    /// Result returns from the message processor when processing a message
    /// </summary>
    public class MessageProcessingResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the message was
        /// successfully processed
        /// </summary>
        public bool WasSuccessful { get; set; }
        
        /// <summary>
        /// Gets or sets the exception, if any, from message processing
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the amount of time processing this message
        /// </summary>
        public TimeSpan Runtime { get; set; }
    }
}
