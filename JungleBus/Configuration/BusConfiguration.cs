// <copyright file="BusConfiguration.cs">
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
using JungleBus.Interfaces.Configuration;
using JungleBus.Interfaces.IoC;
using JungleBus.Messaging;

namespace JungleBus.Configuration
{
    /// <summary>
    /// Bus configuration settings
    /// </summary>
    internal class BusConfiguration : IConfigureObjectBuilder, IConfigureEventPublishing, IConfigureEventReceiving, IConfigureMessageSerializer
    {
        /// <summary>
        /// Gets or sets the service locator for message handlers 
        /// </summary>
        public IObjectBuilder ObjectBuilder { get; set; }

        /// <summary>
        /// Gets or sets an ID for the entire bus
        /// </summary>
        public string BusName { get; set; }

        /// <summary>
        /// Gets or sets the outbound message settings
        /// </summary>
        public SendConfiguration Send { get; set; }

        /// <summary>
        /// Gets or sets the message logger for the bus
        /// </summary>
        public IMessageLogger MessageLogger { get; set; }

        /// <summary>
        /// Gets or sets the function for formatting topic names
        /// </summary>
        public Func<Type, string> SubscriptionFormatter { get; set; }

        /// <summary>
        /// Gets or sets the input queue configuration
        /// </summary>
        public Queue.QueueConfiguration InputQueueConfiguration { get; set; }
    }
}
