﻿// <copyright file="ReceiveConfiguration.cs">
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
using JungleBus.Messaging;

namespace JungleBus.Configuration
{
    /// <summary>
    /// Configuration information for the receive bus
    /// </summary>
    public class ReceiveConfiguration
    {
        /// <summary>
        /// Gets or sets the queue to receive events on
        /// </summary>
        public IMessageQueue InputQueue { get; set; }

        /// <summary>
        /// Gets or sets the number of times to attempt to process a message
        /// </summary>
        public int MessageRetryCount { get; set; }

        /// <summary>
        /// Gets or sets the number of polling instances to run
        /// </summary>
        public int NumberOfPollingInstances { get; set; }

        /// <summary>
        /// Gets or sets the collection of message handlers organized by message type
        /// </summary>
        internal Dictionary<Type, HashSet<Type>> Handlers { get; set; }

        /// <summary>
        /// Gets or sets the collection of message fault handlers organized by message type
        /// </summary>
        internal Dictionary<Type, HashSet<Type>> FaultHandlers { get; set; }
    }
}
